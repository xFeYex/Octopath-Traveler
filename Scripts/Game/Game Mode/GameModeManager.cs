using Utils;

public class GameModeManager : Singleton<GameModeManager>
{
    public GameMode CurrentGameMode;
    [SerializeField] private GameMode defaultGameMode;
    
    /* ---------------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        CurrentGameMode = defaultGameMode;
        Application.targetFrameRate = 60; // 锁帧
    }

    void Start()
    {
        ApplyMode(CurrentGameMode);
    }
    
    /* ---------------------------------------------------------------------------------- */

    /// <summary>
    /// 外部请求调用改模式
    /// </summary>
    /// <param name="newMode"></param>
    public void RequestChangeMode(GameMode newMode)
    {
        if (Instance != this) 
            return;

        if (!CanSwitchMode(newMode)) 
            return;
        
        ApplyMode(newMode);
    }

    /// <summary>
    /// 模式切换闸门.
    /// 关键规则：当战会话仍在运行时，禁Battle-Explore 直切.
    /// 正常离场路径应通过SceneLoadManager：
    /// Battle-> InteractionMenu-> 场景切换-> Explore.
    /// </summary>
    public bool CanSwitchMode(GameMode newMode)
    {
        if (CurrentGameMode != GameMode.Battle || newMode != GameMode.Explore)
            return true;
        
        return false;
    }

    public void ApplyMode(GameMode mode)
    {
        CurrentGameMode = mode;
        EventBus.Publish(new GameModeChangedEvent(CurrentGameMode));
    }
}