
using Utils;

public class EquipmentService
{
    public static List<EquipmentItemSO> BuildCandidates(InventoryManager inventory, EquipSlot targetSlot)
    {
        List<EquipmentItemSO> result = new() { null };

        for (int i = 0; i < inventory.CurrentInventory.Count; i++)
        {
            var item = inventory.CurrentInventory[i];
            
            if (item == null || item.Quantity <= 0) continue;
            if (item.ItemDefinition is not EquipmentItemSO equipmentItem) continue;
            if (!IsSlotCompatible(equipmentItem, targetSlot)) continue;
            
            result.Add(equipmentItem);
        }
        
        return result;
    }

    private static bool IsSlotCompatible(EquipmentItemSO item, EquipSlot targetSlot)
    {
        if (item == null) return false;

        return item.category switch
        {
            EquipmentCategory.Weapon => targetSlot == (EquipSlot)((int)item.WeaponType - 1),
            EquipmentCategory.Shield => targetSlot == EquipSlot.Shield,
            EquipmentCategory.Head => targetSlot == EquipSlot.Head,
            EquipmentCategory.Body => targetSlot == EquipSlot.Body,
            EquipmentCategory.Accessory => targetSlot == EquipSlot.Accessory1 || targetSlot == EquipSlot.Accessory2,
            _ => false,
        };
    }

    /// <summary>
    /// 获取物品的可用数量
    /// 如果是非装备物品，直接返回总持有量
    /// </summary>
    public static int GetAvailableItemCount(InventoryManager inventory, PartyManager partyManager, ItemDefinitionSO item)
    {
        if (item is null) return 0;
        
        int totalQuantity = inventory.GetItemQuantity(item);
        if (totalQuantity <= 0) return 0;

        int equippedCount = 0;
        for (int i = 0; i < partyManager.PartyMembers.Count; i++)
        {
            equippedCount += partyManager.PartyMembers[i].GetEquippedItemCount(item);
        }
        
        return totalQuantity - equippedCount;
    }

    public static StatBlock BuildPreviewTotalStats(CharacterRuntimeData member, EquipSlot slot,
        EquipmentItemSO previewItem)
    {
        if (member is null) return StatBlock.zero;

        StatBlock previewTotal = member.GetTotalStats(); // 先获取当前总属性
        EquipmentItemSO currentItem = member.GetEquippedItem(slot); // 获取当前装备的物品

        // 根据previewItem和currentItem的差异更新previewTotal
        if (currentItem != null)
        {
            previewTotal += currentItem.statBonus * -1f;
        }

        if (previewItem != null)
        {
            previewTotal += previewItem.statBonus;
        }

        return previewTotal;
    }
}