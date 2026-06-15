
using UnityEngine.AddressableAssets;
using Utils;
public class EncounterZone : MonoBehaviour
{
    [Header("BattleScene"), Tooltip("该区域对应的战场景（例如：森林战场景）")]
    public AssetReference battleSceneReference;

    [Header("EncounterSettings"), Tooltip("遇敌需要的最小移动距离")]
    public float minEncounterDistance = 15f;
    [Tooltip("遇敌需要的最大移动距离")]
    public float maxEncounterDistance = 30f;

    [Header("EnemyPools"), Tooltip("在这个区域可能遇到的敌兵组合池")]
    public List<EncounterGroup> encounterGroups = new();
    
    /* ---------------------------------------------------------------------------------- */

    /// <summary>
    /// 根据权重随机抽取一组敌人。
    /// 当前教程约定：每个EncounterZone都少配置组EncounterGroup
    /// </summary>
    public EncounterGroup GetRandomEncounter()
    {
        // 1.先把所有组的权重累加出来，作为随机区间总长度。
        int totalWeight = 0;
        
        // 这里直接在本函数里把权重加总掉，不用再跳一层.
        for (int i = 0; i < encounterGroups.Count; i++)
        {
            totalWeight += encounterGroups[i].Weight;
        }
        
        // 2.再生成一个落在总区间里的随机值。
        int randomValue = Random.Range(0, totalWeight);
        int currentWeight = 0;

        for (int i = 0; i < encounterGroups.Count; i++)
        {
            var grounp = encounterGroups[i];
            currentWeight += grounp.Weight;
            
            // 3.随机值落到哪一段，就返回哪组敌人。
            if (randomValue < currentWeight)
                return grounp;
        }

        return encounterGroups[^1];
    }
}

[System.Serializable]
 public struct EncounterGroup
 {
     #region 遇到组配置
 
     [Tooltip("敌方阵容组合")]
     public List<CharacterDefinitionSO> Enemies;
     
     [Tooltip("敌方阵型")]
     public EnemyLayoutFormation Formation;
 
     [Tooltip("出现权重（权重越高越容易遇到）"), Min(1)] 
     public int Weight;
 
     #endregion
 }