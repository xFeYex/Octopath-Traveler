
using System;
using TMPro;
using UnityEngine.UI;
using Utils;

public class ShopPanelController : PanelController
{
    [Header("一级按钮与金额")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private TMP_Text currentAmountText;

    [Header("二级列表")] 
    [SerializeField] private ItemPanelController itemPanel;

    [Header("交互区域")] 
    [SerializeField] private CanvasGroup leftPart;
    [SerializeField] private CanvasGroup itemPanelCanvasGroup;
    
    [Header("confirm popup")]
    [SerializeField] private RectTransform confirmPopup;
    [SerializeField] private TMP_Text popupTitile;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private Button confirmButton; 
    [SerializeField] private Button cancelButton;

    public override Type PanelActionType => typeof(ShopAction);

    private PanelType _currentShopType;
    private ItemDefinitionSO _pendingItem;
    private ShopAction currentShopAction => (ShopAction)CurrentAction;

    /* ----------------------------------------------------------------------------------------- */

    private void Awake()
    {
        ReBindButtons(buyButton, OpenBuyPanel);
        ReBindButtons(sellButton, OpenSellPanel);
        ReBindButtons(confirmButton, ExecuteTransaction);
        
        confirmPopup.gameObject.SetActive(false);
    }


    public override void SetupPanel(ActionBase actionBase)
    {
        base.SetupPanel(actionBase);
        SetDefaultSelection();
        UpdateCurrencyDisplay();
    }

    private void OpenBuyPanel()
    {
        OpenItemPanel(PanelType.Buy);
    }

    private void OpenSellPanel()
    {
        OpenItemPanel(PanelType.Sell);
    }

    private void OpenItemPanel(PanelType panelType)
    {
        _currentShopType = panelType;
        leftPart.interactable = false;
        itemPanel.gameObject.SetActive(true);
        
        itemPanel.SetupPanel(panelType, CurrentAction, OpenConfirmPopup);
    }

    private void OpenConfirmPopup(ItemDefinitionSO itemDefinition)
    {
        _pendingItem = itemDefinition;
        confirmPopup.gameObject.SetActive(true);
        itemPanelCanvasGroup.interactable = false;
        
        FirstButton = confirmButton;
        SetDefaultSelection();
        
        // 装饰弹窗逻辑
        
        if (_currentShopType == PanelType.Buy)
            SetupBuyPopup(itemDefinition);
        else
            SetupSellPopup(itemDefinition);
    }

    private void SetupBuyPopup(ItemDefinitionSO itemDefinition)
    {
        // 判断玩家是否有足够的货币购买
        bool canAfford = InventoryManager.Instance.Currency >= itemDefinition.BuyPrice;

        popupTitile.text = "确认购买一下物品吗？ :";
        popupText.text = $"{itemDefinition.ItemName}\n价格: ￥{itemDefinition.BuyPrice}";
        
        confirmButton.interactable = canAfford; // 如果不能购买则禁用确认按钮
        (canAfford ? confirmButton : cancelButton).Select(); // 如果不能购买则默认选中取消按钮
    }

    private void SetupSellPopup(ItemDefinitionSO  itemDefinition)
    {
        popupTitile.text = "确认购买一下物品吗？";
        popupText.text = $"{itemDefinition.ItemName}\n获得: ￥{itemDefinition.BuyPrice}";

        confirmButton.interactable = true;
        confirmButton.Select();
    }
    

    public override bool HandleCancelInput()
    {
        if (confirmPopup.gameObject.activeSelf)
        {
            confirmPopup.gameObject.SetActive(false);
            itemPanelCanvasGroup.interactable = true;
            if (itemPanel.gameObject.activeInHierarchy)
            {
                itemPanel.SetDefaultSelection(); // 设置默认
            }
            return true;
        }
        
        if (!itemPanel.gameObject.activeSelf)
        {
            return false;
        }

        itemPanel.gameObject.SetActive(false);
        leftPart.interactable = true;
        _pendingItem = null;
        FirstButton = _currentShopType == PanelType.Buy ? buyButton : sellButton;
        SetDefaultSelection();
        return true;
    }

    private void UpdateCurrencyDisplay()
    {
        if (currentAmountText == null)
            return;
        
        var inventory = InventoryManager.Instance;
        int currentMonery = inventory.Currency;
        currentAmountText.text = $"持有金额: {currentMonery.ToString()}";
    }
    
    private void ExecuteTransaction()
    {
        // Action来触发真实交易
        currentShopAction.TryExecuteTransaction(_currentShopType, _pendingItem);
        confirmPopup.gameObject.SetActive(false);
        itemPanelCanvasGroup.interactable = true;
        UpdateCurrencyDisplay();
        // itemPanel刷新持有数
        if (_currentShopType == PanelType.Buy)
        {
            itemPanel.RefreshItemQuantity(_pendingItem); // 买了刷新数量
            itemPanel.SetDefaultSelection();
            return;
        }
        // 卖的光了从itemPanel里移除按钮
        
        var remaining = EquipmentService.GetAvailableItemCount(InventoryManager.Instance, PartyManager.Instance, _pendingItem);
        
        if (remaining > 0)
        {
            itemPanel.RefreshItemQuantity(_pendingItem);
            itemPanel.SetDefaultSelection();
            return;
        }
        itemPanel.RemoveItemButton(_pendingItem);
    }
}