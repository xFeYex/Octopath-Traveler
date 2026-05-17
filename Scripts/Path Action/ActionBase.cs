using Utils;

public abstract class ActionBase: MonoBehaviour
{
    public PlayerJob MatchJob = PlayerJob.Any;
    
    public ActionCommandInfo CommandInfo;
    
    /* -------------------------------------------------*/

    // 匹配当前命令是否能显示
    public virtual bool CanShow(AllyDefinitionSO interaction)
    {
        return IsJobMatch(interaction);
    }

    public virtual void TriggerAction(AllyDefinitionSO interaction)
    {
        // 需要打开面板就重新写
        // 默认执行
        Execute(interaction);
    }
    
    public virtual void Execute(object contextData = null){}
    
    protected virtual bool IsJobMatch(AllyDefinitionSO interaction)
    {
        if (MatchJob == PlayerJob.Any)
            return true;
        
        return interaction.job == MatchJob;
    }
    
}

[System.Serializable]
public struct ActionCommandInfo
{
    public string DisplayName;
    public Sprite Icon;
    public int Order;
}