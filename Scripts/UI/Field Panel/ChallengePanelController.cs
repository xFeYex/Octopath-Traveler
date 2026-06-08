
using System;
using TMPro;
using UnityEngine.UI;

public class ChallengePanelController : PanelController
{
    [Header("Challenge Panel")]
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private Image characterImage;
    
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    public override Type PanelActionType => typeof(ChallengeAction);
    
    /* ------------------------------------------------------------------------------ */

    public override void SetupPanel(ActionBase actionBase)
    {
        base.SetupPanel(actionBase);
        ChallengeAction challengeAction = actionBase as ChallengeAction;
        var currentCharacter = challengeAction.CurrentCharacter;
        
        npcNameText.text = currentCharacter.Name;
        difficultyText.text =  "难度" + challengeAction.lastDifficulty.ToString();
        characterImage.sprite = currentCharacter.Portrait;
        
        ReBindButtons(confirmButton, OnConfirm);
        ReBindButtons(cancelButton, OnCancel);
        FirstButton = confirmButton;
        
        SetDefaultSelection();
    }
}
