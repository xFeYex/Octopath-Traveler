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

        var stats = GetTotalStats();
        CurrentHP = stats.MaxHP;
        CurrentSP = stats.MaxSP;
        CurrentBP = 0;
        
        ApplyInitialEquipment();
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

    #region 装备数据

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
}