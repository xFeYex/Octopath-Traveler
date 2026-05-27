
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class EquipmentSlotButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private Image slotIconImage;
    
    private CanvasGroup canvasGroup;

    private Button _button;
    private int _index;
    private Action<int> _onSelect;
    private Action<int> _onClicked;

    public Button Button
    {
        get
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
                _defaultNavigation = _button.navigation;
            }
            return _button;
        }
    }

    private bool _isSlotUsable = true;  // 该槽位是否可用（例如，某些职业可能无法使用特定的装备槽）
    private bool _isInputEnabled = true; // 是否允许输入（例如，在某些UI状态下可能暂时禁用输入）

    public Sprite SlotIconSprite => slotIconImage.sprite;
    
    private Navigation _defaultNavigation;
    
    /* ---------------------------------------------------------------------- */

    private void Awake()
    {
        Button.onClick.AddListener(HandleClick);
        canvasGroup = GetComponent<CanvasGroup>();
    }
    
    /* ---------------------------------------------------------------------- */

    public void Setup(EquipmentItemSO equipmentItem, int index, Action<int> onSelect, Action<int> onClicked, bool isSlotUsable)
    {
        _index = index;
        _onSelect = onSelect;
        _onClicked = onClicked;
        _isSlotUsable = isSlotUsable;
        
        string displayName = equipmentItem != null ? equipmentItem.ItemName : "未装备";
        slotNameText.text = displayName;
        
        ApplyButtonInteractableState();
    }

    public void SetInputEnabled(bool isEnabled)
    {
        _isInputEnabled = isEnabled;
        ApplyButtonInteractableState();
    }
    
    // 设置按钮是否可交互
    private void ApplyButtonInteractableState()
    {
        bool isInteractable = _isSlotUsable && _isInputEnabled;
        Button.interactable = isInteractable;
        canvasGroup.alpha = _isSlotUsable ? 1f : 0f;
        canvasGroup.interactable = isInteractable;
        
        Navigation navigation = _defaultNavigation;
        
        if (!isInteractable)
            navigation.mode = Navigation.Mode.None;
        
        Button.navigation = navigation;
    }

    private void HandleClick()
    {
        if (!_button.interactable)
            return;
        _onClicked?.Invoke(_index);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_button.interactable)
            return;
        _onSelect?.Invoke(_index);
    }
}