using UnityEditor.Animations;
using Utils;

/// <summary>
/// 战斗目标规则工具。
/// 
/// 这个工具类的目的，是把“目标相关规则”集中到一个地统一管理：
/// 1.某种目标类型要不要进入目标选择状态；
/// 2.某种目标类型应该筛出哪些候选目标；
/// 3.玩家或AI选中的实体，最后要怎么转换回命令里的目标请求。
/// 这样 PLayerInputState、TargetSelectionState、EnemyAIState 和执行层
/// 就不用各写一套“谁是敌、谁是队友、单体和群体怎么分”的重复代码。
/// 
/// 可以把它理解成：
/// “战里所有目标规则的统一工具箱”。
/// </summary>
public static class BattleTargeting
{
    public static List<BattleEntity> GetAliveTargetByType(BattleEntity self, TargetType targetType,
        List<BattleEntity> allEntities)
    {
        return targetType switch
        {
            TargetType.SingleAlly or TargetType.AllAllies
                => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer == self.IsPlayer),
            _ => allEntities.FindAll(entity => entity.IsAlive && entity.IsPlayer != self.IsPlayer)
        };
    }
}