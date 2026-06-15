
using Utils;

[CreateAssetMenu(menuName = "Battle/Damage Type Icon Set")]
public class DamageTypeIconSetSO : ScriptableObject
{
    [SerializeField] private DamageTypeIconEntry[] entries;
    
    private readonly Dictionary<DamageType, Sprite> _iconCache = new();
    
    public Sprite GetIcon(DamageType type) => _iconCache[type];

    private void OnValidate()
    {
        _iconCache.Clear();
        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            _iconCache[entry.damageType] = entry.icon;
        }
    }
}

[System.Serializable]
public struct DamageTypeIconEntry
{
    #region 图标条目结构
    
    public DamageType damageType;
    public Sprite icon;
    
    #endregion
}