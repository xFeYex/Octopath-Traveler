using System;
using Framework.Event;
using UnityEngine.Pool;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Utils;

public class InteractionUIController : MonoBehaviour,
    IEventReceiver<InteractionChangedEvent>,
    IEventReceiver<InteractionMenuRequestEvent>,
    IEventReceiver<GameModeChangedEvent>
{
    // 绑定NPC可交互icon + 定位
    [Header("Head Icon")] 
    [SerializeField] private RectTransform actionIconHolder; // 交互ui的坐标
    [SerializeField] private GameObject actionIconPrefab; // 具体ui的预制体

    // 绑定二级菜单按钮 + 定位
    [Header("Menu Button")]
    [SerializeField]private RectTransform actionMenuHolder;
    [SerializeField] private GameObject actionMenuBottonPrefab;

    private ObjectPool<GameObject> _iconPool; // icon图标的对象池
    private ObjectPool<GameObject> _menuButtonPool; // 按钮信息的对象池

    private readonly List<GameObject> _activeIcons = new(8);
    private readonly List<GameObject> _activeButtons = new(8);

    private IReadOnlyList<ActionCommandInfo> _currentCommandList;

    private Transform _headAnchor; // 显示的锚点

    /* ----------------------------------------------------------------- */

    #region 周期函数

    private void Awake()
    {
        InitPool();
        actionIconHolder.gameObject.SetActive(false); // 只有在碰到npc的时候才显示
        actionMenuHolder.gameObject.SetActive(false); // 默认是不打开二级菜单的
    }

    private void OnEnable()
    {
        EventBus.Subscribe<InteractionChangedEvent>(this);
        EventBus.Subscribe<InteractionMenuRequestEvent>(this);
        EventBus.Subscribe<GameModeChangedEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractionChangedEvent>(this);
        EventBus.Unsubscribe<InteractionMenuRequestEvent>(this);
        EventBus.Unsubscribe<GameModeChangedEvent>(this);
    }

    private void Update()
    {
        if(GameModeManager.Instance.CurrentGameMode != GameMode.InteractionMenu)
            return;
        
        var input = InputSystemController.Instance;
        if (input.GetUICancelPressed())
        {
            CloseMenu(true);
            GameModeManager.Instance.RequestChangeMode(GameMode.Explore);
        }
    }

    private void LateUpdate()
    {
        if (!actionIconHolder.gameObject.activeSelf || _headAnchor == null) return;
        UpdateHeadIconPosition();
    }

    #endregion

    /* ----------------------------------------------------------------- */

    private void InitPool()
    {
        _iconPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(actionIconPrefab, actionIconHolder),
            actionOnGet: ((obj) =>
            {
                obj.SetActive(true);
                obj.transform.SetAsLastSibling();
            }),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 8,
            maxSize: 20
        );

        _menuButtonPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(actionMenuBottonPrefab, actionMenuHolder),
            actionOnGet: ((obj) =>
            {
                obj.SetActive(true);
                obj.transform.SetAsLastSibling();
            }),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => Destroy(obj),
            defaultCapacity: 8,
            maxSize: 20
        );
    }


    // 更新显示的坐标
    private void UpdateHeadIconPosition()
    {
        var worldPos = _headAnchor.position;
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, screenPos, null,
                out var localPoint))
        {
            actionIconHolder.anchoredPosition = localPoint;
        }
    }

    #region 对象池方法

    // 同步加载对象信息
    private void SyncPool(List<GameObject> activeList, ObjectPool<GameObject> pool, int targetCount)
    {
        while (activeList.Count > targetCount)
        {
            int lastIndex = activeList.Count - 1;
            GameObject item = activeList[lastIndex];
            pool.Release(item);
            activeList.RemoveAt(lastIndex);
        }

        while (activeList.Count < targetCount)
        {
            GameObject item = pool.Get();
            activeList.Add(item);
        }
    }

    private void ReleaseAll(List<GameObject> activeList, ObjectPool<GameObject> pool)
    {
        for (int i = 0; i < activeList.Count; i++)
        {
            pool.Release(activeList[i]);
        }

        activeList.Clear();
    }

    #endregion

    #region 事件相关方法

    // 碰撞显示npn头顶icon
    public void OnEvent(InteractionChangedEvent e)
    {
        if (!e.inRange || e.target is null)
        {
            // 关闭头顶icon
            HideHeadIcons();
            return;
        }

        // 启动显示头顶icon
        _currentCommandList = e.target.CacheCommandInfo;
        _headAnchor = e.target.HeadAnchor;

        ShowHeadIcons();
    }

    // 按E激活二级菜单
    public void OnEvent(InteractionMenuRequestEvent e)
    {
        // 关闭头顶icon
        actionIconHolder.gameObject.SetActive(false);
        ReleaseAll(_activeIcons, _iconPool);

        actionMenuHolder.gameObject.SetActive(true);

        OpenMenu(e.target);
    }

    // 模式改变事件
    public void OnEvent(GameModeChangedEvent e)
    {
        if(e.newMode == GameMode.InteractionMenu)
            return;
        
        if (e.newMode == GameMode.Explore)
        {
            if(_currentCommandList is not null && _currentCommandList.Count > 0)
                ShowHeadIcons();
        }
    }
    
    #endregion

    private void OpenMenu(InteractionBase target)
    {
        // 通知gameMode 切换游戏模式
        GameModeManager.Instance.RequestChangeMode(GameMode.InteractionMenu);
        
        SyncPool(_activeButtons, _menuButtonPool, _currentCommandList.Count);

        Button firstBtn = null;
        for (int i = 0; i < _activeButtons.Count; i++)
        {
            var btn = _activeButtons[i];
            var cmd = _currentCommandList[i];

            int commandIndex = i;
            btn.GetComponent<ActionMenuBotton>().SetButton(cmd, () =>
            {
                target.ExecuteCommandFromUI(commandIndex);
                CloseMenu(false);
            });
            
            if(firstBtn is null)
                firstBtn = btn.GetComponent<Button>();
        }

        if (firstBtn is not null)
        {
            firstBtn.Select();
            EventSystem.current.SetSelectedGameObject(firstBtn.gameObject);
        }
    }

    private void ShowHeadIcons()
    {
        if (_currentCommandList.Count == 0) return;

        actionIconHolder.gameObject.SetActive(true);

        SyncPool(_activeIcons, _iconPool, _currentCommandList.Count);

        for (int i = 0; i < _activeIcons.Count; i++)
        {
            var obj = _activeIcons[i];
            var cmd = _currentCommandList[i];

            obj.GetComponent<Image>().sprite = cmd.Icon;
        }
    }

    private void CloseMenu(bool restoreHeadIcons)
    {
        HideActionMenu();
        if (restoreHeadIcons)
            ShowHeadIcons();
        else 
            HideHeadIcons();
    }
    
    private void HideHeadIcons()
    {
        actionIconHolder?.gameObject.SetActive(false);
        ReleaseAll(_activeIcons, _iconPool);
    }
    
    private void HideActionMenu()
    {
        actionMenuHolder?.gameObject.SetActive(false);
        ReleaseAll(_activeButtons, _menuButtonPool);
    }
}