using System;

// 战斗运行数据
[Serializable]
public class CharacterRuntimeData
{
    public CharacterDefinitionSO Definition;

    public int Level;

    public int CurrentHP;
    public int CurrentSP;
    public int CurrentBP;
    public int CurrentExp;

    public string DisplayName => Definition.Name;

    public StatBlock EquipmentStats; // 装备数据

    /* ------------------------------------------------------ */

    public CharacterRuntimeData(CharacterDefinitionSO definition)
    {
        Definition = definition;
        EquipmentStats = StatBlock.zero;

        var stats = GetTotalStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
        CurrentBP = 0;
    }

    // 运行时的基础数据 = 基础数据 * 等级
    public StatBlock GetBaseStats()
    {
        if (Definition is AllyDefinitionSO allyDefinition)
        {
            return allyDefinition.GetStatForLevel(Level);
        }

        if (Definition is EnemyDefinitionSO enemyDefinition)
        {
            return enemyDefinition.BaseStats;
        }

        return Definition is null ? Definition.BaseStats : StatBlock.zero;
    }

    // 运行时的总基础数据 = 基础数据 + 装备数据
    public StatBlock GetTotalStats() => GetBaseStats() + EquipmentStats;


    #region 数据变化接口

    public void HealFull()
    {
        CurrentHP = GetTotalStats().MaxHP;
        CurrentSP = GetTotalStats().MaxSP;
    }

    public void ModifHP(int amount)
    {
        CurrentHP += amount;
        CurrentHP = Mathf.Clamp(CurrentHP, 0, GetTotalStats().MaxHP);
    }

    public void ModifSP(int amount)
    {
        CurrentSP += amount;
        CurrentSP = Mathf.Clamp(CurrentSP, 0, GetTotalStats().MaxSP);
    }

    public void ResetBattleBp()
    {
        CurrentBP = 0;
    }

#endregion
}