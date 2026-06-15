
using Framework.Event;
using Utils;

public class WeaknessController : MonoBehaviour,
    IEventReceiver<BattleStartedEvent>,
    IEventReceiver<BattleEndedEvent>
{
    [Header("Prefab")]
    [SerializeField] private WeaknessBar weaknessBarPrefab;
    [SerializeField] private DamageTypeIconSetSO  damageTypeIconSet;
    
    [Header("Follow")]
    [SerializeField] private RectTransform containerRoot;
    [SerializeField] private Vector2 screenOffset = Vector2.zero;

    private readonly Dictionary<BattleEntity, WeaknessBar> _barByEntity = new();

    /* ----------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<BattleStartedEvent>(this);
        EventBus.Subscribe<BattleEndedEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BattleStartedEvent>(this);
        EventBus.Unsubscribe<BattleEndedEvent>(this);
        
        ClearBars();
    }

    private void LateUpdate()
    {
        if (_barByEntity.Count == 0) return;
        if (GameModeManager.Instance.CurrentGameMode != GameMode.Battle) return;
        
        foreach (var kv in _barByEntity)
        {
            BattleEntity entity = kv.Key;
            WeaknessBar bar = kv.Value;

            if (!entity.IsAlive)
            {
                bar.SetVisible(false);
                continue;
            }
            
            var screenPos = Camera.main.WorldToScreenPoint(entity.Unit.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRoot, screenPos, null, out Vector2 localPos);
            
            bar.SetVisible(true);
            bar.SetScreenPosition(localPos + screenOffset);
        }
    }

    /* ----------------------------------------------------------------------------- */

    
    
    private void RebuildBars()
    {
        ClearBars();
        BattleUnit[] allUnits = FindObjectsOfType<BattleUnit>(true);
        for (int i = 0; i < allUnits.Length; i++)
        {
            BattleUnit unit = allUnits[i];
            if (unit.Entity.IsPlayer)
                continue;

            WeaknessBar bar = Instantiate(weaknessBarPrefab, containerRoot);
            bar.Setup(unit.Entity, damageTypeIconSet);
            
            _barByEntity[unit.Entity] = bar;
        }
    }
    
    private void ClearBars()
    {
        foreach (var bar in _barByEntity.Values)
        {
            Destroy(bar.gameObject);
        }
        _barByEntity.Clear();
    }

    #region 事件

    public void OnEvent(BattleStartedEvent e)
    {
        RebuildBars();
    }

    public void OnEvent(BattleEndedEvent e)
    {
        ClearBars();
    }

    #endregion
}