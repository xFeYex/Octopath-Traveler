using System;
using Utils;

[CreateAssetMenu(menuName = "Battle/Skill")]
public class SkillDataSO : ScriptableObject
{
    [Header("Special Logic Strategy")]
    public SkillLogicSO specialLogic;
    
    [Header("Identify")] 
    public string skillID;
    
    public string skillName;
    
    [TextArea]
    public string description;
    public Sprite icon;
    
    [Header("Cost")]
    [Min(0)] public int spCost;
    
    [Header("Targeting")]
    public TargetType targetType = TargetType.SingleEnemy;
    
    [Header("Type")]
    public SkillType skillType = SkillType.Damage;
    public DamageKind  damageKind = DamageKind.Physical;
    public ElementType elementType = ElementType.None;
    public WeaponType weaponType = WeaponType.None;

    [Header("Effect (Prototype)")] 
    [Min(0)] public int basePower;
    [Min(1)] public int hitCount;
    [Min(0)] public int healAmount;
    
    [Header("VFX (Prototype)")]
    public GameObject hitVfxPrefab;
    public SkillVfxSpawnMode vfxSpawnMode = SkillVfxSpawnMode.AutoByTargetType;
    [Tooltip("勾选后，特效会从施法者当前的位置发出；不勾选时，沿用SpawnMode决定的位置。")]
    public bool vfxSpawnFromCaster = false;
    public Vector3 vfxOffset;
    [Tooltip("外的Y轴旋转。默认0想把左右方向反过来就填180。")]
    public float vfxYRotation = 0f;
    [Tooltip("效出现后,等多久才真正结算命中。0代表立刻生效。")] 
    [Min(0f)] public float vfxHitDelay = 0f;
    [Tooltip("特效生成后多久自动销毁。θ代表不自动销毁。")]
    [Min(0f)] public float vfxLifeTime = 2f;
    
    [Header("Camera Impulse")]
    [Min(0f)] public float cameraImpulseStrength;

    #region 敌人蓄力提示特效参数

    [Header("TelegraphVFX（蓄提示特效）"), Tooltip("敌人进入蓄力状态时播放的特效（可选）")]
    public GameObject telegraphVfxPrefab;
    [Tooltip("蓄力特效生成偏移(相对于施法者锚点)")]
    public Vector3 telegraphVfxOffset;
    [Min(0f),Tooltip("蓄力特效自动销毁时间o=不自动销毁，等逻辑手动清除")]
    public float telegraphVfxLifetime = 0f;
    [Tooltip("是否将蓄力特效挂到施法者身上，便于角色移动时跟随")]
    public bool telegraphVfxAttachToCaster = true;

    #endregion
    
    [Header("Boost (Tier0 ~ Tier3)")] 
    public BoostTierConfig[] boostTiers = new BoostTierConfig[4]
    {
        BoostTierConfig.Default(0),
        BoostTierConfig.Default(1),
        BoostTierConfig.Default(2),
        BoostTierConfig.Default(3)
    };
    
    /* ------------------------------------------------------------------------------- */ 

    public BoostTierConfig GetBoostTier(int boostLevel)
    {
        int t = Mathf.Clamp(boostLevel, 0, 3);
        return boostTiers[t];
    }

    public float GetBoostPowerMultiplier(int bpSpend)
    {
        int spend = Mathf.Clamp(bpSpend, 0, 3);
        return GetBoostTier(bpSpend).powerMultiplier;
    }

    public int GetFinalHitCount(BattleCommandType commandType, int bpSpend)
    {
        if (commandType == BattleCommandType.Attack && bpSpend <= 0)
            return 1;
        
        int finalHitCount = hitCount;
        int spend = Mathf.Clamp(bpSpend, 0, 3);
        return finalHitCount + GetBoostTier(spend).hitCountBonus;
    }

    /// <summary>
    /// 把技能资源上的武器/属性配置解析成统一伤害类型。
    /// </summary>
    public DamageType ResolveDamageType()
    {
        if (weaponType != WeaponType.None)
        {
            return weaponType switch
            {
                WeaponType.Sword => DamageType.Sword,
                WeaponType.Spear => DamageType.Spear,
                WeaponType.Dagger => DamageType.Dagger,
                WeaponType.Axe => DamageType.Axe,
                WeaponType.Bow => DamageType.Bow,
                WeaponType.Staff => DamageType.Staff,
                _ => DamageType.None
            };
        }

        if (elementType != ElementType.None)
        {
            return elementType switch
            {
                ElementType.Fire => DamageType.Fire,
                ElementType.Ice => DamageType.Ice,
                ElementType.Lightning => DamageType.Lightning,
                ElementType.Light => DamageType.Light,
                ElementType.Wind => DamageType.Wind,
                ElementType.Dark => DamageType.Dark,
                _ => DamageType.Untyped
            };
        }
        
        return DamageType.Untyped;
    }
}

#region Boost分层配置结构

[Serializable]
public struct BoostTierConfig
{
    [Tooltip("Boost等级 (0~3)"), Range(0, 3)] 
    public int tier;
    
    [Header("Combat Stats"), Tooltip("倍率加成：伤害时乘basePower，治疗时乘healAmount(例如：1.0，1.5，2.0，3.0)"), Min(0.01f)]
    public float powerMultiplier;
    
    [Tooltip("命中次数加成：最终命中=hitCount+hitCountBonus"), Min(0)]
    public int hitCountBonus;
    
    [Header("UtilityStats"), Tooltip("概率加成（0~1）：用于偷窃、即死、异常附加等机制的成功率提升")]
    public float chanceBonus;
    
    [Tooltip("持续回合加成：用于Buff/Debuff持续时间的延长")]
    public int durationBonus;
    
    [Tooltip("通用数值加成：用于特殊技能（如传递BP的数量增加、恢复固定SP等）")]
    public int genericValueBonus;

    public static BoostTierConfig Default(int tier)
    {
        return new BoostTierConfig
        {
            tier = tier,
            powerMultiplier = 1f, // θ级通常是1倍
            hitCountBonus = 0,
            chanceBonus = 0f,
            durationBonus = 0,
            genericValueBonus = 0,
        };
    }
}

#endregion