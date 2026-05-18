
public class InteractionBase : MonoBehaviour
{
    [Header("Sign Trans")]
    public Transform HeadAnchor; // 描点
    
    private AllyDefinitionSO _currentInteraction; // 当前交互的是谁
    
    private ActionBase[] _actionsCache; // all当前NPC身上有的交互的命令缓存

    private readonly List<ActionCommandInfo> _cacheCommandInfo = new(8); // 命令的详细信息
    
    private readonly List<VisibleActionEntry> _visibleEntries = new(8); // 可以显示出来的命令信息
    
    public IReadOnlyList<ActionCommandInfo> CacheCommandInfo => _cacheCommandInfo; // all对外显示的数据
    
    private struct VisibleActionEntry
    {
        public ActionBase Action;
        public ActionCommandInfo CommandInfo;
    }
    /* --------------------------------------------------- */

    private void Awake()
    {
        CacheActions();
        HeadAnchor = transform.GetChild(0);
    }

    public void Interact(AllyDefinitionSO interactor)
    {
        EventBus.Publish(new InteractionMenuRequestEvent(this));
    }

    public void OnFocus(AllyDefinitionSO interactor)
    {
        CacheActions();
        _currentInteraction = interactor;
        
        RebuildCommands();
        PublishEvent(true);
    }

    public void OnLoseFocus(AllyDefinitionSO interactor)
    {
        _currentInteraction = null;
        _cacheCommandInfo.Clear();
        
        PublishEvent(false);
        HeadAnchor.gameObject.SetActive(true);
    }
    
    private void CacheActions() => _actionsCache = GetComponents<ActionBase>();

    private void RebuildCommands()
    {
        _cacheCommandInfo.Clear();
        _visibleEntries.Clear();
        for (int i = 0; i < _actionsCache.Length; i++)
        {
            var action = _actionsCache[i];
            
            // TODO: 之后改成part member检测（团队检测）
            if(!action.CanShow(_currentInteraction))
                continue;

            _visibleEntries.Add(new VisibleActionEntry
            {
                Action = action,
                CommandInfo = action.CommandInfo
            });
        }
        
        if(_visibleEntries.Count > 1)
            _visibleEntries.Sort((a,b) => a.CommandInfo.Order.CompareTo(b.CommandInfo.Order));

        // 缓存信息
        for (int i = 0; i < _visibleEntries.Count; i++)
        {
            _cacheCommandInfo.Add(_visibleEntries[i].CommandInfo);
        }

        if (_visibleEntries.Count > 0)
        {
            HeadAnchor.gameObject.SetActive(false);
        }
    }

    private void PublishEvent(bool inRange)
    {
        EventBus.Publish(new InteractionChangedEvent(this, inRange));
    }
    
    #region UI回调
    
    /// <summary>
    /// UI调用
    /// </summary>
    /// <param name="commandIndex"></param>
    /// <returns></returns>
    public bool ExecuteCommandFromUI(int commandIndex)
    {
        if(commandIndex >= _visibleEntries.Count) return false;
        
        var action = _visibleEntries[commandIndex].Action;
        if (!action.CanExecute(_currentInteraction)) return false;
        
        action.TriggerAction(_currentInteraction);
        return true;
    }

    #endregion
}