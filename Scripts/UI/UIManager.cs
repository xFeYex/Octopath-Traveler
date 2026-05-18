using System;
using Framework.Event;


public class UIManager : MonoBehaviour,
    IEventReceiver<PanelRequestEvent>
{
    [Header("根节点与特殊面板引用")] 
    [SerializeField, Tooltip("探索模式下显示的总体 UI 根节点")]
    private GameObject _fieldUIRoot;
    
    public InquirePanelController inquirePanelController; // 注册到UI Manager中
    
    /* -------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<PanelRequestEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PanelRequestEvent>(this);
    }

    public void OnEvent(PanelRequestEvent e)
    {
        if (e.actionBase is InquireAction)
        {
            inquirePanelController.gameObject.SetActive(true);
            inquirePanelController.SetupPanel(e.actionBase);
        }
    }
}
