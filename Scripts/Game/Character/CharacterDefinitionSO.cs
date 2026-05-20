
public class CharacterDefinitionSO : ScriptableObject
{
    [Header("Identity")] 
    public string ID;
    public string Name;
    public Sprite Portrait;

    [Header("Stats")] 
    public int BaseLevel = 1;
    public StatBlock BaseStats;
    
    [Header("Animator")]
    public AnimatorOverrideController fieldAnimator;
    public AnimatorOverrideController battleAnimator;
}

[System.Serializable]
public struct StatBlock // 状态模块
{
    public int MaxHP;
    public int MaxSP;
    
    // 物
    public int PAtk;
    public int PDef; // 防
    
    // 法
    public int MAtk;
    public int MDef;
    
    public int Speed;
    
    public int Accuracy; // 命中率
    public int Evasion;  // 闪避率

    /* --------------------------------------------- */
    
    public static StatBlock zero = new();

    public static StatBlock operator +(StatBlock a, StatBlock b)
    {
        return new()
        {
            MaxHP = a.MaxHP + b.MaxHP,
            MaxSP = a.MaxSP + b.MaxSP,
            PAtk = a.PAtk + b.PAtk,
            PDef = a.PDef + b.PDef,
            MAtk = a.MAtk + b.MAtk,
            MDef = a.MDef + b.MDef,
            Speed = a.Speed + b.Speed,
            Accuracy = a.Accuracy + b.Accuracy,
            Evasion = a.Evasion + b.Evasion,
        };
    }
    
    
}