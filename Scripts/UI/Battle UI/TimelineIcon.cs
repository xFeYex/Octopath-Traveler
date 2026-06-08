
using DG.Tweening;
using UnityEngine.UI;

public class TimelineIcon : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Sprite allyFrame;
    [SerializeField] private Sprite enemyFrame;
    
    // 缓存
    private CanvasGroup _canvasGroup;
    private Vector3 _visualInitPos;

    /* --------------------------------------------------------------------------------------- */
    
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _visualInitPos = visualRoot.localPosition; 
        
        _canvasGroup.alpha = 1f;
    }

    public void Setup(BattleEntity entity)
    {
        // _canvasGroup.alpha = 1f;
        portraitImage.sprite = entity.RuntimeData.Definition.Portrait;
        borderImage.sprite = entity.IsPlayer ? allyFrame : enemyFrame; 
    }

    #region 动画入口

    /// <summary>
    /// 播放入场动画：从偏移位置移动到初始位置，并淡入
    /// </summary>
    public void PlayEntranceAnimation(float duration, float offsetX, Ease ease)
    {
        StopVisualTween();
        
        _canvasGroup.alpha = 0f;
        visualRoot.localPosition = _visualInitPos + new Vector3(offsetX, 0f, 0f);  // 从偏移位置开始
        visualRoot.DOLocalMoveX(_visualInitPos.x, duration).SetEase(ease); // 从偏移位置移动到初始位置
        _canvasGroup.DOFade(1f, duration).SetEase(ease);                   // 淡入
    }

    /// <summary>
    /// 播放出场动画：从初始位置移动到偏移位置，并淡出
    /// </summary>
    public void PlayExitAnimation(float duration, float offsetX, Ease ease)
    {
        StopVisualTween();
        
        visualRoot.DOLocalMove(_visualInitPos + new Vector3(offsetX, 0f, 0f), duration).SetEase(ease); // 移动到偏移位置
        _canvasGroup.DOFade(0f, duration).SetEase(ease); // 淡出
    }

    private void StopVisualTween()
    {
        visualRoot.DOKill();
        _canvasGroup.DOKill();
    }

    #endregion
}
