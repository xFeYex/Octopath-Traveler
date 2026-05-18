public class InquireAction : ActionBase
{
    [Header("Inquire Message Config")]
    [SerializeField] private List<InquireActionData> inquireActionDatas = new();
    
    public int PickRandomMessageIndex() => Random.Range(0, inquireActionDatas.Count);

    public void GetInquireActionData(int index, out InquireActionData inquireActionData) =>
        inquireActionData = inquireActionDatas[index];

    public override void TriggerAction(AllyDefinitionSO interaction)
    {
        EventBus.Publish(new PanelRequestEvent(this));
    }
}

[System.Serializable]
public class InquireActionData
{
    [Header("消息显示")]
    public string title;

    public string personName;
    
    [TextArea(2,6)] // 文字输入区域大小
    public string message;
    
    public Sprite portraitOverride;
}