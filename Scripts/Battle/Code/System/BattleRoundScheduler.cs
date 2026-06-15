/// <summary>
/// 正式的CTB/ 双列表回合调度器。
/// 
/// 这个类的职责非常明确：
/// 1.维护“当前回合剩余动列表”和“下一回合预测列表”；
/// 2.决定下一位真正能行动的 BattleEntity；
/// 3.把结果整理成时间轴UI能直接使用的预测节点。
/// 
/// 教程里可以把它理解成：
/// “只负责排顺序，不负责播放UI和命令执行”的纯调度层。
/// </summary>
public class BattleRoundScheduler
{
    private List<BattleEntity> _currentRound = new();
    private List<BattleEntity> _nextRound = new();
    
    // 记录"已从next队列剔除"的破盾单位
    // 这些单位会在下一回合开始时消费一次Break跳过计数，避免提前恢复
    private readonly Dictionary<BattleEntity, int> _pendingNextRoundBreakSkip = new();

    /* ---------------------------------------------------------------------------- */

    public void Initialize(List<BattleEntity> allEntities)
    {
        _currentRound = GenerateSortedOrder(allEntities);
        _nextRound = GenerateSortedOrder(allEntities);
        TriggerRoundStart(allEntities);
    }
    
    ///<summary>
    /// 获取下一位真正要行动的单位。
    /// 这个口只做“取人”和“跳过不可动者”，
    /// 不负责命令输入、AI、表现播放。
    /// </summary>
    public BattleEntity GetNextActor(List<BattleEntity> allEntities)
    {
        // 1.场上已经没有活人时，就不再继续取行动者。
        int aliveCount = 0;
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].IsAlive)
                aliveCount++;
        }

        if (aliveCount < 0)
            return null;
        
        // 2. 加一层guard，避免极端情况下死循环.
        int guard = aliveCount * 4;
        while (guard-- > 0)
        {
            // 3. 当前回合已经取空了，就推进到下一回合。
            if (_currentRound.Count == 0)
            {
                StartNextRound(allEntities);
                if (_currentRound.Count == 0)
                    continue; // 下一回合也没有活人，继续guard循环，最终返回null。
            }
            BattleEntity candidate = _currentRound[0];
            _currentRound.RemoveAt(0);
            if (!candidate.IsAlive)
                continue; // 跳过死者
            return candidate;
        }

        return null;
    }

    private void StartNextRound(List<BattleEntity> allEntities)
    {
        // 先把下一回合顶上来，变成新的当前回合。
        _currentRound = _nextRound;
        // 再重新生成一份新的下一回合预测
        _nextRound = GenerateSortedOrder(allEntities);
        TriggerRoundStart(allEntities);
    }

    private List<BattleEntity> GenerateSortedOrder(List<BattleEntity> allEntities)
    {
        List<BattleEntity> result = new();
        for (int i = 0; i < allEntities.Count; i++)
        {
            if (allEntities[i].IsAlive) 
                result.Add(allEntities[i]);
        }
        result.Sort((a, b) =>
        {
            int speedCompare = b.GetCurrentSpeed().CompareTo(a.GetCurrentSpeed());
            if (speedCompare != 0)
                return speedCompare;
            return string.CompareOrdinal(a.ID, b.ID);
        });

        string orderLog = "CTB顺序: ";
        for (int i = 0; i < result.Count; i++)
        {
            BattleEntity entity = result[i];
            orderLog += $"{i + 1}. {entity.RuntimeData.Definition.Name} ({entity.GetCurrentSpeed()})";
        }
        Debug.Log(orderLog);
       
        return result;
   }
    
    /// <summary>
    /// 把当前调度结果转换成TimelineUI能直接使用的预测节点。
    /// </summary>
    public List<BattleTimelinePredictionNode> BuildTimeLinePrediction()
    {
        List<BattleTimelinePredictionNode> result = new (_currentRound.Count + _nextRound.Count);
        
        // Round 0 = 当前回合剩余行动者
        for (int i = 0; i < _currentRound.Count; i++)
        {
            BattleEntity entity = _currentRound[i];
            if (!entity.IsAlive) continue;
            
            result.Add(new  BattleTimelinePredictionNode($"{entity.ID}_RO", entity, 0));
        }
        
        // Round 1 = 下一回合预测行动者者
        for (int i = 0 ; i< _nextRound.Count; i++)
        {
            BattleEntity entity = _nextRound[i];
            if (!entity.IsAlive) continue;
            
            result.Add(new  BattleTimelinePredictionNode($"{entity.ID}_R1", entity, 1));
        }

        return result;
    }

    private void TriggerRoundStart(List<BattleEntity> allEntities)
    {
        for (int i = 0; i < allEntities.Count; i++)
        {
            var entity = allEntities[i];
            if (!entity.IsAlive)
                continue;
            
            // 1.先处理Break恢复，避免单位永久停留在破盾状态。
            entity.ResolveBreakRecoveryAtRoundStart();
            
            // 2.再消费上一轮挂起到"下一回合开场”才生效的Break跳过计数。
            if (_pendingNextRoundBreakSkip.Remove(entity))
            {
                entity.ConsumeBrokenTurnsByTimeline(1);
            }
            
            // 3．Boss如果上轮尾声已经挂起了阶段切换，就在这统一真正落地。
            if (!entity.IsBroken && entity.TryApplyPendingBossPhase(out BossPhaseConfig appliedPhase))
                EventBus.Publish(new BattleNotificationEvent(entity.ResolveBossPhasePrompt(appliedPhase)));
            
            // 4.每回合开始时要把防御姿态清掉，并重新触发本回合的Break跳过判定。
            entity.ClearDefendStance();
            entity.TriggerBreakSkipForRound();
            
            // 5.最后只给玩家单位回1点BP。
            if (entity.IsPlayer)
                entity.RecoverBP(); // 玩家每回合恢复 BP
        }
    }

    #region 破盾

    /// <summary>
    /// 当单位被破盾或死亡时调用。/1/规则：
    /// 1）死亡：从当前与下一回合都移除。
    /// 2）已行动后破盾：仅剔除下一回合。
    /// 3）未行动就破盾：同时剔除当前与下一回合
    /// </summary>
    public void KickOutFromTimeline(BattleEntity target)
    {
        // 1. 先处理空引用，避免破防信号从空目标进来。
        if (target == null) return;
        
        // 2.死亡单位直接从当前回合和下一回合都移除。
        if (!target.IsAlive)
        {
            _currentRound.Remove(target);
            _nextRound.Remove(target);
            _pendingNextRoundBreakSkip.Remove(target);
            return;
        }
        
        // 3.没有Break的单位不需要再踢出时间轴。
        if (!target.IsBroken) return;

        // 4.当前回合还没行动就被破盾时，直接在这里补一次跳过计数。
        if (_currentRound.Remove(target))
        {
            target.AddBrokenSkipTurns(1);
            target.ConsumeBrokenTurnsByTimeline(1);
        }

        // 5.下一回合里如果也有它，就先记到挂起表里，等回合切换时再消费。
        if (_nextRound.Remove(target))
        {
            // 已行动后破盾：只需要保证下一回合开场时再补跳过一次即可。
            _pendingNextRoundBreakSkip[target] = 1;
        }
    }

    #endregion
}