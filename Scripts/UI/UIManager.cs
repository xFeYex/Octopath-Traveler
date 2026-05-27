using System;
using Framework.Event;
using Unity.VisualScripting;
using Utils;


public class UIManager : MonoBehaviour,
    IEventReceiver<PanelRequestEvent>
{
    [Header("根节点与特殊面板引用")] 
    [SerializeField, Tooltip("探索模式下显示的总体 UI 根节点")] private GameObject _fieldUIRoot;
    [SerializeField]private GameObject mainPanel;

    // 一键缓存所有注册的面板
    private readonly Dictionary<Type, PanelController> _panelControllerDict = new(); // 存放面板 + 类型
    private readonly List<PanelController> _allPanelList = new(); // 存放面板
    
   
    
    /* -------------------------------------------------------------------------- */
    private void Awake()
    {
        _panelControllerDict.Clear();
        _allPanelList.Clear();
        
        GetPanelsFromRoot(transform);
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PanelRequestEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PanelRequestEvent>(this);
    }

    private void Update()
    {
        var mode = GameModeManager.Instance.CurrentGameMode;
        var input = InputSystemController.Instance;
        if (mode is GameMode.Battle) return;
        
        if (input.GetUICancelPressed())
            HandleGlobalCancelInput(mode);
        
        if (input.GetMenuPressed())
            HandleGlobalMenuInput(mode);
    }

    /* -------------------------------------------------------------------------- */

    private void HandleGlobalMenuInput(GameMode currentMode)
    {
        if (mainPanel.activeInHierarchy)
        {
            CloseAllPanels();
            mainPanel.SetActive(false);
            GameModeManager.Instance.RequestChangeMode(GameMode.Explore);
            return;
        }
        
        if (currentMode == GameMode.InteractionMenu) return;

        if (IsAnyPanelActive()) return;
        
        GameModeManager.Instance.RequestChangeMode(GameMode.Pause);
        mainPanel.SetActive(true);
    }

    // 首先尝试让当前活跃的面板处理取消输入，如果没有面板处理，则关闭所有面板
    private void HandleGlobalCancelInput(GameMode currentMode)
    {
        if (TryHandleCancelByActivePanel())
            return;
        
        if (!IsAnyPanelActive())
            return;
        
        GameModeManager.Instance.RequestChangeMode(GameMode.Explore);
        CloseAllPanels();
    }
    
    private void GetPanelsFromRoot(Transform root)
    {
        var panels = root.GetComponentsInChildren<PanelController>(true);
        foreach (var p in panels)
        {
            _allPanelList.Add(p);
            if(p.PanelActionType is null)
                continue;
            
            _panelControllerDict.Add(p.PanelActionType, p);
        }
    }

    private bool TryHandleCancelByActivePanel()
    {
        foreach (var panel in _allPanelList)
        {
            if (!panel.gameObject.activeSelf)
                continue;
            
            if(panel.HandleCancelInput())
                return true;
        }
        return false;
    }

    private bool IsAnyPanelActive()
    {

        foreach (var p in _allPanelList)
        {
            if(p.gameObject.activeSelf)
                return true;
        }
        return false;
    }

    private void CloseAllPanels()
    {
        foreach (var p in _allPanelList)
        {
            p.gameObject.SetActive(false);
        }
    }
    
    #region 事件函数
    
    public void OnEvent(PanelRequestEvent e)
    {
        var panelType = e.actionBase.GetType();

        _panelControllerDict.TryGetValue(panelType, out var panelController);
        
        panelController?.gameObject.SetActive(true);
        panelController?.SetupPanel(e.actionBase);
    }
    
    #endregion
}
