
public class FieldFollower : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    
    [Header("Animator Params")]
    [SerializeField] private string isMovingParam = "isMoving";
    [SerializeField] private string moveXParam = "moveX";
    [SerializeField] private string moveYParam = "moveZ";

    [Header("最小移动阈值")] 
    [SerializeField] private float movementThreshold = 0.001f;
    
    [Header("Gravity")] 
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundSnapForce = -2f; // 地面吸附力
    [SerializeField] private float maxVelocity = -40f;

    private float verticalVelocity;

    #region 对外接口

    /// <summary>
    /// 设置跟随者的动画控制器
    /// </summary>
    /// <param name="definition"></param>
    public void SetupFollower(CharacterDefinitionSO definition)
    {
        // 将角色定义中的动画控制器应用到Animator组件
        animator.runtimeAnimatorController = definition.fieldAnimator;
    }
    
    // 将角色移到目标位置
    public void MoveTo(Vector3 targetPosition, float speed)
    {
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0; // 只在水平面上移动
        
        Vector3 horizontalStep = Vector3.ClampMagnitude(toTarget, Mathf.Max(0f, speed) * Time.deltaTime);
        
        bool isGrounded = characterController.isGrounded;
        
        // 如果贴地 且 垂直速度小于0
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = groundSnapForce;
        }
        else
        {
            // 如果垂直下落则为加速度
            verticalVelocity += gravity * Time.deltaTime;
            if (verticalVelocity < maxVelocity)
                verticalVelocity = maxVelocity;
        }

        // 组合水平和垂直方向的移动向量
        Vector3 movement = horizontalStep;
        movement.y = verticalVelocity * Time.deltaTime;
        
        // 使角色移动
        characterController.Move(movement);
        
        UpdateAnimation(horizontalStep);
    }

    #endregion

    private void UpdateAnimation(Vector3 Setp)
    {
        bool isMoving = Setp.sqrMagnitude > movementThreshold * movementThreshold;
        
        animator.SetBool(isMovingParam, isMoving);

        if (isMoving)
        {
            animator.SetFloat(moveXParam, Setp.x);
            animator.SetFloat(moveYParam, Setp.z); 
        }
    }
    
    public void SnapTo(Vector3 position)
    {
        characterController.enabled = false; // 禁用CharacterController以直接设置位置
        transform.position = position;
        characterController.enabled = true; // 重新启用CharacterController
        
        verticalVelocity = 0f;
    }
}
