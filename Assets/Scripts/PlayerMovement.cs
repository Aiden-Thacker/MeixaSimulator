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

        float targetSpeed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        if (moveInput.magnitude > 0.1f)
        {
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, 0f, deceleration * Time.deltaTime);
        }

        SpeedControl();

        if(isGrounded && readyToJump && jumpAction.ReadValue<float>() > 0)
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
        float speedMultipler = isSprinting ? sprintMultiplier : 1f;

        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;

        if(isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * speedMultipler * 10f, ForceMode.Force);
        }
        else if(!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * currentMoveSpeed * speedMultipler * airMultiplier * 10f, ForceMode.Force);
        }

        //isMoving = moveInput.y != 0 || moveInput.x != 0;
    }

    void SpeedControl()
    {
        float speedMultipler = isSprinting ? sprintMultiplier : 1f;

        Vector3 flatVal = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float maxSpeed = currentMoveSpeed;

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

    }
}
