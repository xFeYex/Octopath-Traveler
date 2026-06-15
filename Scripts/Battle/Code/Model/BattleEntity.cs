
using Utils;

public class BattleEntity
{
    public string ID {get;}
    public BattleUnit Unit {get;}
    public bool IsPlayer {get;}
    public CharacterRuntimeData RuntimeData { get;}
    
    public CharacterDefinitionSO Definition => RuntimeData.Definition;
    public bool IsAlive => RuntimeData.CurrentHP > 0;
    public int CurrentHP => RuntimeData.CurrentHP;
    public int CurrentSP => RuntimeData.CurrentSP;
    public int CurrentBP => RuntimeData.CurrentBP;
    public StatBlock TotalStats => RuntimeData.GetTotalStats();

    // 缓存
    private const int MaxBattleBP = 5;
    private bool _userdBPInThisTurn = false; // 本回合是否已经使用过BP了
    
    /// <summary> 是否处于防御姿态(持续到当前round 结束） </summary>
    public bool IsDefending { get; private set; }
    public SkillDataSO PreparedSkill { get; set; }

    #region 掉落偷取

    public List<InventoryItem> BattleDrops { get; } = new();
    public bool HasBeenRobbed { get; private set; }

    #endregion

    #region 蓄力技能临时状态字段

    /// 蓄力大招机制（Telegraphed Attacks）--
    /// <summary>当前正在蓄的技能(下回合强制释放).若遇破盾则应被打断(清空)，</summary>
    public SkillDataSO PrepareSkill { get; set; }

    #endregion

    #region Boss 阶段临时状态字段

    // ---Boss 阶段机制(Phase Transition MVP)
    /// <summary> 当前已进入的Boss阶段索引(-1表示还未进入任何阶段）</summary>
    private int CurrentBossPhaseIndex {get;set;}
    /// <summary> 已满阈值但尚未播报/应用的阶段索引(-1表示无挂起）</summary>
    private int _pendingBossPhaseIndex;
    
    #endregion

    #region 破盾与护盾临时状态

    public int CurrentShield { get; private set; }
    public bool IsBroken { get;  private set; }
    private int MaxShield { get; set; }
    
    // 剩余需跳过的行动次数（由时间轴或行动点消费）
    private int BrokenTurnsRemaining { get; set; }
    
    // 回合内一次性"跳过行动"挂起标记：
    // 新回合由TriggerBreakSkipForRound 置true,消耗一次后由ConsumeBrokenTurn置false。
    private bool BreakSkipPending { get; set; }

    private readonly HashSet<DamageType> _weaknesses = new();
    private readonly List<DamageType> _orderedWeaknesses = new();

    #endregion
    /* ------------------------------------------------------------------------------------------------ */

    public BattleEntity(CharacterRuntimeData runtimeData, BattleUnit unit, bool isPlayer, string id)
    {
        RuntimeData = runtimeData;
        ID = id;
        Unit = unit;
        IsPlayer = isPlayer;
        InitializeBattleStats();
    }

    private void InitializeBattleStats()
    {
        if (Definition is EnemyDefinitionSO enemyDefinition)
        {
            InitializeBattleDrops(enemyDefinition);
            MaxShield = Mathf.Max(1, enemyDefinition.MaxShields);
            ApplyWeaknesses(enemyDefinition.Weaknesses, false);
        }
        
        ResetShieldAndBreakState();
        _userdBPInThisTurn = false;
        _pendingBossPhaseIndex = -1;
        CurrentBossPhaseIndex = -1;
    }

    private void InitializeBattleDrops(EnemyDefinitionSO enemyDefinition)
    {
        BattleDrops.Clear();

        if (enemyDefinition.Drops.Count >= 0)
        {
            for (int i = 0; i < enemyDefinition.Drops.Count; i++)
            {
                InventoryItem drop = enemyDefinition.Drops[i];
                if (drop.Quantity <= 0)
                    continue;
                
                BattleDrops.Add(new(drop.ItemDefinition, drop.Quantity));
            }
            
            RefreshRobbedState();
        }
    }

    public void RefreshRobbedState()
    {
        // 只要背包里还存在一条有效且数量大于日的掉落，就说明这只敌人还没被偷空。
        HasBeenRobbed = true;

        for (int i = 0; i < BattleDrops.Count; i++)
        {
            if (BattleDrops[i].Quantity > 0)
            {
                HasBeenRobbed = false;
                return;
            }
        }
    }

    #region 行动相关

    public int GetCurrentSpeed()
        {
            // 目前只取基础属性，未来在这里加上GetBuff（StatType.Speed）的加成
            return TotalStats.Speed;
        }
    
        public void SpendSP(int cost)
        {
            RuntimeData.ModifSP(-cost);
            // 广播更新sp
            EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentSP, CurrentSP, TotalStats.MaxSP));
        }
        
        public void SpendBP(int amount)
        {
            RuntimeData.ModifyBP(-amount);
            
            // 广播更新bp
            EventBus.Publish(new EntityStatChangedEvent(this, StatType.CureentBP, CurrentBP, MaxBattleBP));
        }
    
        public void RecoverBP()
        {
            // 1.先判断上一回合有没有使用过BP。
            bool shouldRecoverBP = !_userdBPInThisTurn;
            
            // 2.无论恢不恢复，这里都要先把“本回合是否用过BP”的标记清掉。
            _userdBPInThisTurn = false;
            
            // 3.不满足恢复条件，或者已经满BP，就直接结束。
            if (!shouldRecoverBP || CurrentBP >= MaxBattleBP)
                return;
            
            // 4.真正恢复1点BP，并广播给UI。
            RuntimeData.ModifyBP(1);
            EventBus.Publish(new EntityStatChangedEvent(this, StatType.CureentBP, CurrentBP, MaxBattleBP));
        }
    
        public void RestoreSP(int amount)
        {
            RuntimeData.ModifSP(amount);
            EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentSP, CurrentSP, TotalStats.MaxSP));
        }
        
        
        public void MarkBPUsed() => _userdBPInThisTurn = true;

    #endregion

    #region 伤害计算

    public int CalculateDamageFrom(BattleEntity attacker, SkillDataSO skill, float powerMultiplier)
    {
        bool isMagical = skill != null && skill.damageKind == DamageKind.Magical;
        StatBlock atkStats = attacker.TotalStats;
        StatBlock defStats = TotalStats;

        int atk = isMagical ? atkStats.MAtk : atkStats.PAtk;
        int  def = isMagical ? defStats.MDef : defStats.PDef;

        if (IsDefending)
            def = Mathf.RoundToInt(def * 1.5f); // 防御姿态下防御力翻倍
        
        int basePower = skill.basePower;
        int rawDamage = Mathf.Max(1, atk - def + basePower);
        return Mathf.RoundToInt(rawDamage * powerMultiplier);
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive) return;
        // 1.先把伤害真正扣到RuntimeData上。
        RuntimeData.ModifHP(-amount);

        // 2.受伤后顺检查是否满足BoSS 阶段切换阈值，并先挂起阶段。
        TryQueueBossPhaseTransitionByHp();

        // 3.广播伤害事件
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentHP, CurrentHP, TotalStats.MaxHP));

        // 4.最后刷新单位表现，比如死亡、残血动画、破盾表现等。
        Unit.UpdateVisuals();
    }

    public int CalculateHealAmountFromSkill(SkillDataSO skill, float powerMultiplier)
    {
        int baseHeal = Mathf.Max(0, skill.healAmount);
        return  Mathf.RoundToInt(baseHeal * powerMultiplier);
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;
        
        RuntimeData.ModifHP(amount);
        
        // 广播回血事件
        EventBus.Publish(new EntityStatChangedEvent(this, StatType.CurrentHP, CurrentHP, TotalStats.MaxHP));
        
        Unit.UpdateVisuals();
    }

    #endregion
    
    #region 防御姿态接口

    /// <summary>
    /// 进入防御姿态，持续到当前round结束.
    /// </summary>
    public void EnterDefendStance()
    {
        if (!IsAlive)
            return;
        
        IsDefending = true;
    }
    
    /// <summary>
    /// 退出防御姿态(通常在新round 开始时统一清除）.
    /// </summary>
    public void ClearDefendStance() => IsDefending = false;

    #endregion

    #region 弱点

    private void ApplyWeaknesses(List<DamageType> weaknesses, bool publishEvent)
    {
        // 1.先清掉旧的弱点缓存。
        _weaknesses.Clear();
        _orderedWeaknesses.Clear();
        
        // 2.再把新弱点按“判定集合+UI顺序列表”双写进缓存。
        foreach (DamageType type in weaknesses)
        {
            if (type == DamageType.None || type == DamageType.Untyped)
            {
                continue;
            }
            
            // 双写结构：HashSet用于命中判定，List用于UI顺序展示。
            if (_weaknesses.Add(type))
                _orderedWeaknesses.Add(type);
        }
        
        // 3.如有需要，最后广播弱点变化事件。
        if (publishEvent)
            EventBus.Publish(new EntityWeaknessChangedEvent(this));
    }

    public bool IsWeakTo(DamageType type) => !IsPlayer && _weaknesses.Contains(type);
    
    public List<DamageType> GetWeaknesses() => _orderedWeaknesses;
    
    #endregion

    #region 破盾

    public bool TryReduceShield(int amount)
    {
        // 1.只有没破防且还有护盾时，才需要真正扣层数。
        if (IsBroken || CurrentShield <= 0)
            return false;
        
        // 2.先扣掉当前护盾值。
        CurrentShield = Mathf.Max(0, CurrentShield - amount);
        
        // 3.立即广播给UI刷新护盾数字。
        EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));
        
        // 4. 如果护盾归零，就正式进入Break。
        if (CurrentShield == 0)
        {
            OnBreak();
            return true;
        }
        
        return false;
    }

    private void OnBreak()
    {
        // 1.先把内部Break标记和跳过次数写好。
        IsBroken = true;
        BrokenTurnsRemaining = 1;
        BreakSkipPending = false;
        
        // 2。破盾会打断蓄力，所以这里把准备中的技能和特效清掉。
        PreparedSkill = null;
        Unit.StopTelegraphVfx();
        // 3.再刷新单位头上的Break·眩晕表现。
        Unit.SetBreakStunVisual(true);
    }

    public void ResolveBreakRecoveryAtRoundStart()
    {
        if (IsBroken && BrokenTurnsRemaining <= 0)
            RecoverFromBreak();
    }
    
    /// <summary>
    /// 从破防状态恢复（通常在Break后的下一回合结束时调用）
    /// </summary>
    public void RecoverFromBreak()
    {
        if (!IsBroken)
            return;

        // 1.先把内部Break状态复位。
        ResetShieldAndBreakState();
        
        // 2.再关掉场景里的Break表现。
        Unit.SetBreakStunVisual(false);
        
        // 3.最后按“恢复事件->护盾事件”的顺序广播，让外部读到最新状态。
        EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));
    }

    private void ResetShieldAndBreakState()
    {
        IsBroken = false;
        BrokenTurnsRemaining = 0;
        BreakSkipPending = false;
        CurrentShield = MaxShield;
    }

    public void AddBrokenSkipTurns(int amount)
    {
        if (!IsBroken)
            return;
        
        BrokenTurnsRemaining += amount;
    }

    public void TriggerBreakSkipForRound()
    {
        if (IsBroken && BrokenTurnsRemaining > 0)
            BreakSkipPending = true;
    }
    
    /// <summary>
    /// 根据时间轴消耗破碎回合数
    /// </summary>
    /// <param name="amount">要消耗的回合数</param>
    public void ConsumeBrokenTurnsByTimeline(int amount)
    {
        // 检查角色是否处于破碎状态，以及要消耗的回合数是否有效
        if (!IsBroken || BrokenTurnsRemaining <= 0)
            return;

        // 减少剩余的破碎回合数，确保不会小于0
        BrokenTurnsRemaining = Mathf.Max(0, BrokenTurnsRemaining - amount);
        
        // 下-回合若仍需跳过，会由TriggerBreakSkipForRound 重新置位.
        BreakSkipPending =false;
    }
    
    #endregion

    #region Boss 阶段逻辑(Phase Transition MVP)

    /// <summary>
    /// 获取当前已生效的Boss阶段配置（若不存在则返回null）
    /// 用于AI根据阶段调整出招权重
    /// </summary>
    public BossPhaseConfig GetActiveBossPhaseConfig()
    {
        if (Definition is not EnemyDefinitionSO enemyDef || CurrentBossPhaseIndex < 0 
            || CurrentBossPhaseIndex >= enemyDef.BossPhases.Count)
            return null;
        
        return enemyDef.BossPhases[CurrentBossPhaseIndex];
    }

    /// <summary>
    /// 按当前HP比例检查BoSS阶段切换，并挂起目标阶段.
    /// 规则：
    /// 1）仅敌方可触发
    /// 2）按配置列表顺序检查（建议阈值从高到低）
    /// 3）支持一次大伤害跨多个阶段（直接挂起到最终应进入阶段）
    /// 4）挂起后由外部流程在"可行行动窗口"调用TryApplyPendingBossPhase正式应用
    /// </summary>
    private void TryQueueBossPhaseTransitionByHp()
    {
        if (IsPlayer || !IsAlive || Definition is not EnemyDefinitionSO enemyDef)
            return;

        float hpRatio = CurrentHP / (float)Mathf.Max(1, TotalStats.MaxHP);
        int targetPhaseIndex = CurrentBossPhaseIndex;

        for (int i = CurrentBossPhaseIndex + 1; i < enemyDef.BossPhases.Count; i++)
        {
            var phase = enemyDef.BossPhases[i];
            if (hpRatio <= phase.triggerHpRatio)
            {
                targetPhaseIndex = i;
            }
            else
            {
                // 如果当前阶段都不满足条件，由于是从高血量到低血量检查，说明后面更低的阈值肯定也不满足
                break;
            }
        }

        if (targetPhaseIndex > CurrentBossPhaseIndex && targetPhaseIndex > _pendingBossPhaseIndex) // 防止重复挂起
        {
            _pendingBossPhaseIndex = targetPhaseIndex;
            Debug.Log($"[BossPhase]  挂起阶段 {_pendingBossPhaseIndex+1}，等待可行行动窗口应用。");
        }
    }

    /// <summary>
    /// 在可行行动窗口调用：
    /// 若存在挂起阶段且当前不处于Break，则立即应用其配置并返回.
    /// </summary>
    public bool TryApplyPendingBossPhase(out BossPhaseConfig appliedPhase)
    {
        appliedPhase = null;
        
        // 规则：处于破盾眩晕时，不应用阶段切换.
        if (IsBroken)
            return false;
        
        if (Definition is not EnemyDefinitionSO enemyDef || _pendingBossPhaseIndex < 0 )
            return false;
        
        var phase = enemyDef.BossPhases[_pendingBossPhaseIndex];
        CurrentBossPhaseIndex = _pendingBossPhaseIndex;
        _pendingBossPhaseIndex = -1;
        appliedPhase = phase;
        ApplyBossPhaseConfig(phase);
        return true;
    }

    /// <summary>
    /// 套用单个Boss阶段配置
    /// 仅处理MVP需求：护盾切换，弱点切换.
    /// </summary>
    private void ApplyBossPhaseConfig(BossPhaseConfig phase)
    {
        // 1.先处理护盾切换。
        if (phase.overrideMaxShield)
        {
            bool wasBroken = IsBroken;
            
            MaxShield = phase.maxShield;
            ResetShieldAndBreakState();
            Unit.SetBreakStunVisual(false);
            
            if (wasBroken)
                EventBus.Publish(new EntityRecoverFromBreakEvent(this));
            EventBus.Publish(new EntityShieldChangedEvent(this, CurrentShield));
        }
        
        // 2.再处理弱点切换。
        if (phase.overrideWeaknesses)
            ApplyWeaknesses(phase.weaknesses, true);
    }

    public string ResolveBossPhasePrompt(BossPhaseConfig phase)
    {
        return !string.IsNullOrWhiteSpace(phase.promptText)
            ? phase.promptText
            : "进入了新的阶段！";
    }
    #endregion
}