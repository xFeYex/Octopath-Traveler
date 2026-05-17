using Unity.VisualScripting;

public class InteractionBase : MonoBehaviour
{
    private AllyDefinitionSO _currentInteraction;
    
    private ActionBase[] _actionsCache;

    private readonly List<ActionCommandInfo> _cacheCommandInfo = new(8);
    
    private readonly List<VisibleActionEntry> _visibleEntries = new(8);
    
    public IReadOnlyList<ActionCommandInfo> CacheCommandInfo => _cacheCommandInfo;

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
    
    public void Interact(AllyDefinitionSO interactor) { }

    public void OnFocus(AllyDefinitionSO interactor)
    {
        CacheActions();
        _currentInteraction = interactor;
        
        RebuildCommands();
    }

    public void OnLoseFocus(AllyDefinitionSO interactor)
    {
        _currentInteraction = null;
        _cacheCommandInfo.Clear();
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

        for (int i = 0; i < _visibleEntries.Count; i++)
        {
            _cacheCommandInfo.Add(_visibleEntries[i].CommandInfo);
            Debug.Log(_visibleEntries[i].CommandInfo.DisplayName);
        }
    }
}