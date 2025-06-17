using UnityEngine;
using UnityEngine.InputSystem;

public class Scale_windows : MonoBehaviour
{
    public float scaleAmount = 0.01f;
    public ProportionalOrManualScaling ProportionalScalingOrManualScaling;

    private bool isSelected = false;
    private static Scale_windows currentSelectedWindow;

    public GameObject ToggleCanvas;
    public InputManager inputManager;

    private float initialDistance = 0f;
    private bool isScaling = false;

    private void Start()
    {
        inputManager = FindObjectOfType<InputManager>();
    }

    private void Update()
    {
        HandleSelection();

        if (!isSelected) return;

        HandlePinchScale();
    }

    private void HandleSelection()
    {
        if (inputManager && inputManager.singleClickObjectSelect == gameObject)
        {
            if (currentSelectedWindow == this)
            {
                Deselect();
            }
            isSelected = true;
            currentSelectedWindow = this;
        }
    }

    private void HandlePinchScale()
    {
        var touches = Touchscreen.current?.touches;
        if (touches == null) return;

        // Get active touches
        var activeTouches = new System.Collections.Generic.List<UnityEngine.InputSystem.Controls.TouchControl>();
        foreach (var t in touches)
        {
            if (t.press.isPressed)
                activeTouches.Add(t);
        }

        if (activeTouches.Count < 2)
        {
            isScaling = false;
            return;
        }

        var touch0 = activeTouches[0];
        var touch1 = activeTouches[1];

        Vector2 pos0 = touch0.position.ReadValue();
        Vector2 pos1 = touch1.position.ReadValue();

        float distance = Vector2.Distance(pos0, pos1);

        if (!isScaling)
        {
            initialDistance = distance;
            isScaling = true;
            return; // Start fresh this frame
        }

        float delta = distance - initialDistance;

        if (Mathf.Abs(delta) > 1f)
        {
            Vector2 diff = pos1 - pos0;
            float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            bool scaledWidth = false;
            bool scaledHeight = false;

            if ((angle >= 315 || angle <= 45) || (angle >= 135 && angle <= 225))
            {
                ScaleWindowWidth(delta);
                scaledWidth = true;
            }
            else
            {
                ScaleWindowHeight(delta);
                scaledHeight = true;
            }

            if (ProportionalScalingOrManualScaling.ProportionalScalingWindow)
            {
                RescaleNeighbourWindow(GetNeighbourWindows(), scaledWidth, scaledHeight);
            }

            initialDistance = distance; // Reset for next frame
        }
    }

    private void Deselect()
    {
        isSelected = false;
        currentSelectedWindow = null;
    }

    private void ScaleWindowHeight(float delta)
    {
        Vector3 newScale = transform.localScale;
        newScale.y += delta * scaleAmount;
        newScale.y = Mathf.Max(newScale.y, 0.1f);
        transform.localScale = newScale;
        transform.GetComponent<WindowEdgeDistanceDisplay>().UpdateCanvasAndLines();
    }

    private void ScaleWindowWidth(float delta)
    {
        Vector3 newScale = transform.localScale;
        newScale.x += delta * scaleAmount;
        newScale.x = Mathf.Max(newScale.x, 0.1f);
        transform.localScale = newScale;
        transform.GetComponent<WindowEdgeDistanceDisplay>().UpdateCanvasAndLines();
    }

    private Transform[] GetNeighbourWindows()
    {
        Transform parent = transform.parent;
        if (parent == null) return new Transform[0];

        var neighbours = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in parent)
        {
            if (child != this.transform && child.CompareTag("Window"))
            {
                neighbours.Add(child);
            }
        }

        return neighbours.ToArray();
    }

    private void RescaleNeighbourWindow(Transform[] NeighbourWindows, bool scaleWidth, bool scaleHeight)
    {
        foreach (Transform t in NeighbourWindows)
        {
            Vector3 newScale = t.localScale;
            if (scaleWidth)
                newScale.x = Mathf.Max(transform.localScale.x, 0.1f);
            if (scaleHeight)
                newScale.y = Mathf.Max(transform.localScale.y, 0.1f);
            t.localScale = newScale;
        }
    }

    public void ToggleCanvasFunction()
    {
        ToggleCanvas.SetActive(!ToggleCanvas.activeSelf);
    }
}
