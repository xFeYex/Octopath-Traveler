///<summary>
/// 单次战斗指令的执行上下文.
/// 设计要点：
/// 1） Controtler/Actor/Command 是只读输入.
/// 2） Targets/IsCancelled是四阶段共享的运行时状态。
/// </summary>
public class BattleActionContext
{
    public BattleContoller Controller { get; }
    public BattleEntity Actor { get; }
    public BattleCommandRequest Command { get; }
}