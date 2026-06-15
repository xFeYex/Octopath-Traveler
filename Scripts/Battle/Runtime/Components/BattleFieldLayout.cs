
using Utils;

public class BattleFieldLayout : MonoBehaviour
{
    [Header("Scene References")]
    // 轮到行动者时移动到这里（红色ActionPosition）.
    public Transform actionTrans;
    public Transform initTrans;

    [Header("Ally Slot Segment")] 
    [SerializeField] private Transform allyTopTrans;
    [SerializeField] private Transform allyBottomTrans;

    [Header("Enemy Slot Segment (Normal)")]
    [SerializeField] private Transform enemyTopTrans;
    [SerializeField] private Transform enemyButtonTrans;
    
    [Header("Enemy Slot Segment (BossTriangle)")]
    [SerializeField] private Transform bossCenterTrans;
    [SerializeField] private Transform minionTopTrans;
    [SerializeField] private Transform minionButtonTrans;

    public Vector3 GetAllySlotPos(int index, int count)
    {
        return LerpByMidpointRule(allyTopTrans.position, allyBottomTrans.position, index, count);
    }

    public Vector3 GetEnemySlotPos(int index, int count, EnemyLayoutFormation formation)
    {
        switch (formation)
        {
            case EnemyLayoutFormation.Line:
                return LerpByMidpointRule(enemyTopTrans.position, enemyButtonTrans.position, index, count);
            case EnemyLayoutFormation.BossTriangle:
                return GetEnemyBossTrianglePos(index, count);
            default:
                return Vector3.zero;
        }
    }

    #region 线段等分算法

    private Vector3 LerpByMidpointRule(Vector3 start, Vector3 end, int index, int count)
    {
        float t = (index + 0.5f) / count;
        return Vector3.Lerp(start, end, t);
    }

    private Vector3 GetEnemyBossTrianglePos(int index, int count)
    {
        if (index == 0)
            return bossCenterTrans.position;
        return LerpByMidpointRule(minionTopTrans.position, minionButtonTrans.position, index, count);
    }

    #endregion

    #region 站位中心点

    public Vector3 GetAllyGroupCenter()
    {
        return Vector3.Lerp(allyTopTrans.position, allyBottomTrans.position, 0.5f);
    }

    public Vector3 GetEnemyGroupCenter(EnemyLayoutFormation formation)
    {
        if (formation == EnemyLayoutFormation.BossTriangle)
            return bossCenterTrans.position;
        
        return Vector3.Lerp(enemyTopTrans.position, enemyButtonTrans.position, 0.5f);
    }

    #endregion
}