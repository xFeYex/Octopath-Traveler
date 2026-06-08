///<summary>
/// 技能逻辑策略基类
/// 作用：将特殊技能逻辑(如召唤，BP传递，变身）从Handler中剥离出来
/// </summary>
public abstract class SkillLogicSO : ScriptableObject
{
    /// <summary>
    /// 执行特殊逻辑
    /// </summary>
    /// <param name="context">战上下(包含施法者，控制器等)</param>
    /// <param name="targets">已选定的目标列表</param>
    /// <param name="skillData">技能本身的静态数据（于读取威等参数）</param>
    /// <returns></returns>
    public abstract IEnumerator ExecuteLogic(BattleActionContext context, List<BattleEntity> targets, SkillDataSO skillData);
}