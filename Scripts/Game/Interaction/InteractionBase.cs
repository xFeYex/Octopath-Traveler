using Unity.VisualScripting;

public class InteractionBase : MonoBehaviour
{
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
    }

    public void Interact(AllyDefinitionSO interactor)
    {
        PublishEvent(true);
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
            Debug.Log(_visibleEntries[i].CommandInfo.DisplayName);
        }
    }

    private void PublishEvent(bool inRange)
    {
        EventBus.Publish(new InteractionChangedEvent(this, inRange));
    }
}