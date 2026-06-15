[CreateAssetMenu(menuName = "Battle/Cinematic Config")]
public class BattleCinematicConfigSO : ScriptableObject
{
    [Header("Cinematic Toggle")]
    public bool EnableKillCinematic = true;
    public bool EnableBreakCinematic = true;
    public float KittDissolveStagger = 0.08f; // 多个敌人被击杀时，特效的错峰间隔

    #region 击败与破盾参数

    [Header("Kill Impact")]
    public BattleImpactCinematicSetting kill = BattleImpactCinematicSetting.CreateLegacyDefault();
    
    [Header("Break Impact")]
    public BattleImpactCinematicSetting Break = BattleImpactCinematicSetting.CreateLegacyDefault();

    #endregion
}

/// <summary>
/// 单次冲击演出的参数集合。
/// 这里只保留镜头和时间缩放，破盾闪屏由BreakVoLume自己管理。
/// </summary>
[System.Serializable]
public class BattleImpactCinematicSetting
{
    [Header("Time Scale")] 
    public float HitStopDuration = 0.5f; // 命中瞬间的停顿时长
    
    [Range(0.05f,1f)]
    public float SlowMoScale = 0.15f; // 慢动作时间缩放倍率0个引用
    
    public float SLowMoInDuration = 0.06f; //进入慢动作的过渡时长0个引用
    public float SLowMoOutDuration = 0.18f; //退出慢动作的过渡时长0个引用
    public float HoldDuration = 0.12f; //没有镜头特写时，慢动作的额外停留时间
    
    [Header("Camera")]
    public float CameraTurnDuration = 0.08f;// 镜头转向目标的时长0个引用
    public float CameraHoldDuration = 0.05f;//镜头转到位后的停留时长0个引用
    public float CameraReturnDuration = 0.12f;//镜头回到默认朝向的时长0个引用
    public Vector3 CameraEuLerOffset = Vector3.zero;//镜头旋转偏移0个引用
    public Vector3 CameraPositionOffset = Vector3.zero;// 镜头位置偏移

    public static BattleImpactCinematicSetting CreateLegacyDefault()
    {
        // 默认值直接沿用I旧BattleConfigSo的演出参数，方便无缝回到之前的视觉手感。
        return new BattleImpactCinematicSetting();
    }
}