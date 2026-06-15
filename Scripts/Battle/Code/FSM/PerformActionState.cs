using Utils;

/// <summary>
/// 执行行动状态。
/// 
/// 这个状态主要负责：Unity脚本
/// 1）读取当前行动者和当前指令；
/// 2） 创建 BattleActionContext;
/// 3）把命令交给 BattleCommandExecutor 分发到对应Handler；Unity 消息
/// 4）执行完后刷新时间轴，并进入回合收尾状态。
/// 
/// 可以把它理解成：
/// “真正把这一回合命令打出去的状态”。
/// </summary>
public class PerformActionState : BattleState
{
    private AttackSkillCommandHandler AttackSkillCommandHandler = new();
    private EscapeCommandHandler EscapeCommandHandler = new();
    private DefendCommandHandler  DefendCommandHandler = new();
    private ItemCommandHandler  ItemCommandHandler = new();
    
    public PerformActionState(BattleController controller) : base(controller) { }

    public override IEnumerator Execute()
    {
        BattleEntity entity = _controller.CurrentEntity;
        BattleCommandRequest command = _controller.CurrentCommand;

        switch (command.Type)
        {
            case BattleCommandType.Attack:
            case BattleCommandType.Skill:
                yield return AttackSkillCommandHandler.Execute(_controller);
                break;
            case BattleCommandType.Defend:
                yield return DefendCommandHandler.Execute(_controller);
                break;
            case BattleCommandType.Escape:
                yield return EscapeCommandHandler.Execute(_controller);
                break;
            case BattleCommandType.Item:
                yield return ItemCommandHandler.Execute(_controller);
                break;
        }
        
        _controller.UpdateTimelinePrediction();
        _controller.SetState(new TurnEndState(_controller));
    }
}