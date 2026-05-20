using System;
using Framework.Event;
using Utils;


public class UIManager : MonoBehaviour,
    IEventReceiver<PanelRequestEvent>
{
    [Header("根节点与特殊面板引用")] 
    [SerializeField, Tooltip("探索模式下显示的总体 UI 根节点")]
    private GameObject _fieldUIRoot;

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
        if (mode is GameMode.Battle) return;
        if (mode is GameMode.InteractionMenu)
        {
            if (IsAnyPanelActive() && InputSystemController.Instance.GetUICancelPressed())
            {
                TryHandleCancelByActivePanel();
                return;
            }
        }

        if (InputSystemController.Instance.GetUICancelPressed())
            CloseAllPanels();
    }

    /* -------------------------------------------------------------------------- */

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

    private void TryHandleCancelByActivePanel()
    {
        foreach (var p in _allPanelList)
        {
            if (p.gameObject.activeSelf)
            {
                p.gameObject.SetActive(false);
                return;
            }
        }
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
