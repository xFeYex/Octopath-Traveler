
public abstract class BattleState
{
    protected readonly BattleContoller _controller;

    public BattleState(BattleContoller controller)
    {
        _controller = controller;
    }
    
    public virtual IEnumerator Enter()
    {
        yield break;
    }
    
    public abstract IEnumerator Execute();

    public virtual IEnumerator Exit()
    {
        yield break;
    }
}