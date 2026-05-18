using Utils;

public class GameModeManager : Singleton<GameModeManager>
{
    public GameMode CurrentGameMode;
    [SerializeField] private GameMode defaultGameMode;

    protected override void Awake()
    {
        base.Awake();
        CurrentGameMode = defaultGameMode;
    }

    void Start()
    {
        AppleMode(CurrentGameMode);
    }

    /// <summary>
    /// 外部请求调用改模式
    /// </summary>
    /// <param name="newMode"></param>
    public void RequestChangeMode(GameMode newMode)
    {
        if (Instance != this) return;

        if (!CanSwitchMode(newMode)) return;
        
        AppleMode(newMode);
    }

    public bool CanSwitchMode(GameMode mode)
    {
        return CurrentGameMode != GameMode.Battle;
    }

    public void AppleMode(GameMode mode)
    {
        CurrentGameMode = mode;
        EventBus.Publish(new GameModeChangedEvent(CurrentGameMode));
    }
}