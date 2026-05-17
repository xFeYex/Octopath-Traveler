using System;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;
    [SerializeField] private float speed = 5f;

    [Header("Gravity")] 
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float groundSnapForce = -2f; // 地面吸附力
    [SerializeField] private float maxVelocity = -40f;
    
    private float verticalVelocity;
    private Vector2 movementInput;
    private bool isMoving;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        Movemnet();
        SetAnimation();
    }

    private void Movemnet()
    {
        var input = InputSystemController.Instance;
        
        if (input == null) return;
        
        movementInput = input.GetMovementInput();
        
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
        
        Vector3 velocity = new Vector3(movementInput.x, 0, movementInput.y) * speed;
        velocity.y = verticalVelocity;
        
        characterController.Move(velocity * Time.deltaTime);
    }

    private void SetAnimation()
    {
        if (animator == null) return;
        
        isMoving = movementInput.magnitude > 0;
        animator.SetBool("isMoving", isMoving);

        if (isMoving)
        {
            animator.SetFloat("moveX", movementInput.x);
            animator.SetFloat("moveY", movementInput.y);
        }
    }
}