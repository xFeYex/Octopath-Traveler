

using Unity.Cinemachine;

public class BattleUnit : MonoBehaviour
{
    public BattleEntity Entity {get; private set;}
    [Header("Base Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("目标选择光标")]
    [SerializeField] private SpriteRenderer targetCursorRenderer;

    [Header("破盾眩晕特效")] 
    [SerializeField] private GameObject breakStunFx;
    [SerializeField] private float breakStunYOffest = 0.4f;
    
    [Header("镜头震动")]
    [SerializeField] private CinemachineImpulseSource impulseSource;
    
    private GameObject _telegraphVfxInstance;
    private BattleUnitDissolveFX _dissolveFX;
    
    /* ----------------------------------------------------------------------------------------- */
    private void Awake()
    {
        _dissolveFX = GetComponent<BattleUnitDissolveFX>();
        targetCursorRenderer.enabled = false;
        UpdateTargetCursorPosition();
    }

    private void UpdateTargetCursorPosition()
    {
        if (spriteRenderer.sprite == null)
            return;
        Vector3 worldCenter = spriteRenderer.bounds.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 localPos = targetCursorRenderer.transform.localPosition;
        localPos.y = localCenter.y;
        localPos.z = localCenter.z - 0.1f;
        targetCursorRenderer.transform.localPosition = localPos;

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
            // 死亡动画
            SetBreakStunVisual(false);
            animator.SetBool("isDead", true);
            StopTelegraphVfx();
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

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("attack");
    }

    public void PlayUseItemAnimation()
    {
        animator.SetTrigger("use");
    }

    public void PlayVictoryAnimation()
    {
        animator.SetTrigger("victory");
    }

    #region 伤害飘字
    
    public Vector3 GetPopupAnchorPosition() => targetCursorRenderer.transform.position;

    #endregion

    #region 破盾

    public void SetBreakStunVisual(bool visible)
    {
        if (visible)
            UpdateBreakStunPosition();
        breakStunFx.SetActive(visible);
    }

    private void UpdateBreakStunPosition()
    {
        // 获取精灵在世界空间中的最高点
        Vector3 worldTop = spriteRenderer.bounds.max;
        
        // 将世界坐标转换为本地坐标，以便在物体自身的坐标系中进行操作
        Vector3 localTop = transform.InverseTransformPoint(worldTop);
        
        // 这个特效物体本身就是锚点，直接移动它的本地位置。
        Transform breakStunTransform = breakStunFx.transform;
        Vector3 localPos = breakStunTransform.localPosition;
        localPos.y = localTop.y + breakStunYOffest;
        breakStunTransform.localPosition = localPos;
    } 

    #endregion

    public void PlayImpulse(float strength)
    {
        impulseSource.GenerateImpulse(strength);
    }

    #region 蓄力动画

    public void StopTelegraphVfx()
    {
        if (_telegraphVfxInstance == null) return;
        Destroy(_telegraphVfxInstance);
        _telegraphVfxInstance = null;   
    }

    public void PlayTelegraphVfx(SkillDataSO skill)
    {
        StopTelegraphVfx();
        if (skill.telegraphVfxPrefab == null) return;
        
        var spawnPos = GetPopupAnchorPosition() + skill.telegraphVfxOffset;
        Transform parent = skill.telegraphVfxAttachToCaster ? transform : null;
        _telegraphVfxInstance = Instantiate(skill.telegraphVfxPrefab, spawnPos, Quaternion.identity, parent);
        
        if (skill.telegraphVfxLifetime > 0f)
            Destroy(_telegraphVfxInstance, skill.telegraphVfxLifetime);
    }

    #endregion

    #region 死亡特效

    public void SetBodyVisible(bool visible)
    {
        spriteRenderer.enabled = visible;
    }

    public void PlayEnemyDissolve(float delay = 0f)
    {
        // 这里只负责桥接，不在BattleUnit里重复写死亡VFX逻辑。
        _dissolveFX.PlayDeathVfx(delay);
    }

    #endregion
}
    
