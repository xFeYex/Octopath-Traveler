
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine.Pool;
using Utils;
using Random = UnityEngine.Random;

public class DamagePopup : MonoBehaviour
{
    [Header("Components")] 
    private TMP_Text textMesh;

    [Header("Motion Settings (位移设置)")] 
    [SerializeField] private float initialVelocityY = 10f;
    [SerializeField] private Vector2 randomHorizontalRange = new Vector2(-3f, -3f);
    [SerializeField] private float drag = 5f;
    [SerializeField] private float gravity = 0f;
    
    [Header("Scale Settings (缩放弹跳)")]
    [SerializeField] private Ease scaleCurve = Ease.OutBack;
    [SerializeField] private float scaleDuration = 0.3f;
    
    [Header("Fade Settings (淡出设置)")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeOutStartTime = 0.6f;
    
    private Vector3 _velocity;
    private float _timer;
    private Color _baseColor;
    private Vector3 _baseScale;
    
    private ObjectPool<DamagePopup> _pool;
    public void SetPool(ObjectPool<DamagePopup> pool) => _pool = pool;

    /* --------------------------------------------------------------------------------------------- */

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        _baseColor = textMesh.color;
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        float dt = Time.timeScale <= 0 ? Time.unscaledDeltaTime : Time.deltaTime;
        _timer += dt;

        UpdateScale();
        UpdateMotion(dt);
        UpdateFade();
        
        if (_timer > lifetime)
            Release();
    }
   
    /* --------------------------------------------------------------------------------------------- */
    
    private void UpdateScale()
    {
        if (scaleDuration <= 0 || _timer >= scaleDuration)
        {
            transform.localScale = _baseScale;
            return;
        }
        
        float t = Mathf.Clamp01(_timer / scaleDuration);
        float scaleValue = DOVirtual.EasedValue(0f, 1f, t, scaleCurve);
        transform.localScale = _baseScale * scaleValue;
    }

    private void UpdateMotion(float dt)
    {
        transform.position += _velocity * dt;
        _velocity -= _velocity * dt * drag;

        if (gravity != 0)
        {
            _velocity += new Vector3(0, -gravity, 0) * dt;
        }
    }

    private void UpdateFade()
    {
        if (_timer < fadeOutStartTime || lifetime <= fadeOutStartTime)
            return;
        
        float t = Mathf.Clamp01(_timer / fadeOutStartTime) / (lifetime - fadeOutStartTime);
        textMesh.alpha = Mathf.Lerp(1f, 0f, t);
    }

    private void Release()
    {
        _pool.Release(this);
    }

    public void Setup(int amount, DamagePopupType popupType)
    {
        textMesh.text = ConvertNumberToSpriteString(amount, popupType);
        _timer = 0f;
        textMesh.alpha = 1f;
        transform.localScale = _baseScale * 0f;
        
        _velocity = new Vector3(Random.Range(randomHorizontalRange.x, randomHorizontalRange.y), initialVelocityY, 0f);
    }
    
    private string ConvertNumberToSpriteString(int value, DamagePopupType popupType)
    {
        string original = Mathf.Abs(value).ToString();
        StringBuilder builder = new StringBuilder();
        int startIndex = (int)popupType * 10;

        foreach (char c in original)
        {
            int digit = c - '0';
            builder.Append($"<sprite={startIndex + digit}>");
        }
        
        return builder.ToString();
    }
}
