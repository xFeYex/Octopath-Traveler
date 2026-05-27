
using System;
using Utils;

public class ItemPanelController : PanelController
{
    [Header("Item Panel")]
    [SerializeField] private ItemButton itemButtonPrefab;
    [SerializeField] private Transform itemButtonParent;
    
    private readonly List<ItemButton> _itemButtons = new(); // 物品按钮列表
    private PanelType _currentPanelType;
    private Action<ItemDefinitionSO> _onItemClick;
    
    /* ---------------------------------------------------------------------------------- */

    public void SetupPanel(PanelType panelType, ActionBase actionBase, Action<ItemDefinitionSO> onItemClick = null)
    {
        if (actionBase != null)
            base.SetupPanel(actionBase);
        
        _currentPanelType = panelType;
        _onItemClick = onItemClick;
        
        ClearItemView();
        
        var inventoryManager = InventoryManager.Instance;
        switch (panelType)
        {
            case  PanelType.Buy:
                BuildBuyList(inventoryManager);
                break;
            case  PanelType.Sell:
                BuildSellList(inventoryManager);
                break;
            case  PanelType.Item:
                BuildItemList(inventoryManager);
                break;
        }

        if (_itemButtons.Count == 0)
        {
            FirstButton = null;
            return;
        }
        FirstButton = _itemButtons[0].CurrentButton;
        SetDefaultSelection();
    }
    
    private void ClearItemView()
    {
        foreach (var itemButton in _itemButtons)
        {
            Destroy(itemButton.gameObject);
        }
        _itemButtons.Clear();
        FirstButton = null;
    }

    private void BuildBuyList(InventoryManager inventory)
    {
        ShopAction shopAction = (ShopAction)CurrentAction;
        foreach (var item in shopAction.itemsBag)
        {
            int ownedQuantity = inventory.GetItemQuantity(item.ItemDefinition);
            AddItemButton(new InventoryItem(item.ItemDefinition, ownedQuantity));
        }
    }
    
    private void BuildSellList(InventoryManager inventory)
    {
        foreach (var item in inventory.CurrentInventory)
        {
            int availableForSell = EquipmentService.GetAvailableItemCount(inventory, PartyManager.Instance, item.ItemDefinition);

            if (availableForSell > 0)
            {
                AddItemButton(new InventoryItem(item.ItemDefinition, availableForSell));
                continue;
            }

            if (availableForSell == 0
                && item.ItemDefinition != null
                && item.ItemDefinition.itemType == ItemType.Equipment
                && item.Quantity == 1)
            {
                AddItemButton(new InventoryItem(item.ItemDefinition, 0), false, true);
            }
                
        }
    }

    private void BuildItemList(InventoryManager inventory)
    {
        foreach (var item in inventory.CurrentInventory)
        {
            AddItemButton(item);
        }
    }

    private void AddItemButton(InventoryItem inventoryItem, bool interactable = true, bool equippedNameFormat = false)
    {
        ItemButton itemButton = Instantiate(itemButtonPrefab, itemButtonParent);

        if (itemButton is ShopItemButton shopItemButton)
        {
            shopItemButton.SetupButton(inventoryItem, _currentPanelType, _onItemClick);
        }
        else
        {
            itemButton.SetupButton(inventoryItem, _onItemClick);
        }
        
        if (equippedNameFormat)
            itemButton.SetEquippedFormat();
        
        itemButton.CurrentButton.interactable = interactable;
        _itemButtons.Add(itemButton);
    }

    public void RefreshItemQuantity(ItemDefinitionSO itemDefinition)
    {
        foreach (var itemButton in _itemButtons)
        {
            if (itemButton.CurrentItemDefinition == itemDefinition)
            {
                int newQuantity = InventoryManager.Instance.GetItemQuantity(itemDefinition);
                itemButton.UpdateQuantity(newQuantity);
                break;
            }
        }
    }

    internal void RemoveItemButton(ItemDefinitionSO pendingItem)
    {
        // 找到当前物品对应按钮
        int targetIndex = _itemButtons.FindIndex(itemButton => itemButton.CurrentItemDefinition == pendingItem);
        ItemButton targetButton = _itemButtons[targetIndex];
        _itemButtons.RemoveAt(targetIndex);
        Destroy(targetButton.gameObject);
        // 先继续出下一个按钮是什么，再删除当前按钮
        // 重置FirstButton
        FirstButton = _itemButtons.Count > 0 ? _itemButtons[0].CurrentButton : null;

        if (_itemButtons.Count > 0)
        {
            int newIndex = Mathf.Min(targetIndex, _itemButtons.Count - 1);
            FirstButton = _itemButtons[newIndex].CurrentButton;
            SetDefaultSelection();
        }
    }
}