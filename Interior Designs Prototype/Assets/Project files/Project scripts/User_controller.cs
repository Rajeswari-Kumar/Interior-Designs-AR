using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class User_controller : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Rotation")]
    public float gravity = -9.81f;
    public float gyroRotationSpeed = 1.5f;

    [Header("Input")]
    public InputActionProperty move; // Bind to "Vector2" in Input System
    private Touchscreen touchscreen;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private AttitudeSensor attitude_;

    public GameObject cameraContainer;

    private Quaternion initialRotation;

    private void Start()
    {
        cameraContainer = this.gameObject.GetComponentInChildren<Camera>().gameObject;
        controller = GetComponent<CharacterController>();

        touchscreen = Touchscreen.current;

        // Try to find Attitude Sensor
        attitude_ = InputSystem.GetDevice<AttitudeSensor>();

        if (attitude_ != null)
        {
            InputSystem.EnableDevice(attitude_);

            // Capture initial phone rotation as baseline
            initialRotation = attitude_.attitude.ReadValue();

            Debug.Log("Attitude sensor is available.");
        }
        else
        {
            Debug.LogError("Attitude sensor is not available.");
        }
    }


    private void Update()
    {
        HandleGravity();

        // Apply phone's rotation to the transform
        if (attitude_ != null)
        {
            Quaternion phoneRotation = attitude_.attitude.ReadValue();
            Quaternion relativeRotation = phoneRotation * Quaternion.Inverse(initialRotation);

            // Rotate 90º around X to match phone's forward to Unity's forward
            Quaternion adjustment = Quaternion.Euler(90, 0, 0);
            
            transform.localRotation = new Quaternion(-phoneRotation.x, -phoneRotation.z, -phoneRotation.y,phoneRotation.w) * adjustment;

        }

        HandleMovement();
    }


    private void HandleGravity()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        if (move.action == null) return;

        Vector2 input = move.action.ReadValue<Vector2>();

        Vector3 moveDirection = transform.forward * input.y + transform.right * input.x;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        controller.Move(velocity * Time.deltaTime);
    }
}
