using Utils;

/// <summary>
/// 一次战斗指令的数据载体（由UI组装，交给执行状态消费）
/// </summary>
public class BattleCommandRequest
{
    /// <summary> 指令类型:Attack / Skill/ Item / Defend/ Escape </summary>
    public BattleCommandType Type;
    public  SkillDataSO Skill;

    public ItemDefinitionSO ItemDefinition;
    public string TargetEntityID;
    public int BPSpend;

    public static BattleCommandRequest CreateAttack(BattleEntity actor, int bpSpend = 0)
    {
        return new BattleCommandRequest
        {
            Type = BattleCommandType.Attack ,
            Skill = actor.Definition.BasicAttack,
            BPSpend = bpSpend
        };
    }
    
    public static BattleCommandRequest CreateSkill( SkillDataSO skill, int bpSpend = 0)
    {
        return new BattleCommandRequest
        {
            Type = BattleCommandType.Attack ,
            Skill = skill,
            BPSpend = bpSpend
        };
    }

    public static BattleCommandRequest CreateItem(ItemDefinitionSO item)
    {
        return new BattleCommandRequest
        {
            Type = BattleCommandType.Item,
            ItemDefinition = item,
            TargetEntityID = null
        };
    }

    public static BattleCommandRequest CreateDefend()
    {
        return new BattleCommandRequest
        {
            Type = BattleCommandType.Defend,
            TargetEntityID = null
        };
    }
    
    public static BattleCommandRequest CreateEscape()
    {
        return new BattleCommandRequest
        {
            Type = BattleCommandType.Escape,
            TargetEntityID = null
        };
    }

}