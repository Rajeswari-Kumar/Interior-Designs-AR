using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Height_Width_Display : MonoBehaviour
{
    public GameObject canvasPrefab; // Assign World Space Canvas prefab
    public Vector3 offset = new Vector3(0, 0, -0.55f); // In front of window

    private Dictionary<GameObject, GameObject> windowToCanvasMap = new();
    private Dictionary<GameObject, Transform> windowReferencePoints = new();

    public bool windowMeasurementShow = false;

    public void CreateAllMeasurementCanvases()
    {
        GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");

        foreach (GameObject window in windows)
        {
            CreateCanvasForWindow(window);
        }
    }

    public void CreateCanvasForWindow(GameObject window)
    {
        if (window == null || windowToCanvasMap.ContainsKey(window))
            return;

        Transform referencePoint = GetWindowReferencePoint(window);
        if (referencePoint == null)
        {
            Debug.LogWarning($"No reference point with tag 'Window Canvas' found in {window.name}");
            return;
        }

        GameObject canvas = Instantiate(canvasPrefab);
        canvas.AddComponent<Maintain_UI_Scale>(); // Prevents scaling with parent
        canvas.transform.position = referencePoint.position + referencePoint.forward.normalized * offset.z;
        canvas.transform.rotation = referencePoint.rotation;

        windowToCanvasMap[window] = canvas;
        windowReferencePoints[window] = referencePoint;

        //Debug.Log($"Created canvas for window: {window.name}");
    }

    public bool HasCanvas(GameObject window)
    {
        return windowToCanvasMap.ContainsKey(window);
    }

    private Transform GetWindowReferencePoint(GameObject window)
    {
        foreach (Transform child in window.transform)
        {
            if (child.CompareTag("Window Canvas"))
                return child;
        }
        return null;
    }

    void LateUpdate()
    {
        var keys = new List<GameObject>(windowToCanvasMap.Keys);

        foreach (GameObject window in keys)
        {
            UpdateCanvasForWindow(window);

            //if (window == null || windowToCanvasMap[window] == null)
            //    continue;

            //GameObject canvas = windowToCanvasMap[window];
            //Vector3 scale = window.transform.lossyScale;
            //float width = Mathf.Round(scale.x * 100f) / 100f;
            //float height = Mathf.Round(scale.y * 100f) / 100f;

            //foreach (Transform measure in canvas.transform)
            //{
            //    if (measure.name == "Height")
            //        measure.GetComponent<TextMeshProUGUI>().text = "<----- " + height.ToString("F2") + "m ----->";
            //    else if (measure.name == "Width")
            //        measure.GetComponent<TextMeshProUGUI>().text = "<----- " + width.ToString("F2") + "m ----->";
            //}

            //if (windowReferencePoints.TryGetValue(window, out Transform refPoint) && refPoint != null)
            //{
            //    Vector3 forwardOffset = refPoint.forward.normalized * offset.z;
            //    canvas.transform.position = refPoint.position + forwardOffset;
            //}
        }
    }
    public void UpdateCanvasForWindow(GameObject window)
    {
        if (window == null || !windowToCanvasMap.ContainsKey(window))
            return;

        GameObject canvas = windowToCanvasMap[window];

        Vector3 scale = window.transform.localScale * 10;
        float width = Mathf.Round(scale.x * 100f) / 100f;
        float height = Mathf.Round(scale.y * 100f) / 100f;

        foreach (Transform measure in canvas.transform)
        {
            if (measure.name == "Height")
                measure.GetComponent<TextMeshProUGUI>().text = "<----- " + height.ToString("F2") + "m ----->";
            else if (measure.name == "Width")
                measure.GetComponent<TextMeshProUGUI>().text = "<----- " + width.ToString("F2") + "m ----->";
        }

        if (windowReferencePoints.TryGetValue(window, out Transform refPoint) && refPoint != null)
        {
            Vector3 forwardOffset = refPoint.forward.normalized * offset.z;
            canvas.transform.position = refPoint.position + forwardOffset;
        }
    }

    public void DeleteAllMeasurementCanvases()
    {
        foreach (var canvas in windowToCanvasMap.Values)
        {
            if (canvas != null)
                Destroy(canvas);
        }

        windowToCanvasMap.Clear();
        windowReferencePoints.Clear();
        windowMeasurementShow = false;
    }
}
