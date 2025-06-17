using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Scale_wall : MonoBehaviour
{
    public float scaleAmount = 0.01f;
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;

    private Renderer wallRenderer;
    private bool isSelected = false;
    private static Scale_wall currentSelectedWall;
    private float lastWallWidth;

    private void Start()
    {
        wallRenderer = GetComponent<Renderer>();
        wallRenderer.material.color = defaultColor;

        if (SceneManager.GetActiveScene().name == "Room designing scene")
            this.enabled = false;

        lastWallWidth = transform.localScale.x;
    }

    private void Update()
    {
        //HandleSelection();

        if (!isSelected) return;

        //float scroll = Input.GetAxis("Mouse ScrollWheel");
        //if (Mathf.Abs(scroll) > 0.001f)
        //{
        //    // Left Click + Scroll = Scale Height
        //    if (Input.GetMouseButton(1))
        //    {
        //        ScaleHeight(scroll * 100);
        //    }
        //    // Right Click + Scroll = Scale Width
        //    else if (Input.GetMouseButton(0))
        //    {
        //        ScaleWidth(scroll * 100);
        //    }
        //}
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    if (currentSelectedWall != null)
                        currentSelectedWall.Deselect();

                    isSelected = true;
                    currentSelectedWall = this;
                    //wallRenderer.material.color = selectedColor;
                    //MakeWallTransparent(1, this.gameObject);
                }
                else if (currentSelectedWall == this)
                {
                    Deselect();
                }
            }
            else if (currentSelectedWall == this)
            {
                Deselect();
            }
        }
    }

    private void Deselect()
    {
        isSelected = false;
        currentSelectedWall = null;
        //wallRenderer.material.color = defaultColor;
    }

    private void ScaleHeight(float delta)
    {
        Vector3 newScale = transform.localScale;
        newScale.y += delta * scaleAmount;
        newScale.y = Mathf.Max(newScale.y, 0.1f); // Clamp minimum
        transform.localScale = newScale;
    }

    private void ScaleWidth(float delta)
    {
        Vector3 newScale = transform.localScale;
        newScale.x += delta * scaleAmount;
        newScale.x = Mathf.Max(newScale.x, 0.1f); // Clamp minimum
        transform.localScale = newScale;

        // Optionally update window layout
        Transform[] existingWindows = GetNeighbourWindows();
        int numberOfWindows = existingWindows.Length;
        DeletePreviousWindows(existingWindows);
        FindObjectOfType<Procedural_generation>()?.PlaceWindows(this.transform, newScale.x);

        float newWallWidth = transform.localScale.x;
        float windowWidth = lastWallWidth / numberOfWindows;

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
    }

    private Transform[] GetNeighbourWindows()
    {
        Transform parent = this.transform;
        if (parent == null) return new Transform[0];

        var neighbours = new System.Collections.Generic.List<Transform>();
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
