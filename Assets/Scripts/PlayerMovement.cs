using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultiplier = 2.0f;
    public float groundDrag;
    private bool isSprinting;

    [Header("Acceleration")]
    public float acceleration = 8f;
    public float deceleration = 10f;

    [SerializeField] private float currentMoveSpeed;
    private float targetSpeed;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    public float crouchTransitionSpeed = 10f;
    private float startYScale;
    private bool isCrouching;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    [SerializeField] private bool readyToJump;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [SerializeField] private bool isGrounded;

    public Transform orientation;

    private bool isMoving;
    private Vector3 moveDirection = Vector3.zero;
    private Rigidbody rb;

    [Header("Input")]
    [SerializeField] private InputActionAsset playerControls;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;
    private Vector2 moveInput;

    public enum MovementState
    {
        Walking,
        Sprinting,
        Air,
        Crouching,
    }
    [Header("Player State")]
    [SerializeField] private MovementState state;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveAction = playerControls.FindActionMap("Player").FindAction("Move");
        jumpAction = playerControls.FindActionMap("Player").FindAction("Jump");
        sprintAction = playerControls.FindActionMap("Player").FindAction("Sprint");
        crouchAction = playerControls.FindActionMap("Player").FindAction("Crouch");

        moveAction.performed += context => moveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => moveInput = Vector2.zero;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        startYScale = transform.localScale.y;

        readyToJump = true;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        crouchAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        crouchAction.Disable();
    }

    private void Update()
    {
        float debugHeight = playerHeight * 0.5f + 0.2f;
        Vector3 debugLine = transform.TransformDirection(Vector3.down) * debugHeight;

        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        Debug.DrawRay(transform.position, debugLine, Color.red);

        isSprinting = sprintAction.ReadValue<float>() > 0;

        isCrouching = crouchAction.ReadValue<float>() > 0;

        StateHandler();

        if (moveInput.magnitude > 0.1f)
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, 0f, deceleration * Time.deltaTime);
        }

        SpeedControl();

        Crouch();

        if (isGrounded && readyToJump && !isCrouching && jumpAction.ReadValue<float>() > 0)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if(isGrounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * 10f, ForceMode.Force);
        }
        else if(!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * airMultiplier * 10f, ForceMode.Force);
        }
    }

    void SpeedControl()
    {
        Vector3 flatVal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float maxSpeed = targetSpeed;

        if(flatVal.magnitude > maxSpeed)
        {
            Vector3 limitedVal = flatVal.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVal.x, rb.linearVelocity.y, limitedVal.z);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void Crouch()
    {
        float targetYScale = isCrouching ? crouchYScale : startYScale;

        Vector3 targetScale = new Vector3(transform.localScale.x, targetYScale, transform.localScale.z);

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, crouchTransitionSpeed * Time.deltaTime);
    }

    void StateHandler()
    {
        if (isCrouching)
        {
            state = MovementState.Crouching;
            targetSpeed = crouchSpeed;
        }
        else if (!isGrounded)
        {
            state = MovementState.Air;
            targetSpeed = moveSpeed;
        }
        else if (isSprinting)
        {
            state = MovementState.Sprinting;
            targetSpeed = moveSpeed * sprintMultiplier;
        }
        else
        {
            state = MovementState.Walking;
            targetSpeed = moveSpeed;
        }
    }
}
