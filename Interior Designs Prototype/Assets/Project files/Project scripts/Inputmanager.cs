using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public Camera mainCamera;
    public float lastClickTime = 0f;
    public float doubleClickThreshold = 0.35f;

    public GameObject singleClickObjectSelect;
    public GameObject doubleClickObjectSelect;

    public bool doubleClicked = false;
    public Ray ray; // Public ray for both single and double click
    public RaycastHit Hit;

    private Renderer previousRenderer;
    private Color originalColor;
    private Vector2 lastTouchPosition;
    public InputActionProperty primaryTouch;
    public WindowEdgeDistanceDisplay edgeDisplay;
  
    private void Awake()
    {
        if (Touchscreen.current == null)
        {
            Debug.LogWarning("Touchscreen not supported.");
        }
    }

    private void Start()
    {
        mainCamera = FindObjectOfType<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
        edgeDisplay = GetComponent<WindowEdgeDistanceDisplay>();
    }

    private void Update()
    {

        HandleTouchInput();

        //HandleMouseInput();

    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float currentClickTime = Time.time;
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (currentClickTime - lastClickTime <= doubleClickThreshold)
            {
                // Handle double click
                doubleClicked = true;
                TryClickObject(true);
            }
            else
            {
                // Handle single click
                doubleClicked = false;
                TryClickObject(false);
            }
            lastClickTime = currentClickTime;
        }
    }

    private void HandleTouchInput()
    {
        var touch = Touchscreen.current?.primaryTouch;
        if (touch == null) return;

        if (touch.press.wasPressedThisFrame)
        {
            lastTouchPosition = touch.position.ReadValue();

            float currentTapTime = Time.time;
            ray = mainCamera.ScreenPointToRay(lastTouchPosition);
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f);

            if (currentTapTime - lastClickTime <= doubleClickThreshold)
            {
                doubleClicked = true;
                TryClickObject(true);
            }
            else
            {
                doubleClicked = false;
                TryClickObject(false);
            }

            lastClickTime = currentTapTime;
        }
    }



    private void TryClickObject(bool isDoubleClick)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (isDoubleClick && hit.transform.CompareTag("Window"))
            {
                // Revert previous color first
                if (previousRenderer != null)
                {
                    previousRenderer.material.color = originalColor;
                    previousRenderer = null;
                }

                doubleClickObjectSelect = hit.transform.gameObject;
                Renderer renderer = doubleClickObjectSelect.GetComponent<Renderer>();

                // Store original color
                originalColor = renderer.material.color;

                // Highlight in cyan
                renderer.material.color = Color.cyan;

                previousRenderer = renderer;

                singleClickObjectSelect = null;
            }
            else if (!isDoubleClick && hit.transform.CompareTag("Window"))
            {
                if (singleClickObjectSelect == hit.transform.gameObject)
                {
                    // Deselect if clicking again
                    if (previousRenderer != null)
                    {
                        previousRenderer.material.color = originalColor;
                        previousRenderer = null;
                    }
                    singleClickObjectSelect = null;
                }
                else
                {
                    // Revert previous first
                    if (previousRenderer != null)
                    {
                        previousRenderer.material.color = originalColor;
                        previousRenderer = null;
                    }
                    singleClickObjectSelect = hit.transform.gameObject;
                    Renderer renderer = singleClickObjectSelect.GetComponent<Renderer>();

                    // Store original color
                    originalColor = renderer.material.color;

                    // Highlight in yellow
                    renderer.material.color = Color.yellow;

                    previousRenderer = renderer;

                    doubleClickObjectSelect = null;
                }
            }
            else if (!isDoubleClick && hit.transform.CompareTag("Wall"))
            {
                if (singleClickObjectSelect == hit.transform.gameObject)
                {
                    singleClickObjectSelect = null;
                }
                else
                {
                    singleClickObjectSelect = hit.transform.gameObject;
                    
                    doubleClickObjectSelect = null;
                }
            }
            else if (!isDoubleClick && hit.transform.CompareTag("Floor"))
            {
                if (singleClickObjectSelect == hit.transform.gameObject)
                {
                    singleClickObjectSelect = null;
                }
                else
                {
                    singleClickObjectSelect = hit.transform.gameObject;

                    doubleClickObjectSelect = null;
                }
            }
            else
            {
                // If we clicked something else or nothing
                if (previousRenderer != null)
                {
                    previousRenderer.material.color = originalColor;
                    previousRenderer = null;
                }
                singleClickObjectSelect = null;
                doubleClickObjectSelect = null;
            }
        }
        else
        {
            // If we clicked in empty space
            if (previousRenderer != null)
            {
                previousRenderer.material.color = originalColor;
                previousRenderer = null;
            }
            singleClickObjectSelect = null;
            doubleClickObjectSelect = null;
        }
    }
}
