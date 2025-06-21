using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
public class Scale_wall : MonoBehaviour
{
    public float scaleAmount = 0.01f;
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;

    private Renderer wallRenderer;
    private bool isSelected = false;
    private static Scale_wall currentSelectedWall;
    private float lastWallWidth;

    private float initialDistance = 0f;
    private bool isScaling = false;
    private InputManager inputManager;
    public bool canScale = false;
 // You must tag the button GameObject as "WallToggle"


    private void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        wallRenderer.material.color = defaultColor;
        inputManager = FindObjectOfType<InputManager>();
        lastWallWidth = transform.localScale.x;
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
                if (currentSelectedWall != null && currentSelectedWall != this)
                    currentSelectedWall.Deselect();

                isSelected = true;
                currentSelectedWall = this;
            }
        
    }

    public void WallScaling() => canScale = !canScale;

    private void Deselect()
    {
        isSelected = false;
        currentSelectedWall = null;
    }

    private void HandlePinchScale()
    {
        if(FindObjectOfType<Toggle>().isOn == true)
        {
            var touches = Touchscreen.current?.touches;
            if (touches == null) return;

            var activeTouches = new List<UnityEngine.InputSystem.Controls.TouchControl>();
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
                return;
            }

            float delta = distance - initialDistance;

            if (Mathf.Abs(delta) > 1f)
            {
                Vector2 diff = pos1 - pos0;
                float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                angle = (angle + 360f) % 360f;

                if ((angle >= 315 || angle <= 45) || (angle >= 135 && angle <= 225))
                {
                    ScaleWidth(delta);
                }
                else
                {
                    //ScaleHeight(delta);
                }

                initialDistance = distance;
            }
        
        }
    }

    private void ScaleHeight(float delta)
    {
        Vector3 newScale = transform.localScale;
        newScale.y += delta * scaleAmount;
        newScale.y = Mathf.Max(newScale.y, 0.1f);
        transform.localScale = newScale;
    }

    private void ScaleWidth(float delta)
    {
        //if(currentSelectedWall == this.gameObject)
        {

        Vector3 newScale = transform.localScale;
        newScale.x += delta * scaleAmount;
        newScale.x = Mathf.Max(newScale.x, 0.1f);
        transform.localScale = newScale;

        Transform[] existingWindows = GetNeighbourWindows();
        int numberOfWindows = existingWindows.Length;
        DeletePreviousWindows(existingWindows);
        FindObjectOfType<Procedural_generation>()?.PlaceWindows(this.transform, newScale.x);

        float newWallWidth = transform.localScale.x;
        float windowWidth = lastWallWidth / Mathf.Max(numberOfWindows, 1);

        if (Mathf.RoundToInt(windowWidth) == Mathf.RoundToInt(newWallWidth - lastWallWidth))
        {
            FindObjectOfType<Procedural_generation>().windowsPerWall += 1;
            lastWallWidth = newWallWidth;
        }

        if (Mathf.RoundToInt(lastWallWidth - newWallWidth) >= Mathf.RoundToInt(windowWidth))
        {
            FindObjectOfType<Procedural_generation>().windowsPerWall -= 1;
            lastWallWidth = newWallWidth;
        }

        FindObjectOfType<Procedural_generation>()?.AdjustAllWallsAndCeiling(this.transform);
        }


    }

    private Transform[] GetNeighbourWindows()
    {
        Transform parent = this.transform;
        if (parent == null) return new Transform[0];

        var neighbours = new List<Transform>();
        foreach (Transform child in parent)
        {
            if (child != transform && child.CompareTag("Window"))
            {
                neighbours.Add(child);
            }
        }
        return neighbours.ToArray();
    }

    private void DeletePreviousWindows(Transform[] neighbours)
    {
        foreach (Transform t in neighbours)
        {
            Destroy(t.gameObject);
        }
    }

    void MakeWallTransparent(float transparency, GameObject wall)
    {
        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer == null) return;

        Material mat = wallRenderer.material;
        if (mat == null) return;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        Color color = mat.color;
        color.a = transparency;
        mat.color = color;
    }
}
