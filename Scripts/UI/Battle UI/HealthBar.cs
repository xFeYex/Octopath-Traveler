
using System;
using Framework.Event;
using TMPro;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour,
    IEventReceiver<ActiveEntityChangedEvent>
{
    [Header("UI Elements")] 
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    
    // SP和BP显示
    [SerializeField] private Slider spSlider;
    [SerializeField] private TMP_Text spText;
    [SerializeField] private Slider bpSlider;
    
    [Header("Highlight")]
    [SerializeField] private RectTransform highlightRoot;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite activeBackground;
    [SerializeField] private float activeScale = 1.1f;
    
    // 缓存数据
    private BattleEntity _targetEntity;
    private Sprite _normalBackground;
    private Vector3 _baseScale = Vector3.one;
    private bool _isActive;
    
    /* ---------------------------------------------------------------------------------- */

    private void OnEnable()
    {
        EventBus.Subscribe<ActiveEntityChangedEvent>(this);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe<ActiveEntityChangedEvent>(this);
    }

    /* ---------------------------------------------------------------------------------- */

    public void Setup(BattleEntity entity)
    {
        _targetEntity = entity;
        characterName.text = entity.RuntimeData.Definition.Name;

        var stats = entity.RuntimeData.GetTotalStats();
        
        hpSlider.maxValue = stats.MaxHP;
        spSlider.maxValue = stats.MaxSP;
        bpSlider.maxValue = 5;
        
        // 刷新当前值
        RefreshUI();
    }

    private void RefreshUI()
    {
        var data = _targetEntity.RuntimeData;
        StatBlock stats = data.GetTotalStats();
        hpSlider.value = data.CurrentHP;
        hpText.text = $"{data.CurrentHP} / {stats.MaxHP}";
        
        spSlider.value = data.CurrentSP;
        spText.text = $"{data.CurrentSP} / {stats.MaxSP}";

        bpSlider.value = data.CurrentBP;
    }


    #region 事件监听

    public void OnEvent(ActiveEntityChangedEvent e)
    {
        if (_targetEntity == null)
        {
            SetActiveVisual(false);
            return;
        }
        
        SetActiveVisual(e.Entity == _targetEntity);
    }

    // 根据是否是当前行动者切换高亮显示
    private void SetActiveVisual(bool active)
    {
        if (_isActive == active)
            return;
        
        _isActive = active;
        backgroundImage.sprite = active ? activeBackground : _normalBackground;
        backgroundImage.SetNativeSize();
        highlightRoot.localScale = active ? _baseScale * activeScale : _baseScale;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform); // 强制刷新布局，确保背景图尺寸更新后UI元素位置正确
    }

    #endregion
    
}
