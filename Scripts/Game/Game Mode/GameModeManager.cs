using Utils;

public class GameModeManager : Singleton<GameModeManager>
{
    public GameMode currentGameMode;
    [SerializeField] private GameMode defaultGameMode;

    protected override void Awake()
    {
        base.Awake();
        currentGameMode = defaultGameMode;
    }

    void Start()
    {
        // 发布广播
        EventBus.Publish(new GameModeChangedEvent(currentGameMode));
    }
}