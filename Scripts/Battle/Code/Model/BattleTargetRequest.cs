using Utils;

/// <summary>
/// 战斗目标请求数据。
/// 
/// 这个结构体的目的不是"帮我们选目标"，而是把这条命令最终想打谁记录下来。
/// 这样玩家输入、敌人AI、目标选择状态都可以先把结果整理成同一种数据，
/// 后面的执器和Handler就不关心这个目标是玩家点出来的，还是AI算出来的。
/// 
/// 可以把它理解成：
/// “这条命令最终指向谁”的统一描述。
/// </summary>
public class BattleTargetRequest
{
    public string TargetEntityID;
    public TargetType Type;
    
    public bool HasTagetEntity => !string.IsNullOrEmpty(TargetEntityID);

    #region 构造助手

    public static BattleTargetRequest FromType(TargetType type) => new BattleTargetRequest() { Type = type };

    public static BattleTargetRequest SingleEnemy(string id)
    {
        return new BattleTargetRequest { Type = TargetType.SingleEnemy, TargetEntityID = id };
    }

    public static BattleTargetRequest SingleAlly(string id)
    {
        return new BattleTargetRequest { Type = TargetType.SingleAlly, TargetEntityID = id };
    }

    public static BattleTargetRequest Self(string id)
    {
        return new BattleTargetRequest{ Type = TargetType.Self, TargetEntityID = id };
    }

    public static BattleTargetRequest AllEnemies => new BattleTargetRequest { Type = TargetType.AllEnemies };
    
    public static BattleTargetRequest AllAllies => new BattleTargetRequest { Type = TargetType.AllAllies };
    #endregion
}
