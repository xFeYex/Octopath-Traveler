
using System;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipmentCandidateButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text ownedCountText;

    [Header("Color")] 
    [SerializeField] private Color normalTextColor;
    [SerializeField] private Color disabledTextColor;
    
    private Button _button;
    private int _index;
    public Action<int> onClick;
    public Action<int> onSelect;
    
    public Button Button => _button;
    
    /* ---------------------------------------------------------------------------- */

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(HandleClick);
    }

    public void Setup(int index, EquipmentItemSO item, int ownedCount, bool isInteractable, ItemIconSetSO iconSet)
    {
        _index = index;
        _button.interactable = isInteractable;

        if (item != null)
        {
            itemNameText.text = item.ItemName;
            ownedCountText.text = ownedCount.ToString();
            itemIcon.sprite = iconSet.GetIcon(item.itemIconKey);
            itemIcon.enabled = true;
        }
        else
        {
            itemNameText.text = "未装备";
            ownedCountText.text = "-";
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }
        
        itemNameText.color = isInteractable ? normalTextColor : disabledTextColor;
        ownedCountText.color = isInteractable ? normalTextColor : disabledTextColor;
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_button.interactable) return;
        onSelect?.Invoke(_index);
    }

    private void HandleClick()
    {
        if (!_button.interactable) return;
        onClick?.Invoke(_index);
    }
}
