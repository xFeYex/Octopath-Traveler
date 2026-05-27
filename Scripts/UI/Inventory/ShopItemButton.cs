
using System;
using TMPro;
using Utils;

public class ShopItemButton : ItemButton
{
    [Header("Shop Item Button")] 
    [SerializeField] private TMP_Text priceText;

    public void SetupButton(InventoryItem  inventoryItem, PanelType panelType, Action<ItemDefinitionSO> onItemClick)
    {
        base.SetupButton(inventoryItem, onItemClick);
        priceText.text = panelType == PanelType.Buy
            ? $"${inventoryItem.ItemDefinition.BuyPrice}" 
            : $"${inventoryItem.ItemDefinition.SellPrice}";
    }
}