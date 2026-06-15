
using System;
using DG.Tweening;
using Framework.Event;
using TMPro;

public class BattleNotificationUI : MonoBehaviour,
    IEventReceiver<BattleNotificationEvent>,
    IEventReceiver<SkillNameDisplayEvent>
{
    #region 通知条配置

    [Header("UI 引用")] 
    [SerializeField] private GameObject notificationRoot;
    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private CanvasGroup notificationCanvasGroup;
    
    [Header("动画参数")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.15f;
    [SerializeField, Min(0f)] private float displayDuration = 1.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.3f;

    [Header("颜色配置")] 
    [SerializeField] private Color successColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private Color failureColor = new Color(0.7f, 0.7f, 0.7f);

    #endregion

    #region 技能名提示

    [Header("技能命提示")]
    [SerializeField] private GameObject skillNameRoot;
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private CanvasGroup skillNameCanvasGroup;
    
    [SerializeField, Min(0f)] private float skillNameFadeInDuration = 0.2f;
    [SerializeField, Min(0f)] private float skillNameDisplayDuration = 1.2f;
    [SerializeField, Min(0f)] private float skillNameFadeOutDuration = 0.18f;

    #endregion

    #region 运行时状态

    private Tween _notificationTween;
    private Tween _skillNameTween;

    #endregion
    
    /* -------------------------------------------------------------------------------------------------- */

    private void Awake()
    {
        notificationRoot.SetActive(false);
        skillNameRoot.SetActive(false);
        notificationCanvasGroup.alpha = 0f;
        skillNameCanvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<BattleNotificationEvent>(this);
        EventBus.Subscribe<SkillNameDisplayEvent>(this);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<BattleNotificationEvent>(this);
        EventBus.Unsubscribe<SkillNameDisplayEvent>(this);

        _notificationTween?.Kill();
        _skillNameTween?.Kill();

        notificationRoot.SetActive(false);
        skillNameRoot.SetActive(false);
        notificationCanvasGroup.alpha = 0f;
        skillNameCanvasGroup.alpha = 0f;
    }

    /* -------------------------------------------------------------------------------------------------- */

    #region 事件实现

    public void OnEvent(BattleNotificationEvent e)
    {
        notificationText.text = e.Message;
        notificationText.color = e.IsSuccess ? successColor : failureColor;
        notificationText.alpha = 1f;
        
        _notificationTween?.Kill();

        _notificationTween = PlayFadeSequence(
            notificationRoot,
            notificationCanvasGroup,
            fadeInDuration,
            fadeOutDuration,
            () =>
            {
                notificationRoot.SetActive(false);
                _notificationTween = null;
            }
        );
    }

    public void OnEvent(SkillNameDisplayEvent e)
    {
        skillNameText.text = e.SkillName;
        skillNameText.alpha = 1f;
        
        _skillNameTween?.Kill();

        _skillNameTween = PlayFadeSequence(
            skillNameRoot,
            skillNameCanvasGroup,
            skillNameFadeInDuration,
            skillNameFadeOutDuration,
            () =>
            {
                skillNameRoot.SetActive(false);
                _skillNameTween = null;
            }
        );
    }

    #endregion

    private Tween PlayFadeSequence(GameObject root, CanvasGroup targetCanvasGroup, float fadeInDuration,
        float fadeOutDuration, TweenCallback onFinished)
    {
        root.SetActive(true);
        targetCanvasGroup.alpha = 0;

        //DoTween.Sequence（）：创建一个时间轴容器，把后面的多个Tween串成一段完整演出。
        //SetUpdate(true）：让这段UI 动画不受Time.timeScale影响，暂停或慢动作时也能正常播放。
        //Append（...)：把一个Tween接到时间轴尾部，这里先接“从θ淡到1”的淡入动画。
        //AppendInterval（...)：在时间轴尾部插入一段纯等待时间，用来让文字在屏幕上停留一会。
        //Append（...)：继续把淡出动画接到等待后面，形成“淡入->停留->淡出”的完整顺序。
        //onComplete(...）：整条时间轴播完后执行收尾逻辑，比如隐藏物体、清空Tween引用。
        return DOTween.Sequence()
            .SetUpdate(true)
            .Append(targetCanvasGroup.DOFade(1f, fadeInDuration))
            .AppendInterval(displayDuration)
            .Append(targetCanvasGroup.DOFade(0f, fadeOutDuration))
            .OnComplete(onFinished);
    }
}