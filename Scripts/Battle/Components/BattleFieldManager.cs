
using Utils;

public class BattleFieldManager : MonoBehaviour
{
    [Header("Prefab")] 
    [SerializeField] private BattleUnit battleUnitPrefab;
    private BattleFieldLayout layout;

    private Transform allyRoot;
    private Transform enemyRoot;
    private readonly Dictionary<BattleUnit, Vector3> _homePos = new();

    private readonly List<BattleUnit> _spawnedAllyUnits = new();
    private readonly List<BattleUnit> _spawnedEnemyUnits = new();
    public IReadOnlyList<BattleUnit> SpawnedAllyUnits => _spawnedAllyUnits;
    public IReadOnlyList<BattleUnit> SpawnEnemyUnits => _spawnedEnemyUnits;
    
    private EnemyLayoutFormation _currentFormation = EnemyLayoutFormation.Line;
    
    /* ----------------------------------------------------------------------------------------- */

    public void SpawnAll(BattleStartPayload payload)
    {
        layout = FindAnyObjectByType<BattleFieldLayout>();
        
        allyRoot = new GameObject("Ally Root").transform;
        allyRoot.SetParent(layout.transform, false);
        
        enemyRoot = new GameObject("Enemy Root").transform;
        enemyRoot.SetParent(layout.transform, false);

        _currentFormation = payload.Formation;

        // 要先清空已生成的
        ClearAllUnits();
        
        // 创建units
        _spawnedAllyUnits.AddRange(SpawnSide(payload.Allies.Count, true));
        _spawnedEnemyUnits.AddRange(SpawnSide(payload.Enemies.Count, false));
    }

    /// <summary>
    /// 按阵营生成一侧单位，并记录出生点/归位点，
    /// </summary>
    private List<BattleUnit> SpawnSide(int count, bool isAlly)
    {
        List<BattleUnit> units = new();
        for (int i = 0; i < count; i++)
        {
            // 1.计算最终战斗站位（Home Position）
            Vector3 targetSlotPos = isAlly 
                ? layout.GetAllySlotPos(i, count)
                : layout.GetEnemySlotPos(i, count, _currentFormation);
            
            // 2.决定出生点（SpawnPosition）
            Vector3 spawnPos = isAlly
                ? layout.initTrans.position
                : targetSlotPos;

            // 3. 生成单位
            BattleUnit unitObj = Instantiate(
                battleUnitPrefab, spawnPos, 
                Quaternion.identity, 
                isAlly ?  allyRoot : enemyRoot);
            
            // 4.记录缓存
            units.Add(unitObj);
            
            // [关键]这记录的是targetSlotPos（最终站位），不是spawnPos（出生点）。
            // 后续BattleSetupState会用这个Home 位置来执行"跑进场"动画
            _homePos[unitObj] = targetSlotPos;
        }
        return units;
    }

    private void ClearAllUnits()
    {
        foreach (var unit in _spawnedAllyUnits)
        {
            Destroy(unit.gameObject);
        }
        _spawnedAllyUnits.Clear();

        foreach (var unit in _spawnedEnemyUnits)
        {
            Destroy(unit.gameObject);
        }
        _spawnedEnemyUnits.Clear();
        _homePos.Clear();
    }
    
    public Vector3 GetHomePos(BattleUnit unit) => _homePos[unit];
    public Vector3 GetActionPos() => layout.actionTrans.position;
}
