using System;
using UnityEngine.UI;
using Utils;

public class FadeController : Singleton<FadeController>
{
    // todo:应该是有两种淡入淡出的方式 但是mask擦除效果暂时无法完成
    // 默认使用的淡入淡出样式，可在 Inspector 中指定。
    [Header("Default Style")] 
    [SerializeField] private FadeStyle defaultStyle = FadeStyle.PanelFade;

    // 全屏遮罩图片，用于 PanelFade 样式下通过透明度实现黑场过渡。
    [Header("Fade Panel")] 
    [SerializeField] private Image panelImage;

    // 不同过渡样式对应的默认持续时间。
    [Header("Timing")] 
    [SerializeField, Range(0.05f, 3f)] private float panelFadeDuration = 0.35f;
    [SerializeField, Range(0.05f, 3f)] private float wipeFadeDuration = 0.6f;

    // 当前实际执行的过渡样式，以及正在运行的淡入淡出协程。
    private FadeStyle _currentStyle;
    private Coroutine _fadeRoutine;
    
    // 下一次淡出/淡入可临时覆盖默认时长；使用后会被重置为 -1。
    private float _nextFadeOutDurationOverride = -1f;
    private float _nextFadeInDurationOverride = -1f;
    
    /* ----------------------------------------------------------------------------------------- */

    protected override void Awake()
    {
        base.Awake();
        // 初始化时隐藏遮罩，并确保透明度从完全透明开始。
        panelImage.enabled = false;
        SetPanelAlpha(0);
    }
    
    /* ----------------------------------------------------------------------------------------- */

    public void SetStyle(FadeStyle style)
    {
        // todo: 先默为 default 了, 因为没 mask
        _currentStyle = defaultStyle;
        //_currentStyle = style;
    }
    
    public void SetNextFadeOutDuration(float fadeInDuration, float fadeOutDuration)
    {
        _nextFadeOutDurationOverride = fadeOutDuration > 0f ? fadeOutDuration : -1f;
        _nextFadeInDurationOverride = fadeInDuration > 0f ? fadeInDuration : -1f;
    }
    
    public void FadeOut(Action onDone = null)
    {
        // 对外触发淡出时使用，结束后执行 onDone 回调。
        StartFade(1f,ResolveDuration(true) , onDone);
    }

    public void FadeIn(Action onDone = null)
    {
        // 对外触发淡入时使用，结束后执行 onDone 回调。
        StartFade(0, ResolveDuration(false), onDone);
    }

    private void StartFade(float target, float duration, Action onDone)
    {
        // 同一时间只允许一个淡入淡出协程运行，新的过渡会打断旧的过渡。
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }
        _fadeRoutine = StartCoroutine(FadeRoutine(target, duration, onDone));
    }

    private IEnumerator FadeRoutine(float target, float duration, Action onDone)
    {
        // PanelFade 才需要启用遮罩图；其它样式可在扩展逻辑中处理自己的表现。
        panelImage.enabled = _currentStyle == FadeStyle.PanelFade;
        
        float start = panelImage.color.a; // todo: wipe 模式的初始值
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 根据已经经过的时间，在当前透明度和目标透明度之间平滑插值。
            float alpha = Mathf.Lerp(start, target, elapsed / duration);
            SetPanelAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 强制落到目标值，避免最后一帧因为 deltaTime 误差残留细微透明度偏差。
        SetPanelAlpha(target);
        onDone?.Invoke();
        _fadeRoutine = null;
    }
    
    private void SetPanelAlpha(float alpha)
    {
        // Unity 的 Color 是值类型，修改透明度后需要整体写回 Image。
        Color color = panelImage.color;
        color.a = alpha;
        panelImage.color = color;
    }

    private float ResolveDuration(bool isFadeOut)
    {
        // 淡出时优先消费一次性的时长覆盖值。
        if (isFadeOut && _nextFadeOutDurationOverride >= 0f)
        {
            float value = _nextFadeOutDurationOverride;
            _nextFadeOutDurationOverride = -1f;
            return value;
        }
        
        // 未设置覆盖值时，根据当前样式返回对应的默认时长。
        return _currentStyle == FadeStyle.PanelFade ? panelFadeDuration : wipeFadeDuration;
    }
}
