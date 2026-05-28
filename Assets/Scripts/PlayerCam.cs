using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [Header("Sensitivity")]
    public float senX;
    public float senY;

    [Header("References")]
    [SerializeField] private Transform orientation;

    [Header("Input")]
    [SerializeField] private InputActionAsset playerControls;
    
    InputAction lookAction;
    Vector2 lookInput;

    float yRotation;
    float xRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lookAction = playerControls.FindActionMap("Player").FindAction("Look");

        lookAction.performed += context => lookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        lookAction.Enable();
    }

    private void OnDisable()
    {
        lookAction.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = lookInput.x * Time.deltaTime * senX;
        float mouseY = lookInput.y * Time.deltaTime * senY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
