
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using Utils;

public class EquipmentPanelController : PanelController
{
    private enum FocusLayer // 目前正在激活的UI层级
    {
        CharacterTabs,
        SlotList,
        CandidateList
    }
    [Header("Character Tabs")] [SerializeField]
    private RectTransform tabRoot;

    [SerializeField] private CanvasGroup tabCanvasGroup;
    [SerializeField] private EquipmentCharacterTabButton tabPrefab;

    [Header("Slot Button")]
    [SerializeField] private EquipmentSlotButton[] slotButtons;
    [SerializeField] private CanvasGroup slotListCanvasGroup;
    
    [Header("Left Category Name")]
    [SerializeField] private TMP_Text[] leftCategoryNameTexts;
    [SerializeField] private Color leftCategoryEnabledColor = Color.black;
    [SerializeField] private Color leftCategoryDisabledColor = new Color(0.72f, 0.72f, 0.72f);
    
    [Header("Stat Preview")]
    [SerializeField] private EquipmentStatPreviewPanel statPreviewPanel;
    
    [Header("Header")]
    [SerializeField] private TMP_Text memberNameText;

    [Header("Candidate Panel")] 
    [SerializeField] private GameObject candidatePanelRoot;
    [SerializeField] private EquipmentCandidatePanelController candidatePanelController;
    
    #region 运行时缓存

    private List<EquipmentCharacterTabButton> _tabButtons = new();
    private List<CharacterRuntimeData> _partyMembers = new();

    private int _memberIndex;
    private int _slotIndex;
    
    private CharacterRuntimeData CurrentMember => 
        _memberIndex >= 0 && _memberIndex < _partyMembers.Count ? _partyMembers[_memberIndex] : null;

    private FocusLayer _focusLayer = FocusLayer.CharacterTabs;
    #endregion
    
    /* ---------------------------------------------------------------------- */

    private void Awake()
    {
        candidatePanelController.OnSelectIndexChanged += OnCandidateSelected;
        candidatePanelController.OnCandidateClicked += OnCandidateClicked;
    }

    private void OnDestroy()
    {
        candidatePanelController.OnSelectIndexChanged -= OnCandidateSelected;
        candidatePanelController.OnCandidateClicked -= OnCandidateClicked;
    }

    /* ---------------------------------------------------------------------- */
    public void SetupWithPartyMember(List<CharacterRuntimeData> partyMembers)
    {
        _partyMembers.Clear();
        if (partyMembers == null) return;

        for (int i = 0; i < partyMembers.Count; i++)
        {
            var member = partyMembers[i];
            if (member == null) continue;
            
            // TODO: 绑定人物属性
            _partyMembers.Add(member);
        }
        
        BuildCharacterTabs();
        ClampMemberIndex();
        RefreshCurrentMemberView();
        EnterCharacterTabLayer();
    }

    public override bool HandleCancelInput()
    {
        if (candidatePanelRoot.activeInHierarchy)
        {
            EnterSlotLayer();
            return true;
        }

        if (_focusLayer == FocusLayer.SlotList)
        {
            EnterCharacterTabLayer();
            return true;
        }
        
        return false;
    }

    private void RefreshLeftCategoryColors(CharacterRuntimeData member)
    {
        for (int i = 0; i < leftCategoryNameTexts.Length; i++)
        {
            leftCategoryNameTexts[i].color = IsSlotUsableForMember(member, i) 
                ? leftCategoryEnabledColor 
                : leftCategoryDisabledColor;
        }
    }
    
    private void RefreshCurrentMemberView()
    {
        // todo: 刷新当前人物装备
        for (int i = 0; i < FixedSlotOrder.Length; i++)
        {
            EquipmentSlotButton slotButton = slotButtons[i];
            EquipSlot slot = FixedSlotOrder[i];
            EquipmentItemSO equippedItem = CurrentMember?.GetEquippedItem(slot);
            
            // todo: 装备槽回调函数
            slotButton.gameObject.SetActive(true);
            slotButton.Setup(equippedItem, i, OnSlotSelected, OnSlotClicked, IsSlotUsableForMember(CurrentMember, i));
        }
        
        // todo:装备插槽对比
        statPreviewPanel.Refresh(CurrentMember.GetTotalStats(), CurrentMember.GetTotalStats(), false);
        UpdateTabSelectionVisual(); // 更新Tab选中状态
        RefreshLeftCategoryColors(CurrentMember); // 更新左侧类别颜色
    }

    private bool IsSlotUsableForMember(CharacterRuntimeData member, int slotIndex)
    {
        // 非武器槽位默认可用
        if (slotIndex >= weaponSlotTypes.Length)
            return true;
        
        return member.Definition is AllyDefinitionSO allyDef 
               && allyDef.CanEquipWeaponType(weaponSlotTypes[slotIndex]);
    }

    private void SetSlotListInteractable(bool interactable)
    {
        slotListCanvasGroup.interactable = interactable;
        for (int i = 0; i < slotButtons.Length; i++)
        {
            slotButtons[i].SetInputEnabled(interactable);
        }
    }
    
    #region Tab切换人物

    private void BuildCharacterTabs()
    {
        ClearTabButton();
        for (int i = 0; i < _partyMembers.Count; i++)
        {
            var tab = Instantiate(tabPrefab, tabRoot);
            tab.Setup(_partyMembers[i], i, OnTabSelected, OnTabClick);
            tab.SetSelectedVisual(i == _memberIndex);
            _tabButtons.Add(tab);
        }
    }
    
    private void ClearTabButton()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            Destroy(_tabButtons[i].gameObject);
        }
        _tabButtons.Clear();
    }
    
    private void ClampMemberIndex()
    {
        _memberIndex = _tabButtons.Count > 0 ? 
            Mathf.Clamp(_memberIndex, 0, _partyMembers.Count - 1) : 0;
    }
    
    private void SetCharacterTabInteractable(bool interactable)
    {
        tabCanvasGroup.interactable = interactable;
        for (int i = 0 ; i < _tabButtons.Count;i++)
            _tabButtons[i].Button.interactable = interactable;
    }
    
    // 进入人物选择层
    private void EnterCharacterTabLayer()
    {
        _focusLayer = FocusLayer.CharacterTabs;
        SetCharacterTabInteractable(true);
        UpdateTabSelectionVisual();
        SetSlotListInteractable(false);
        
        _memberIndex = Mathf.Clamp(_memberIndex, 0, _partyMembers.Count - 1);
        FirstButton = _tabButtons[_memberIndex].Button;
        SetDefaultSelection();
    }
    
    private void UpdateTabSelectionVisual()
    {
        for (int i = 0; i < _tabButtons.Count; i++)
        {
            _tabButtons[i].SetSelectedVisual(i == _memberIndex);
        }
    }
    
    private void OnTabClick(int index)
    {
        // TODO: 切换人物装备
        OnTabSelected(index);
        EnterSlotLayer();
    }

    private void OnTabSelected(int index)
    {
        _memberIndex = index;
        RefreshCurrentMemberView();
    }
    
    #endregion

    #region 可选择装备面板

    private void OpenCandidateList(int slotIndex)
    {
        // 打开可选装备面板时，禁用装备槽位和人物Tab的交互，防止切换人物或装备槽位
        SetSlotListInteractable(false);
        SetCharacterTabInteractable(false);
        
        candidatePanelRoot.SetActive(true);
        EquipSlot slot = FixedSlotOrder[slotIndex];
        string slotName = leftCategoryNameTexts[slotIndex].text;
        
        candidatePanelController.OpenForSlot(slot, CurrentMember, slotName, slotButtons[slotIndex].SlotIconSprite);

        _focusLayer = FocusLayer.CandidateList;
        _slotIndex = slotIndex;
        
        FirstButton = candidatePanelController.GetFirstButton();
        SetDefaultSelection();
        UpdateCandidatePreview(0);
    }

    private void CloseCandidateListInternal()
    {
        candidatePanelController.Close();
        candidatePanelRoot.SetActive(false);
    }

    private void OnCandidateSelected(int index)
    {
        // 更新能力值变化
        UpdateCandidatePreview(index);
    }

    private void OnCandidateClicked(int index)
    {
        // 确认装备更换，刷新界面
        var slot  = FixedSlotOrder[_slotIndex];
        EquipmentItemSO targetItem = candidatePanelController.GetCandidate(index);
        if (CurrentMember.GetEquippedItem(slot) == targetItem) return;
        
        CurrentMember.SetEquippedItem(slot, targetItem);
        RefreshCurrentMemberView();
        OpenCandidateList(_slotIndex);
    }

    private void UpdateCandidatePreview(int candidateIndex)
    {
        if (!IsSlotIndexUsable(_slotIndex)) return;
        
        EquipSlot slot = FixedSlotOrder[_slotIndex];
        EquipmentItemSO previewItem = candidatePanelController.GetCandidate(candidateIndex);
        
        RefreshPreview(slot, previewItem, true);
    }

    private void RefreshPreview(EquipSlot slot, EquipmentItemSO previewItem, bool isInPreviewMode)
    {
        var currentTotal = CurrentMember.GetTotalStats();
        var previewTotal = EquipmentService.BuildPreviewTotalStats(CurrentMember, slot, previewItem);
        
        statPreviewPanel.Refresh(currentTotal, previewTotal, isInPreviewMode);
    }
    #endregion
    
    #region 装备Slot Layer

    private void OnSlotSelected(int index)
    {
        if (CurrentMember == null || statPreviewPanel == null) return;
        
        statPreviewPanel.Refresh(CurrentMember.GetTotalStats(), CurrentMember.GetTotalStats(), false);
    }

    private void OnSlotClicked(int index)
    {
        OnSlotSelected(index);
        OpenCandidateList(index);
    }
    
    private void EnterSlotLayer()
    {
        _focusLayer = FocusLayer.SlotList;
        CloseCandidateListInternal();
        SetCharacterTabInteractable(false); // 进入装备槽位层时禁用Tab交互，防止切换人物
        EnsureSelectedSlotValid();
        SetSlotListInteractable(true);
        
        if (_slotIndex >= slotButtons.Length) return;
        FirstButton = slotButtons[_slotIndex].Button;
        SetDefaultSelection();
    }

    private void EnsureSelectedSlotValid()
    {
        if (IsSlotIndexUsable(_slotIndex)) return;
        
        for (int i = 0 ; i < FixedSlotOrder.Length; i++)
        {
            if (IsSlotIndexUsable(i))
            {
                _slotIndex = i;
                return;
            }
        }
        
        _slotIndex = -1;
    }

    private bool IsSlotIndexUsable(int index) => 
        index >= 0 && index < FixedSlotOrder.Length && IsSlotUsableForMember(CurrentMember, index);
    
    #endregion

    #region 固定槽位

    private readonly WeaponType[] weaponSlotTypes =
    {
        WeaponType.Sword,
        WeaponType.Spear,
        WeaponType.Dagger,
        WeaponType.Axe,
        WeaponType.Bow,
        WeaponType.Staff
    };

    private readonly EquipSlot[] FixedSlotOrder =
    {
        EquipSlot.Sword,
        EquipSlot.Spear,
        EquipSlot.Dagger,
        EquipSlot.Axe,
        EquipSlot.Bow,
        EquipSlot.Staff,
        EquipSlot.Shield,
        EquipSlot.Head,
        EquipSlot.Body,
        EquipSlot.Accessory1,
        EquipSlot.Accessory2
    };

    #endregion
}
