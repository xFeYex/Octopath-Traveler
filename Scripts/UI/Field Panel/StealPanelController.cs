
using System;
using TMPro;
using UnityEngine.UI;

public class StealPanelController : PanelController
{
     [Header("Steal Panel")]
     [SerializeField] private StealItemButton stealItemButtonPrefab;
     [SerializeField] private RectTransform contentRoot;
     
     [Header("confirm popup")]
     [SerializeField] private RectTransform confirmPopup;
     [SerializeField] private TMP_Text popupText;
     [SerializeField] private Button confirmButton; 
     [SerializeField] private Button cancelButton;

     private readonly List<StealItemButton> _stealItemButtons = new();
     public override Type PanelActionType => typeof(StealAction);
     private StealAction CurentStealAction => (StealAction) CurrentAction;

     private ItemDefinitionSO _pendingItem;
     /* ---------------------------------------------------------------------- */

     public override void SetupPanel(ActionBase actionBase)
     {
          base.SetupPanel(actionBase);
          
          confirmPopup.gameObject.SetActive(false);

          RefreshItemList();
     }

     // 每次打开面板时刷新物品列表，确保显示最新的可偷取物品
     private void RefreshItemList()
     {
          ClearItemList();

          foreach (InventoryItem item in CurentStealAction.itemsToSteal)
          {
               StealItemButton stealItemButton = Instantiate(stealItemButtonPrefab, contentRoot);
               stealItemButton.SetupButton(item, OpenConfirmPopup);
               _stealItemButtons.Add(stealItemButton);
          }
          
          if(_stealItemButtons.Count == 0)
               return;
          
          FirstButton = _stealItemButtons[0].CurrentButton;
          SetDefaultSelection();
     }

     private void OpenConfirmPopup(ItemDefinitionSO itemDefinition)
     {
          _pendingItem = itemDefinition;
          FirstButton = confirmButton;
          confirmPopup.gameObject.SetActive(true);
          cancelButton.gameObject.SetActive(true);
          
          popupText.text = $"{itemDefinition.name} 成功率: {itemDefinition.RarityWeight}%";
          
          SetButtonInteractable(false); // 禁用其他按钮
          
          // 绑定确认和取消按钮事件
          ReBindButtons(confirmButton, OnConfirm);
          ReBindButtons(cancelButton, ClosePopup);
          
          SetDefaultSelection();
     }

     protected override void OnConfirm()
     {
          bool success = CurentStealAction.TrySteal(_pendingItem);
          
          popupText.text = success ? $"偷取成功" : $"偷取失败!";
          
          cancelButton.gameObject.SetActive(false);
          ReBindButtons(confirmButton, ClosePopup);
     }

     private void ClosePopup()
     {
          HidePopup();
          RefreshItemList();
     }

     private void HidePopup()
     {
          _pendingItem = null;
          confirmPopup.gameObject.SetActive(false);
          SetButtonInteractable(true);
     }

     // 处理取消输入，如果确认弹窗打开则关闭它，否则不处理
     public override bool HandleCancelInput()
     {
          if (confirmPopup.gameObject.activeSelf)
          {
               ClosePopup();

               return true; // 已处理取消输入
          }
          return false; // 没有处理取消输入
     }

     private void SetButtonInteractable(bool interactor)
     {
          foreach (StealItemButton stealItemButton in _stealItemButtons)
               stealItemButton.CurrentButton.interactable = interactor;
     }
     
     private void ClearItemList()
     {
          foreach (StealItemButton stealItemButton in _stealItemButtons)
          {
               Destroy(stealItemButton.gameObject);
          }
          _stealItemButtons.Clear();
          FirstButton = null;
     }
}