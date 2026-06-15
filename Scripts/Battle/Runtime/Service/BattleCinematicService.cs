
using System;
using DG.Tweening;
using Framework.Event;

public class BattleCinematicService : MonoBehaviour,
    IEventReceiver<KillCinematicRequestedEvent>,
    IEventReceiver<BreakCinematicRequestedEvent>
{
    [Header("Scene References")] [SerializeField, Tooltip("直接拖拽BreakVolume上的后处理组件")]
    private BattleHitPostFX breakPostFx;

    [SerializeField, Tooltip("直接拖拽BattleCamera的空父节点，用于击杀和破盾时轻微转镜头")]
    private Transform battleCameraPivot;

    [Header("Cinematic Config")] [SerializeField, Tooltip("战斗演出参数S0。镜头和慢动作都集中在这里调。")]
    private BattleCinematicConfigSO cinematicConfig;

    public float killDissolveStagger => cinematicConfig.KittDissolveStagger;

    /* ---------------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<KillCinematicRequestedEvent>(this);
        EventBus.Subscribe<BreakCinematicRequestedEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<KillCinematicRequestedEvent>(this);
        EventBus.Unsubscribe<BreakCinematicRequestedEvent>(this);
    }

    /* ---------------------------------------------------------------------------------- */

    #region 事件接口

    public void OnEvent(KillCinematicRequestedEvent e)
    {
        StartCoroutine(PlayKillCinematic(e.Target));
    }

    public void OnEvent(BreakCinematicRequestedEvent e)
    {
        StartCoroutine(PlayBreakCinematic());
    }

    #endregion

    #region 击杀与破盾统一演出流程

    private IEnumerator PlayBreakCinematic()
    {
        breakPostFx.Play();

        if (!cinematicConfig.EnableBreakCinematic)
            yield break;

        yield return PlayImpactCinematic(cinematicConfig.Break);
    }

    private IEnumerator PlayKillCinematic(BattleEntity target)
    {
        target.Unit.PlayEnemyDissolve();
        if (!cinematicConfig.EnableKillCinematic)
        {
            yield break;
        }

        yield return PlayImpactCinematic(cinematicConfig.kill);
    }

    private IEnumerator PlayImpactCinematic(BattleImpactCinematicSetting settings)
    {
        // 1.先按配置把镜偏到位。
        Tween cameraTween = PlayCamera(settings);
        
        // 2.记录当前时间倍率，方便后面收回。
        float previousTimeScale = Time.timeScale;
        if (settings.HitStopDuration > 0f)
        {
            // 3.命中瞬间先做一次短暂停顿。
            Time.timeScale = 0;
            yield return new WaitForSecondsRealtime(settings.HitStopDuration);
        }

        // 4.接着进入慢动作。
        yield return PlayTimeScale(previousTimeScale, settings.SlowMoScale, settings.SLowMoInDuration);

        // 5.镜头序列已经把转向、停留和回位都串好了，直接等它播完就行。
        // WaitForCompletion：在协程里等到这条DoTween动画真正播完。
        yield return cameraTween.WaitForCompletion();

        // 6.最后把时间倍率收回去。
        yield return PlayTimeScale(settings.SlowMoScale, previousTimeScale, settings.SLowMoOutDuration);
    }

    #endregion

    #region 相机演出

    private Tween PlayCamera(BattleImpactCinematicSetting settings)
    {
        // 1.先记录镜头当前的基准位置和朝向。
        Vector3 basePos = battleCameraPivot.position;
        Quaternion baseRot = battleCameraPivot.rotation;

        // 2.再根据配置算出这次要移动到的位置和朝向。
        Vector3 toPos = basePos + (baseRot * settings.CameraPositionOffset);
        Quaternion toRot = baseRot * Quaternion.Euler(settings.CameraEuLerOffset);

        // 3.先停掉这个Transform身上l旧的补间动画，避免连续触发演出时互相抢位置。
        battleCameraPivot.DOKill();

        // SetUpdate(true）：无视Time.timeScale，用真实时间播放镜头动画。
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        
        AddCameraMove(sequence, toPos, toRot, settings.CameraTurnDuration);
        
        // 4.在镜头装到位后插入停留时间，再回到原位。
        if (settings.CameraHoldDuration > 0f)
            sequence.AppendInterval(settings.CameraHoldDuration);
        
        AddCameraMove(sequence, basePos, baseRot, settings.CameraTurnDuration);
        
        return sequence;
    }

    private void AddCameraMove(Sequence sequence, Vector3 toPos, Quaternion toRot, float duration)
    {
        if (duration <= 0f)
        {
            sequence.AppendCallback(() => battleCameraPivot.SetPositionAndRotation(toPos, toRot));
            return;
        }
        
        sequence.Append(battleCameraPivot.DOMove(toPos, duration).SetEase(Ease.Linear));
        
        // Join：让旋转和位移同时播放。
        sequence.Join(battleCameraPivot.DORotateQuaternion(toRot, duration).SetEase(Ease.Linear));
    }

#endregion
    
    #region 时间工具

    private static IEnumerator PlayTimeScale(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            Time.timeScale = to;
            yield break;
        }
        
        // 1.用真实时间累加，避免慢动作过程中Time.timeScale影响自己。
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            
            // 2.从起始倍率平滑过渡到目标倍率。
            Time.timeScale = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        
        // 3.最后一帧直接收口到目标值，避免浮点误差残留。
        Time.timeScale = to;
    }
    
    #endregion
}