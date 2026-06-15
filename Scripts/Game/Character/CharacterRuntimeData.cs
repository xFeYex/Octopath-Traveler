using System;
using Unity.Collections;
using Unity.Mathematics;
using Utils;

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
    
    private bool hasAppliedInitialEquipment; // 初始装备是否被装备上
    
    [Serializable]
    public class EquippedItemEntry
    {
        public EquipSlot slot;
        public EquipmentItemSO item;
    }
    
    public List<EquippedItemEntry> EquippedItems = new();

    /* ------------------------------------------------------ */

    public CharacterRuntimeData(CharacterDefinitionSO definition)
    {
        Definition = definition;
        EquipmentStats = StatBlock.zero;
        Level = definition.BaseLevel;
        ApplyInitialEquipment();
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

    public void ModifyBP(int amount)
    {
        CurrentBP += Mathf.Clamp(CurrentBP + amount, 0, 5);
    }

    public void ResetBattleBp()
    {
        CurrentBP = 0;
    }

    #endregion

    #region 挑战战力评估

    public static int EvaluatePowerFromStats(StatBlock stats)
    {
        float score =
            stats.MaxHP * 0.2f +
            stats.MaxSP * 0.1f +
            stats.PAtk * 1.5f +
            stats.MAtk * 1.5f +
            stats.PDef * 1.0f +
            stats.MDef * 1.0f +
            stats.Speed * 1.2f;
        
        return Mathf.Max(1, Mathf.RoundToInt(score));
    }

    #endregion
    
    #region 装备系统

    public void ApplyInitialEquipment()
    {
        if (hasAppliedInitialEquipment) return;
        
        AllyDefinitionSO allyDef = Definition as AllyDefinitionSO;
        if (allyDef == null || allyDef.InitialEquipment == null || allyDef.InitialEquipment.Count == 0)
        {
            hasAppliedInitialEquipment = true;
            return;
        }

        if(InventoryManager.Instance == null) return;
        
        for (int i = 0; i < allyDef.InitialEquipment.Count; i++)
        { 
            var entry = allyDef.InitialEquipment[i];
            var item = entry.item;

            if (item == null) continue;
            
            // TODO: 装备物品
            SetEquippedItem(entry.slot, item);
            
            InventoryManager.Instance.AddItem(item, 1);
        }

        hasAppliedInitialEquipment = true;
    }

    public void SetEquippedItem(EquipSlot slot, EquipmentItemSO item)
    {
        var entry = EquippedItems.Find(e => e.slot == slot);
        if (entry != null)
        {
            if (item == null)
                EquippedItems.Remove(entry);
            else
                entry.item = item;
        }
        else
        {
            EquippedItems.Add(new EquippedItemEntry(){slot = slot, item = item});
        }
        
        RebuildEquipmentStats();
    }

    /// <summary>
    /// 获取指定装备槽中已装备的物品
    /// </summary>
    /// <param name="slot">要查询的装备槽位</param>
    /// <returns>返回装备槽中的物品，没有则返回null</returns>
    public EquipmentItemSO GetEquippedItem(EquipSlot slot)
    {
        var entry = EquippedItems.Find(entry => entry.item != null && entry.slot == slot);
        return entry?.item;
    }

    public int GetEquippedItemCount(ItemDefinitionSO targetItem)
    {
        if (targetItem == null) return 0;

        int count = 0;
        foreach (var entry in EquippedItems)
        {
            if (entry.item != null && entry.item == targetItem)
                count++;
        }
        
        return count;
    }

    public void RebuildEquipmentStats()
    {
        var merged = StatBlock.zero;
        for (int i = EquippedItems.Count - 1; i >= 0; i--)
        {
            var entry = EquippedItems[i];
            if (entry.item == null || entry == null)
            {
                EquippedItems.RemoveAt(i);
                continue;
            }
            merged += entry.item.statBonus;
        }
        
        EquipmentStats = merged;

        var total = GetTotalStats();
        CurrentHP = Math.Min(CurrentHP, total.MaxHP);
        CurrentSP = Math.Min(CurrentSP, total.MaxSP);
    }

    #endregion

    #region 经验成长与升级

    public int GetExpRequiredToNextLevel()
    {
        // 队伍成员才会走经验成长，这里直接读取盟友成长配置。
        return ((AllyDefinitionSO)Definition).GetExpRequiredTonNextLevel(Level);
    }

    public float GetExpProgress01()
    {
        int targetExp = GetExpRequiredToNextLevel();
        if (targetExp == 0)
            return 1f;
        
        return CurrentExp / (float)targetExp;
    }

    /// <summary>
    /// 添加经验并处理升级，返回实际应用的经验值.
    /// </summary>
    public int AddExp(int amount)
    {
        int remainingExp = amount;

        AllyDefinitionSO allyDef = (AllyDefinitionSO)Definition;

        int appliedExp = 0; // 实际应用的经验值
        bool leveledUp = false;

        while (remainingExp > 0)
        {
            int targetExp = allyDef.GetExpRequiredTonNextLevel(Level); //获取当前等级所需的经验值
            if (targetExp == 0)
            {
                CurrentExp = 0;
                break;
            }

            int need = targetExp - CurrentExp; // 计算当前经验值与目标经验值之间的差值
            int gain = Mathf.Min(need, remainingExp); // 计算实际应用的经验值，取差值与剩余经验值中的较小
            
            CurrentExp += gain; // 增加经验值
            remainingExp -= gain; // 减少剩余经验值
            appliedExp += gain; // 增加实际应用的经验值

            if (CurrentExp >= targetExp)
            {
                Level++;
                leveledUp = true;
                CurrentExp = 0; // 减少经验值，使其不超过当前等级所需的经验值
            }
        }
        
        if (leveledUp)
            HealFull();
        
        return appliedExp;
    }

    #endregion
}












