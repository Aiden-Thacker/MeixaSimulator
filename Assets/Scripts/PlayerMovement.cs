using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float sprintMultipler = 2.0f;

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

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float speedMultipler = sprintAction.ReadValue<float>() > 0 ? sprintMultipler : 1f;

        float verticalSpeed = moveInput.y * moveSpeed * speedMultipler;
        float horizontalSpeed = moveInput.x * moveSpeed * speedMultipler;

        Vector3 horizontalMovement = new Vector3(horizontalSpeed, 0f, verticalSpeed);
        horizontalMovement = transform.rotation * horizontalMovement;

        moveDirection.x = horizontalMovement.x;
        moveDirection.z = horizontalMovement.z;

        rb.AddForce(moveDirection.normalized, ForceMode.Force);

        isMoving = moveInput.y != 0 || moveInput.x != 0;
    }
}
