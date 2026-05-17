public class InquireAction : ActionBase
{
    [Header("Inquire Message Config")]
    [SerializeField] private List<InquireActionData> inquireActionData = new();
}

[System.Serializable]
public class InquireActionData
{
    [Header("消息显示")]
    public string title;

    public string personName;

    public string message;
    
    public Sprite portraitOverride;
}