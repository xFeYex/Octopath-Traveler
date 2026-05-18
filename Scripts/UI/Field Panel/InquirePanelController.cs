using TMPro;
using UnityEngine.UI;

public class InquirePanelController : PanelController
{
    [Header("Inquire Panel")]
    // UI数据绑定
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private Image npcAvatar;
    [SerializeField] private TMP_Text messageTitleText;
    [SerializeField] private TMP_Text messageContentText;
    
    private InquireAction _currentAction;
    private int _currentIndex = -1;
    
    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    /* ------------------------------------------------------------------------- */

    public override void SetupPanel(ActionBase actionBase)
    {
        base.SetupPanel(actionBase);

        FirstButton = confirmButton;
        BindButtons();
        SetDefaultSelection();
        
        _currentAction = actionBase as InquireAction;
        
        ApplyMessage(_currentAction.PickRandomMessageIndex());
    }

    private void ApplyMessage(int messageIndex)
    {
        _currentAction.GetInquireActionData(messageIndex, out InquireActionData inquireActionData);
        _currentIndex = messageIndex;
        
        npcNameText.text = inquireActionData.personName;
        npcAvatar.sprite = inquireActionData.portraitOverride;
        messageTitleText.text = inquireActionData.title;
        messageContentText.text = inquireActionData.message;
    }
    
    private void BindButtons()
    {
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnCancel);
    }
}