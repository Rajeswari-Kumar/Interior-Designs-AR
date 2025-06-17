using UnityEngine;
using UnityEngine.InputSystem;

public class Move_around_windows : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.005f;

    private bool isSelected = false;
    private Camera mainCamera;

    private Renderer rend;
    private WindowEdgeDistanceDisplay edgeDisplay;
    [SerializeField] private GameObject windowCanvas; // Assign this in the Inspector

    private InputManager inputManager; // cache the Input Manager
    public PlayerInput playerInput;
    public InputActionProperty DragTouch;


    private void Start()
    {
        mainCamera = Camera.main;
        rend = GetComponent<Renderer>();
        edgeDisplay = GetComponent<WindowEdgeDistanceDisplay>();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        inputManager = FindObjectOfType<InputManager>();
        playerInput = FindAnyObjectByType<PlayerInput>();
        //primaryTouch = playerInput.actions["Primary touch"];
    }

    private void Update()
    {
        HandleTouchDrag();
    }


    private void MoveWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 moveDirection = new Vector3(mouseX, mouseY, 0);
        transform.localPosition += moveDirection * moveSpeed;

        if (edgeDisplay) edgeDisplay.UpdateCanvasAndLines();
    }

    private void HandleTouchDrag()
    {
        // If there are 2 or more touches, we ignore this and return immediately.
        //if (Touchscreen.current != null && Touchscreen.current.touches.Count >= 2)
        //{
        //    return;
        //}
        if (inputManager && inputManager.singleClickObjectSelect == gameObject)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();

            Ray ray = mainCamera.ScreenPointToRay(touchPos);
            Transform wallTransform = transform.parent;
            if (wallTransform == null) return;

            Plane wallPlane = new Plane(-wallTransform.forward, transform.position);
            if (wallPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                Vector3 localPoint = wallTransform.InverseTransformPoint(hitPoint);
                localPoint.z = 0;

                transform.localPosition = localPoint;

                if (edgeDisplay) edgeDisplay.UpdateCanvasAndLines();
            }
        }
    }
}
