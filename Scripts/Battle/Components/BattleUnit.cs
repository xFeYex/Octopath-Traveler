

public class BattleUnit : MonoBehaviour
{
    public BattleEntity Entity {get; private set;}
    [Header("Base Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("目标选择光标")]
    [SerializeField] private Transform hitPoint;
    [SerializeField] private SpriteRenderer targetCursorRenderer;
    
    /* ----------------------------------------------------------------------------------------- */
    private void Awake()
    {
        targetCursorRenderer.enabled = false;
        UpdateTargetCursorPosition();
    }

    private void UpdateTargetCursorPosition()
    {
        // Vector3 worldCenter = spriteRenderer.bounds.center;
        // Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        // Vector3 localPos = targetCursorRenderer.transform.localPosition;
        // localPos.y = localCenter.y;
        // targetCursorRenderer.transform.localPosition = localPos;
        // hitPoint.localPosition = localPos;
        
        Bounds bounds = spriteRenderer.bounds;
        Vector3 worldLeftCenter = new Vector3(bounds.min.x, bounds.center.y, bounds.center.z);
        Vector3 localLeftCenter = transform.InverseTransformPoint(worldLeftCenter);
        
        Transform cursorTransform = targetCursorRenderer.transform;
        Vector3 localPos = cursorTransform.localPosition;
        localPos.x = localLeftCenter.x;
        localPos.y = localLeftCenter.y;
        localPos.z = localLeftCenter.z - 0.1f;
        cursorTransform.localPosition = localPos;
    }

    public void SetTargetSelection(bool visible)
    {
        if (visible)
            UpdateTargetCursorPosition();
        
        targetCursorRenderer.enabled = visible;
    }

    public void Bind(BattleEntity entity)
    {
        Entity = entity;
        
        if (entity.Definition.battleAnimator != null)
            animator.runtimeAnimatorController = entity.Definition.battleAnimator;
        
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (!Entity.IsAlive)
        {
            animator.SetBool("isDead", true);
            return;
        }
        
        animator.SetBool("isDead", false);
        float maxHP = Entity.TotalStats.MaxHP;
        float hpRatio = Mathf.Clamp01(Entity.CurrentHP / maxHP);
        animator.SetFloat("hp01", hpRatio);
    }

    public IEnumerator MoveToPosition(Vector3 targetPos, float duration = 0.5f)
    {
        animator.SetBool("isMoving", true);
        
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
        animator.SetBool("isMoving", false);
    }
}
    
