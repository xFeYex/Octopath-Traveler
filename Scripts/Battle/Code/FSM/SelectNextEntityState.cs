
/// <summary>
/// 
/// 选择下一位行动者状态。
/// 这个状态主要负责：
/// 1）先检查战斗是否已经结束；
/// 2）再从BattleRoundScheduler取出下一位真正要行动的实体；///3）同步当前行动者事件和时间轴显示；
/// 4）最后根据阵营切到玩家输入状态或敌方AI状态。
/// 
/// 可以把它理解成：
/// “状态机的回合分发器”。
/// </summary>
public class SelectNextEntityState : BattleState
{
    public SelectNextEntityState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute()
    {
        // 1.检查战斗是否已经结束：
        if (BattleOutcomeResolver.TryGetBattleEndedEvent(_controller.AllEntities, out BattleEndedEvent endedEvent))
        {
            _controller.EndBattle(endedEvent);
            yield break;
        }
        
        // 2.从BattleRoundScheduler里取出下一位真正要行动的实体：
        BattleEntity nextEntity = _controller.GetNextActorByRound();
        
        // 3.记录当前行动者，并立刻刷新时间轴预测，让UI先同步这一轮队列变化。
        _controller.CurrentEntity = nextEntity;
        _controller.UpdateTimelinePrediction(); // 每次选出行动者后都更新一次时间轴预测，保证UI显示正确。
        
        // 4.更新当前行动者的大头像与名字。
        if (_controller.Config.TurnStartDelay > 0)
            yield return new WaitForSeconds(_controller.Config.TurnStartDelay);
        
        // 5.广播“当前轮到谁行动了”
        _controller.TimelineUI.SetActiveEntity(nextEntity); // 通知时间轴UI更新当前行动者的焦点显示。
        EventBus.Publish(new ActiveEntityChangedEvent(nextEntity)); // 发布事件通知其他系统当前行动者变了。
        
        // 6.最后根据阵营决定下一步去哪：
        // 玩家进输入状态，敌人进AI状态。
        if (nextEntity.IsPlayer)
            _controller.SetState(new PlayerInputState(_controller));
        else
            _controller.SetState(new EnemyAIState(_controller));
    }
}