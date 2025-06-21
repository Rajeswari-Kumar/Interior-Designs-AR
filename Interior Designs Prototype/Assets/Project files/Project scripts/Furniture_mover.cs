using UnityEngine;
using UnityEngine.InputSystem;

public class FurnitureMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.005f;
    [SerializeField] private float rotationSpeed = 2f;

    private Camera mainCamera;
    private InputManager inputManager;
    private WindowEdgeDistanceDisplay edgeDisplay;

    [SerializeField] private Transform groundPlaneTransform;

    private Vector2 prevTouch0, prevTouch1;
    private bool wasTwoFingerTouchLastFrame = false;

    private void Start()
    {
        mainCamera = Camera.main;
        inputManager = FindObjectOfType<InputManager>();
        edgeDisplay = GetComponent<WindowEdgeDistanceDisplay>();

        if (groundPlaneTransform == null)
        {
            PlaneTrans plane = FindObjectOfType<PlaneTrans>();
            if (plane != null)
                groundPlaneTransform = plane.transform;
        }
    }

    private void Update()
    {
        HandleFurnitureTouchDragOrRotate();
    }

    private void HandleFurnitureTouchDragOrRotate()
    {
        if (Touchscreen.current == null)
            return;

        int touchCount = 0;
        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                touchCount++;
        }

        bool isSelected = (inputManager && inputManager.singleClickObjectSelect == gameObject);

        if (touchCount == 2 && isSelected)
        {
            HandleTwoFingerRotation();
            return;
        }
        else
        {
            wasTwoFingerTouchLastFrame = false;
        }

        // Handle dragging
        if (isSelected)
        {
            Vector2 touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(touchPos);

            if (groundPlaneTransform == null)
                return;

            Plane groundPlane = new Plane(Vector3.up, groundPlaneTransform.position);
            if (groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                transform.position = new Vector3(hitPoint.x, groundPlaneTransform.position.y, hitPoint.z);
            }

            if (edgeDisplay)
                edgeDisplay.UpdateCanvasAndLines();
        }

        // Double-click delete
        if (inputManager && inputManager.doubleClickObjectSelect == gameObject)
        {
            Destroy(gameObject);
        }
    }

    private void HandleTwoFingerRotation()
    {
        var touches = Touchscreen.current.touches;
        var activeTouches = new System.Collections.Generic.List<UnityEngine.InputSystem.Controls.TouchControl>();

        foreach (var t in touches)
        {
            if (t.press.isPressed)
                activeTouches.Add(t);
        }

        if (activeTouches.Count < 2) return;

        Vector2 currentTouch0 = activeTouches[0].position.ReadValue();
        Vector2 currentTouch1 = activeTouches[1].position.ReadValue();

        if (!wasTwoFingerTouchLastFrame)
        {
            prevTouch0 = currentTouch0;
            prevTouch1 = currentTouch1;
            wasTwoFingerTouchLastFrame = true;
            return;
        }

        Vector2 prevVector = prevTouch1 - prevTouch0;
        Vector2 currentVector = currentTouch1 - currentTouch0;

        float angleDelta = Vector2.SignedAngle(prevVector, currentVector);
        transform.Rotate(Vector3.up, angleDelta * rotationSpeed, Space.World);

        prevTouch0 = currentTouch0;
        prevTouch1 = currentTouch1;
    }
}
