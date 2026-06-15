
using System;
using Framework.Event;

public class BattleHUDController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent>,
    IEventReceiver<GameModeChangedEvent>,
    IEventReceiver<BattleResultViewEnterEvent>
{
    [Header("HUD Panels")] 
    [SerializeField] private GameObject ctbPanel;
    [SerializeField] private GameObject healthBarPanel;
    
    
    /* ---------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<BattleStartedEvent>(this);
        EventBus.Subscribe<GameModeChangedEvent>(this);
        EventBus.Subscribe<BattleResultViewEnterEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
        EventBus.Unsubscribe<BattleResultViewEnterEvent>(this);
    }

    /* ---------------------------------------------------------------------- */

    private void SetHudVisible(bool visible)
    {
        ctbPanel.SetActive(visible);
        healthBarPanel.SetActive(visible);
    }
    
    #region 事件监听
    
    public void OnEvent(BattleStartedEvent e)
    {
        SetHudVisible(true);
    }
    
    public void OnEvent(GameModeChangedEvent e)
    {
        SetHudVisible(false);
    }
    
    public void OnEvent(BattleResultViewEnterEvent e)
    {
        SetHudVisible(false);
    }
    
    #endregion


    
}