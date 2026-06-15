
using Utils;

[CreateAssetMenu(menuName = "Character/Ally")]
public class AllyDefinitionSO : CharacterDefinitionSO
{
    [Header("Ally Definition")] 
    public PlayerJob job;
    
    [Header("Growth Settings")]
    public GlobalGrowthConfigSO globalGrowthConfigSO;
    public GrowthProfile growthProfile;

    [Header("Equipment Capability")]
    public List<WeaponType> EquipableWeaponTypes = new();
    
    [Header("Initial Equipment")]
    public List<InitialEquipmentEntry> InitialEquipment = new();

    #region 经验成长参数

    [Header("Progression")] 
    [Min(1)] public int ExpToNextLevelAtLv1 = 200;
    [Min(1f)] public float ExpGrowthPerLevel = 1.15f;
    [Min(2)] public int MaxLevel = 99;

    #endregion
    
    [System.Serializable]
    public struct InitialEquipmentEntry
    {
        public EquipSlot slot;
        public EquipmentItemSO item;
    }
    
    /* ------------------------------------------------------------------------------------------------ */

    /// <summary>
    /// 计算升级到下一级所需的经验值
    /// </summary>
    /// <param name="currentLevel">当前等级</param>
    /// <returns>升级到下级所需的经验值，如果已达到最等级则返回0</returns>
    public int GetExpRequiredTonNextLevel(int currentLevel)
    {
        // 检查是否已达到最大等级
        if (currentLevel >= MaxLevel)
            return 0;
        
        // 确保等级不小于1
        int clampedLevel = Mathf.Max(1, currentLevel);
        
        // 使用指数增长公式计算所需经验值
        float scaled = ExpToNextLevelAtLv1 * Mathf.Pow(ExpGrowthPerLevel, clampedLevel - 1);
        
        // 返回四舍五入后的经验值，并确保至少为1
        return Mathf.Max(1, Mathf.RoundToInt(scaled));
    }
    
    public bool CanEquipWeaponType(WeaponType weaponType)
    {
        if (weaponType == WeaponType.None) return false;
        return EquipableWeaponTypes.Contains(weaponType);
    }
    
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
            Speed = Mathf.RoundToInt(BaseStats.Speed * speend),
            Accuracy = BaseStats.Accuracy,
            Evasion = BaseStats.Evasion,
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