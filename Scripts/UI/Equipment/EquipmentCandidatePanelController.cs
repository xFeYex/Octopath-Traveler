
using System;
using TMPro;
using UnityEngine.UI;
using Utils;

public class EquipmentCandidatePanelController : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private Image slotIconImage;
    
    [Header("List")]
    [SerializeField] private RectTransform candidateListRoot;
    [SerializeField] private EquipmentCandidateButton candidateButtonPrefab;
    
    // 缓存
    private readonly List<EquipmentCandidateButton> _buttons = new();
    private readonly List<EquipmentItemSO> _candidates = new();
    // 缓存 - 用C#标准事件代替逐层转递的 Action 标准
    public event Action<int> OnSelectIndexChanged;
    public event Action<int> OnCandidateClicked;
    
    public EquipmentItemSO GetCandidate(int index) => _candidates[index];

    public Button GetFirstButton() => _buttons[0].Button;
    
    /* ---------------------------------------------------------------------------------------------------- */

    public void OpenForSlot(EquipSlot slot, CharacterRuntimeData member, string slotDisplayName, Sprite slotIcon)
    {
        RefreshHeader(slotDisplayName, slotIcon);
        RebuildCandidates(slot);
    }

    private void RebuildCandidates(EquipSlot slot)
    {
        _candidates.Clear();
        ClearButtons();
        
        InventoryManager inventory =  InventoryManager.Instance;
        PartyManager  partyManager = PartyManager.Instance;
        
        _candidates.AddRange(EquipmentService.BuildCandidates(inventory, slot));

        for (int i = 0; i < _candidates.Count; i++)
        {
            int availableCount = EquipmentService.GetAvailableItemCount(inventory, partyManager, _candidates[i]);
            EquipmentCandidateButton candidateButton = Instantiate(candidateButtonPrefab, candidateListRoot);

            bool isInteractable = _candidates[i] == null || availableCount > 0;
            candidateButton.Setup(i, _candidates[i], availableCount, isInteractable, inventory.IconSet);
            _buttons.Add(candidateButton);

            candidateButton.onClick += HandCandidateClicked;
            candidateButton.onSelect += HandleCandidateSelected;
        }
    }

    private void RefreshHeader(string slotDisplayName, Sprite slotIcon)
    {
        slotNameText.text = slotDisplayName;
        slotIconImage.sprite = slotIcon;
        slotIconImage.enabled = slotIcon != null;
    }

    public void Close()
    {
        _candidates.Clear();
        ClearButtons();
    }

    private void ClearButtons()
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (_buttons[i] != null)
                Destroy(_buttons[i].gameObject);
        }
        
        _buttons.Clear();
    }
    
    private void HandCandidateClicked(int index)
    {
        OnCandidateClicked?.Invoke(index);
    }

    private void HandleCandidateSelected(int index)
    {
        OnSelectIndexChanged?.Invoke(index);
    }
    
}
