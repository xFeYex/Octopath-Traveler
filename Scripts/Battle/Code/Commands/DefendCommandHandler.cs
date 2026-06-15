/// <summary>
/// 防御指令处理器:
/// 1) 进入防御姿态
/// 2) 下一回合执行顺序插队到第1位
/// 3) 播放短暂防御停顿
/// </summary>
public sealed class DefendCommandHandler : BattleCommandHandlerBase
{
    protected override IEnumerator ExecutionPhase()
    {
        BattleEntity entity = Actor;

        // 1. 进入减伤姿态(直到下一次轮到他行动前生效)
        entity.EnterDefendStance();
        
        yield break;
    }
}