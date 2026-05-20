using System;
using TMPro;
using UnityEngine.UI;

public class RecruitPanelController : PanelController
{
    [Header("Recruit Panel")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image characterImage;
    
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    public override Type PanelActionType => typeof(RecruitAction);
    /* ---------------------------------------------------------------------- */

    public override void SetupPanel(ActionBase actionBase)
    {
        base.SetupPanel(actionBase);
        
        RecruitAction recruitAction = actionBase as RecruitAction;

        npcNameText.text = recruitAction.CurrentCharacter.Name;
        characterImage.sprite = recruitAction.CurrentCharacter.Portrait;
        levelText.text = recruitAction.CurrentCharacter.BaseLevel.ToString();

        BindButtons();
        SetDefaultSelection(); // 默认高亮按钮
    }
    
    private void BindButtons()
    {
        ReBindButtons(confirmButton, OnConfirm);
        ReBindButtons(cancelButton, OnCancel);
    }
}