using Framework.Event;
using Utils;

public class InputSystemController : Singleton<InputSystemController>, IEventReceiver<GameModeChangedEvent>
{
    private CharacterInputAction _inputAction;
    private bool _isInitialized;
    private ActionMap _currentMap; // 当前的输入总线map

    public CharacterInputAction InputAction;
    
    /* -------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        if (!_isInitialized)
        {
            _inputAction ??= new CharacterInputAction(); // 如果为空才构造
            _isInitialized = true;
        }
    }

    void OnEnable()
    {
        EventBus.Subscribe<GameModeChangedEvent>(this);
        _inputAction.Player.Enable();
    }

    void OnDisable()
    {
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
        _inputAction.Player.Disable();
    }

    void OnDestroy()
    {
        _inputAction.Disable();
    }

    public Vector2 GetMovementInput()
    {
        if(!_isInitialized || _currentMap != ActionMap.Player)
            return Vector2.zero;
        return _inputAction.Player.Move.ReadValue<Vector2>();
    }

    public bool GetPlayerConfirmPressed()
    {
        if(!_isInitialized) return false;
        
        if(_currentMap == ActionMap.Player)
            return _inputAction.Player.Confirm.WasPressedThisFrame();
        
        return false;
        
    }

    public bool GetUICancelPressed()
    {
        if(!_isInitialized) return false;
        
        if(_currentMap == ActionMap.UI)
            return _inputAction.UI.Cancel.WasPressedThisFrame();
        
        return false;
    }

    #region 事件实现

    // 订阅实现逻辑
    public void OnEvent(GameModeChangedEvent e)
    {
        _currentMap = GetActionMap(e.newMode);

        switch (_currentMap)
        {
            case ActionMap.Player:
                _inputAction.Player.Enable();
                _inputAction.UI.Disable();
                break;
            case ActionMap.UI:
                _inputAction.Player.Disable();
                _inputAction.UI.Enable();
                break;
            case ActionMap.None:
            default:
                break;
        }
    }

    private ActionMap GetActionMap(GameMode mode)
    {
        switch (mode)
        {
            case GameMode.Pause:
            case GameMode.InteractionMenu:
            case GameMode.Battle:
                return ActionMap.UI;
            case GameMode.Explore:
            default:
                return ActionMap.Player;
        }
    }

    #endregion
    
}
