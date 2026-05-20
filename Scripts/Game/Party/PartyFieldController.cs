
public class PartyFieldController : MonoBehaviour
{
    [Header("References")] // 参照
    [SerializeField] private Transform followerParent;  // 预制体生成的位置
    [SerializeField] private GameObject fieldFollowerPrefab; // 跟随的预制体（成员）
    [SerializeField] private Transform playerTrans; // 玩家 - 队长
    
    [Header("Settings")]
    [SerializeField] private float followDistance = 1.2f;   // 相联两人间距
    [SerializeField] private float followSpeed = 5f;

    [SerializeField] private float zOffset = 0.01f; // 跟随时的Z轴偏移，避免成员重叠
    [SerializeField] private float sampleMinDistance = 0.05f; // 采样点之间的最小距离，避免过于密集

    private List<Vector3> trail = new(); // 记录规矩
    private List<FieldFollower> feildFollowers = new(); // 跟随者列表
    
    /* --------------------------------------------------------------------------------------------- */

    void LateUpdate()
    {
        UpdateLeaderTrail();

        for (int i = 0; i < feildFollowers.Count; i++)
        {
            var follower = feildFollowers[i];
            float targetDistance = followDistance * (i + 1); // 跟随者与主角的目标距离
            Vector3 targetPos = GetPointAtDistance(targetDistance); // 获取目标位置
            follower.MoveTo(ApplyFollowerOffset(targetPos, i), followSpeed);
        }
    }
    
    /* --------------------------------------------------------------------------------------------- */

    private Vector3 GetPointAtDistance(float distanceFromLeader)
    {
        if (trail.Count == 0) return playerTrans.position;

        float accmulated = 0f; // 当前累计距离

        for (int i = 0; i < trail.Count -1 ; i++)
        {
            Vector3 a = trail[i];
            Vector3 b = trail[i + 1];
            
            float dist =  Vector3.Distance(a, b);

            if (accmulated + dist >= distanceFromLeader)
            {
                float t = (distanceFromLeader - accmulated) / dist;
                return Vector3.Lerp(a, b, t);
            }
            
            accmulated += dist;
        }
        
        return trail[trail.Count - 1];
        
        
    }
    
    public void UpdateFollowers(List<CharacterDefinitionSO> partyMembers)
    {
        // 更新跟随者数量
        int followerCount = partyMembers.Count - 1; // 减去主角
        while (feildFollowers.Count < followerCount)
        {
            int index = feildFollowers.Count;
            var pos = ApplyFollowerOffset(playerTrans.position, index);
            
            // 创建这个预制体，在这个坐标，没有旋转，在这个根物体下
            GameObject followerObj = Instantiate(fieldFollowerPrefab, pos, Quaternion.identity, followerParent);
            
            feildFollowers.Add(followerObj.GetComponent<FieldFollower>());
        }

        for (int i = 0; i < followerCount; i++)
        {
            feildFollowers[i].SetupFollower(partyMembers[i + 1]); // 跟随者对应队伍成员，跳过主角
        }
        
        RebuildTrailAndSnapFollowers();
    }
    
    private Vector3 ApplyFollowerOffset(Vector3 position, int index)
    {
        // 根据索引计算偏移，形成队列效果
        position.z += zOffset * (index + 1);
        return position;
    }

    private void UpdateLeaderTrail()
    {
        Vector3 leaderPos = playerTrans.position;

        if (trail.Count == 0)
        {
            trail.Add(leaderPos);
            return;
        }
        
        float dist = Vector3.Distance(leaderPos, trail[0]);
        if (dist > sampleMinDistance)
        {
            trail.Insert(0, leaderPos);
            
            if (trail.Count > 100)
                trail.RemoveAt(trail.Count - 1); // 限制轨迹长度，最多30个
        }
    }

    private void RebuildTrailAndSnapFollowers()
    {
        trail.Clear();
        
        for (int i = 0; i< feildFollowers.Count ; i++)
        {
            feildFollowers[i].SnapTo(ApplyFollowerOffset(playerTrans.position, i));
        }
        
        UpdateLeaderTrail();
    }
}
