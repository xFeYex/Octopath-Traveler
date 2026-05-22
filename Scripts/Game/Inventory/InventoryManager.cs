
using Utils;

public class InventoryManager : Singleton<InventoryManager>
{
    [Header("Icon Set")]
    public ItemIconSetSO IconSet;
    
    [Header("Inventory")]
    public List<InventoryItem> CurrentInventory = new List<InventoryItem>();
    
    /* ---------------------------------------------------------------- */

    #region 对外调用接口

    public void AddItem(ItemDefinitionSO itemDefinition, int quantity)
    {
        foreach (var item in CurrentInventory)
        {
            if (item.ItemDefinition != itemDefinition)
                continue;
            item.Quantity += quantity;
            return;
        }
        CurrentInventory.Add(new InventoryItem(itemDefinition, quantity));
    }

    public void RemoveItem(ItemDefinitionSO itemDefinition, int quantity)
    {
        for (int i = 0; i < CurrentInventory.Count; i++)
        {
            if (CurrentInventory[i].ItemDefinition != itemDefinition)
                continue;
            
            CurrentInventory[i].Quantity -= quantity;
            
            if (CurrentInventory[i].Quantity <= 0)
                CurrentInventory.RemoveAt(i);
            
            return;
        }
    }

    public int GetItemQuantity(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null) return 0;

        foreach (var item in CurrentInventory)
        {
            if  (item.ItemDefinition == itemDefinition)
                return item.Quantity;
        }
        return 0;
    }

    #endregion
    
}

/* ---------------------------------------------------------------- */

[System.Serializable]
public class InventoryItem
{
    public ItemDefinitionSO ItemDefinition;
    public int Quantity;
    
    /* ---------------------------------------------------------------- */

    public InventoryItem(ItemDefinitionSO item, int quantity)
    {
        ItemDefinition = item;
        Quantity = quantity;
    }

    public bool IsEquipment => ItemDefinition != null && ItemDefinition.itemType == ItemType.Equipment;
    public bool IsConsumable => ItemDefinition != null && ItemDefinition.itemType == ItemType.Consumable;
}
