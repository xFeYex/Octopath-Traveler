
using Utils;

[CreateAssetMenu(menuName = "Character/Ally")]
public class AllyDefinitionSO : CharacterDefinitionSO
{
    [Header("Ally Definition")] 
    public PlayerJob job;
    
    [Header("Growth Settings")]
    public GlobalGrowthConfigSO globalGrowthConfigSO;
    public GrowthProfile growthProfile;

    #region 属性成长

    public StatBlock GetStatForLevel(int level)
    {
        float hpMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.HP).Evaluate(level);
        float spMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.SP).Evaluate(level);
        float pAtkMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.PAtk).Evaluate(level);
        float pDefMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.PDef).Evaluate(level);
        float mAtkMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.MAtk).Evaluate(level);
        float mDefMult = globalGrowthConfigSO.GetCurveByRank(growthProfile.MDef).Evaluate(level);
        float speend = globalGrowthConfigSO.GetCurveByRank(growthProfile.Speed).Evaluate(level);

        return new StatBlock
        {
            MaxHP = Mathf.RoundToInt(BaseStats.MaxHP * hpMult),
            MaxSP = Mathf.RoundToInt(BaseStats.MaxSP * spMult),
            PAtk = Mathf.RoundToInt(BaseStats.PAtk * pAtkMult),
            PDef = Mathf.RoundToInt(BaseStats.PDef * pDefMult),
            MAtk = Mathf.RoundToInt(BaseStats.MAtk * mAtkMult),
            MDef = Mathf.RoundToInt(BaseStats.MDef * mDefMult),
            Speed = Mathf.RoundToInt(BaseStats.Speed * speend)
        };
    }

    #endregion
}

[System.Serializable]
public struct GrowthProfile
{
    public GrowthRank HP;
    public GrowthRank SP;
    
    public GrowthRank PAtk;
    public GrowthRank PDef;
    
    public GrowthRank MAtk;
    public GrowthRank MDef;
    
    public GrowthRank Speed;
}