using System;
using Framework.Event;
using UnityEngine.Pool;
using UnityEngine.UI;

public class InteractionUIController : MonoBehaviour,
    IEventReceiver<InteractionChangedEvent>
{
    [Header("Head Icon")] 
    [SerializeField] private RectTransform actionIconHolder; // 交互ui的坐标
    [SerializeField] private GameObject actionIconPrefab; // 具体ui的预制体
    
    private ObjectPool<GameObject> _iconPool;   // icon图标的对象池
    private readonly List<GameObject> _activeIcons = new(8);

    private IReadOnlyList<ActionCommandInfo> _currentCommandList;

    /* ----------------------------------------------------------------- */
    
    private void Awake()
    {
        InitPool();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<InteractionChangedEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<InteractionChangedEvent>(this);
    }

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
    }

    #region 对象池方法

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

    #endregion

    #region 事件相关方法

    public void OnEvent(InteractionChangedEvent e)
    {
        // 启动显示头顶icon
        _currentCommandList = e.target.CacheCommandInfo; 
        
        ShowHeadIcons();
    }

    #endregion

    private void ShowHeadIcons()
    {
        actionIconHolder.gameObject.SetActive(true);
        
        SyncPool(_activeIcons, _iconPool, _currentCommandList.Count);

        for (int i = 0; i < _activeIcons.Count; i++)
        {
            var obj = _activeIcons[i];
            var cmd = _currentCommandList[i];
            
            obj.GetComponent<Image>().sprite = cmd.Icon;
        }
    }
}