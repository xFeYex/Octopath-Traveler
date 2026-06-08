
using System;
using UnityEngine.UI;
using Utils;

public class BattleCommandUI : Singleton<BattleCommandUI>
{
    [Header("主命令面板")]
    [SerializeField] private CanvasGroup commandMenuCanvasGroup;

    [Header("二级菜单")] 
    [SerializeField] private RectTransform commandMenuPanel;
    [SerializeField] private SkillButton skillButtonPrefab;
    
    

    [Header("Buttons")] 
    [SerializeField] private Button btnAttack;
    [SerializeField] private Button btnSkill;
    [SerializeField] private Button btnItem;
    [SerializeField] private Button btnDefend;
    [SerializeField] private Button btnEscape;
    
    private BattleEntity _currentEntity;
    
    private readonly List<GameObject> _spawnedSubMenuButtons = new();
    
    #region 回调
    
    // 一级命令选择完成后，回传给上层调用者（通常是PLayerInputState）
    // 用来告诉战斗状态机：玩家这次选的是Attack/Defend/Escape这类主命令。
    private Action<BattleCommandType> _onCommandSelected;
    // 技能按钮点下后，回传给上层调用者（通常是PlayerInputState）
    // 用来把当前选中的SkillDataS0交出去，后续由上层组装成技能命令
    private Action<SkillDataSO> _onSkillSelected;
    // 物品按钮点下后，回传给上层调用者（通常是PlayerInputState）
    // 用来把当前选中的ItemDefinitionSo交出去，后续由上层组装成物品命令。
    private Action<ItemDefinitionSO> _onItemSelected;

    #endregion
    
    private Button _lastPrimaryButton;
    private bool _subMenuOpen;
    
    /* ---------------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        BindPrimaryButton();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!commandMenuCanvasGroup.gameObject.activeSelf) return;

        var input = InputSystemController.Instance;
        
        if (input != null && input.GetUICancelPressed() && _subMenuOpen)
            CloseSubMenu();
    }

    /* ---------------------------------------------------------------------------------- */
    
    private void BindPrimaryButton()
    {
        btnAttack.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Attack));
        btnSkill.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Skill));
        btnDefend.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Defend));
        btnEscape.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Escape));
        btnItem.onClick.AddListener(() => OnCommandClicked(BattleCommandType.Item));
    }
    
    private void OnCommandClicked(BattleCommandType commandType)
    {
        if (commandType == BattleCommandType.Skill)
        {
            OpenSkillMenu();
            return;
        }
        
        CloseAndInvoke(commandType);
    }
    
    /// <summary>
    /// 请求输入战命令
    /// </summary>
    /// <param name="entity">请求输入的战实体</param>
    /// <param name="onCommandSelected">当命令选择完成时的回调函数，接收BattleCommandType类型的参数</param>
    // public void RequestInput(BattleEntity entity, Action<BattleCommandType> onCommandSelected)
    // {
    //     _currentEntity = entity;
    //     _onCommandSelected = onCommandSelected;
    //     ShowPanel();
    // }

    private void CloseAndInvoke(BattleCommandType commandType)
    {
        ClosePanel();
        _onCommandSelected?.Invoke(commandType);
        _onCommandSelected = null; // 清空委托
    }
    
    public void ShowPanel()
    {
        commandMenuCanvasGroup.gameObject.SetActive(true);
        btnAttack.Select();
    }

    public void ClosePanel()
    {
        CloseSubMenu();
        commandMenuCanvasGroup.gameObject.SetActive(false);
    }

    #region 二级菜单

    private void OpenSkillMenu()
    {
        int currentSP = _currentEntity.CurrentSP;
        List<SkillDataSO> skills = _currentEntity.Definition.InitalSkills;

        if (skills.Count == 0)
            return;
        
        BeginSubMenu(btnSkill);
        Button firstButton = null;
        
        foreach (var skill in skills)
        {
            if (skill == null) continue;
            
            SkillButton skillButton = Instantiate(skillButtonPrefab, commandMenuPanel);
            skillButton.SetupSkillButton(skill);
            var button = skillButton.GetComponent<Button>();
            
            bool canCast = skill.spCost <= currentSP;
            button.interactable = canCast;
            ApplySkillButtonVisual(skillButton, canCast);
            
            button.onClick.AddListener(() => OnSkillButtonClick(skill));

            if (firstButton == null && canCast)
                firstButton = button;
            
            _spawnedSubMenuButtons.Add(skillButton.gameObject);
        }
        
        if (_spawnedSubMenuButtons.Count == 0)
        {
            CloseSubMenu();
            return;
        }
            
        firstButton.Select();
    }

    private void OnSkillButtonClick(SkillDataSO skill)
    {
        ClosePanel();
        _onSkillSelected?.Invoke(skill);
        _onSkillSelected = null;
    }

    private void BeginSubMenu(Button returnButton)
    {
        ClearSubmenuButtons();
        commandMenuPanel.gameObject.SetActive(true);
        
        _lastPrimaryButton = returnButton;  // 记录当前打开的二级菜单对应的主按钮，方便以后返回时重新选中
        _subMenuOpen = true;
        commandMenuCanvasGroup.interactable = false; // 打开二级菜单时，暂时禁止主命令按钮的交互
    }

    private void CloseSubMenu(bool restorePrimaryButton = true)
    {
        ClearSubmenuButtons();
        commandMenuPanel.gameObject.SetActive(false);
        commandMenuCanvasGroup.interactable = true;
        
        if (restorePrimaryButton && _subMenuOpen)
            _lastPrimaryButton?.Select();   // 返回主菜单时，重新选中之前的主按钮
        
        _lastPrimaryButton = null;
        _subMenuOpen = false;
    }

    private void ClearSubmenuButtons()
    {
        foreach (var button in _spawnedSubMenuButtons)
        {
            Destroy(button);
        }
        
        _spawnedSubMenuButtons.Clear();
    }

    public void RequestInput(BattleEntity entity, Action<BattleCommandType> onCommandSelected,
        Action<SkillDataSO> onSkillSelected,
        Action<ItemDefinitionSO> onItemSelected)
    {
        _currentEntity = entity;
        _onCommandSelected = onCommandSelected;
        _onSkillSelected = onSkillSelected;
        _onItemSelected = onItemSelected;
        ShowPanel();
    }

    private void ApplySkillButtonVisual(SkillButton skillButton, bool canCast)
    {
        float alpha = canCast ? 1f : 0.5f;
        foreach (Graphic graphic in skillButton.GetComponentsInChildren<Graphic>(true))
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
    
    #endregion
}
