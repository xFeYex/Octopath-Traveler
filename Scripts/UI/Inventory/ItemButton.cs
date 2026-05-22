using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class ItemButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDescription;
    [SerializeField] private TMP_Text itemQuality;
    [SerializeField] private GameObject itemTips;
    
    protected Button _button; // 当前按钮
    public Button CurrentButton => _button;
    
    protected InventoryItem _currentItem; // 当前物品
    protected ItemDefinitionSO CurrentItemDefinition => _currentItem.ItemDefinition;
    
    private Action<ItemDefinitionSO> _onItemClick; // 点击事件回调
    
    /* --------------------------------------------------------------------------- */

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    /* --------------------------------------------------------------------------- */

    public virtual void SetupButton(InventoryItem inventoryItem) => SetupButton(inventoryItem, null); // 默认不设置点击回调
    
    public virtual void SetupButton(InventoryItem inventoryItem, Action<ItemDefinitionSO> onItemClick)
    {
        _currentItem = inventoryItem;
        _onItemClick = onItemClick;
        
        itemIcon.sprite = InventoryManager.Instance.IconSet.GetIcon(inventoryItem.ItemDefinition.itemIconKey);
        itemName.text = inventoryItem.ItemDefinition.ItemName;
        itemDescription.text = inventoryItem.ItemDefinition.ItemDescription;
        
        if(itemQuality != null)
            itemQuality.text = inventoryItem.Quantity.ToString();
    }
    
    
    protected virtual void OnClick()
    {
        if(_onItemClick != null)
            _onItemClick.Invoke(CurrentItemDefinition);
    }
    
    #region UI状态回调

    public void OnSelect(BaseEventData eventData)
    {
        itemTips.SetActive(true);
    }
    
    public void OnDeselect(BaseEventData eventData)
    {
        itemTips.SetActive(false);
    }
    
    #endregion

    
}
