
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.HID;

[Serializable]
public class NamedInventory
{
    public string name;
    public GameObject gameObject;
}

public class NamedGameObjectDictionary : MonoBehaviour
{
    [Header("Inventory List")]
    [SerializeField]
    private List<NamedInventory> objectList = new List<NamedInventory>();

    private Dictionary<string, GameObject> objectDict;

    [Header("Currently Selected Window")]
    public GameObject selectedWindow;
    private Color originalColor;
    private bool hasOriginalColor = false;

    [Header("Fixture Selection")]
    public GameObject fixtureMarkerPrefab; // Assign a small sphere prefab in Inspector
    private GameObject selectedFixturePoint;
    public bool templateFixture = false;

    // New flag: True if a "Fixture template" is selected, blocks spawning fixture points
    public bool isTemplateFixtureSelected = false;

    // New flag: True if a fixture point is selected, blocks spawning new fixture points
    private bool isFixturePointSelected = false;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.25f; // seconds
    private GameObject lastClickedObject = null;
    private Vector3 fixtureOriginalLocalScale;
    private Camera MainCamera;
    public InputManager inputManager;
    private void Awake()
    {
        objectDict = new Dictionary<string, GameObject>();
        foreach (var item in objectList)
        {
            if (!string.IsNullOrEmpty(item.name) && item.gameObject != null)
            {
                objectDict[item.name] = item.gameObject;
            }
        }
        fixtureOriginalLocalScale = fixtureMarkerPrefab.transform.localScale;
    }
    private void Start()
    {
        MainCamera = Camera.main;
        inputManager = FindObjectOfType<InputManager>();
    }
    /// <summary>
    /// Get object from dictionary by name.
    /// </summary>
    public GameObject GetObjectByName(string name)
    {
        if (objectDict.TryGetValue(name, out GameObject obj))
        {
            return obj;
        }

        Debug.LogWarning($"Object with name '{name}' not found in dictionary.");
        return null;
    }
    void Update()
    {
        HandleSelection();
    }
    private void HandleSelection()
    {
        GameObject clickedObject = null;
        bool isDoubleClick = false;

        if (inputManager && inputManager.doubleClickObjectSelect != null)
        {
            clickedObject = inputManager.doubleClickObjectSelect;
            if(clickedObject.CompareTag("Fixture template"))
                inputManager.doubleClickObjectSelect = null;

            isDoubleClick = (clickedObject == lastClickedObject && Time.time - lastClickTime < doubleClickThreshold);
        }
        else if (inputManager && inputManager.singleClickObjectSelect != null)
        {
            clickedObject = inputManager.singleClickObjectSelect;
            if (clickedObject.CompareTag("Fixture template"))
                inputManager.singleClickObjectSelect = null;

            isDoubleClick = (clickedObject == lastClickedObject && Time.time - lastClickTime < doubleClickThreshold);
        }

        if (clickedObject != null)
        {
            lastClickTime = Time.time;
            lastClickedObject = clickedObject;

            ProcessRaycast(inputManager.Hit, clickedObject, isDoubleClick);
        }
    }




    private void ProcessRaycast(Vector3 hit, GameObject clickedObj, bool isDoubleClick)
    {
        if (clickedObj == selectedFixturePoint)
        {
            if (isDoubleClick)
            {
                DeleteFixturePoint();
            }
            else
            {
                DeselectFixturePoint();
            }
            return;
        }

        if (clickedObj.CompareTag("Fixture template"))
        {
            SelectFixturePoint(clickedObj);
            return;
        }

        if (isFixturePointSelected)
        {
            Debug.Log("Fixture point already selected — cannot create another.");
            return;
        }

        if (clickedObj.CompareTag("Window"))
        {
            SelectWindow(clickedObj);
            return;
        }

        if (clickedObj.CompareTag("Wall") || clickedObj.CompareTag("Floor"))
        {
            SpawnFixturePoint(hit, clickedObj.transform);
            return;
        }

        if (selectedWindow != null)
            DeselectWindow();

        if (isFixturePointSelected)
            DeselectFixturePoint();
    }


    /// <summary>
    /// Spawn object 2 units in front of camera.
    /// </summary>
    public void SpawnObjects(string objectName)
    {
        
        Vector3 spawnPosition = new Vector3(0,1,0);
        Instantiate(GetObjectByName(objectName), spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Replace selected window with new prefab.
    /// </summary>
    public void ReplaceSelectedWindow(string newObjectName)
    {
        if (selectedWindow == null)
        {
            Debug.LogWarning("No window is currently selected.");
            return;
        }

        GameObject replacementPrefab = GetObjectByName(newObjectName);

        if (replacementPrefab == null)
        {
            Debug.LogWarning($"No prefab found for '{newObjectName}'.");
            return;
        }

        Vector3 position = selectedWindow.transform.position +
                           (selectedWindow.CompareTag("Window") || selectedWindow.CompareTag("Fixture template")
                               ? new Vector3(0, -2f, 0.5f)
                               : Vector3.zero);

        Quaternion rotation = selectedWindow.transform.rotation;
        Transform parent = selectedWindow.transform.parent;

        // Cache current tag before destroying
        string tag = selectedWindow.tag;

        // Destroy the old one
        Destroy(selectedWindow);

        GameObject newWindow;

        if (tag == "Fixture template")
        {
            // Instantiate without parenting first to preserve original rotation and scale
            newWindow = Instantiate(replacementPrefab, position, replacementPrefab.transform.rotation);
            newWindow.transform.SetParent(parent, worldPositionStays: true); // maintain world position and scale
        }
        else
        {
            // For non-fixture templates, preserve the old scale and rotation
            newWindow = Instantiate(replacementPrefab, position, rotation, parent);
            newWindow.transform.localScale = selectedWindow != null ? selectedWindow.transform.localScale : Vector3.one;
        }

        selectedWindow = newWindow;
        Debug.Log($"Replaced window with '{newObjectName}'");
        isFixturePointSelected = false;
    }


    /// <summary>
    /// Called from UI button.
    /// </summary>
    public void OnInventoryButtonClick(string objectName)
    {
        //if (FindObjectOfType<Fix_the_template>().isFixed == true)
            ReplaceSelectedWindow(objectName);
    }

    /// <summary>
    /// Select a window, or deselect if already selected.
    /// Also sets the flag to block fixture spawning if fixture template selected.
    /// </summary>
    public void SelectWindow(GameObject window)
    {
        // Clicked the same window again → toggle deselect
        if (selectedWindow == window)
        {
            DeselectWindow();
            return;
        }

        // Deselect previous if selecting a new one
        if (selectedWindow != null)
        {
            //DeselectWindow();
        }

        Renderer rend = window.GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
            hasOriginalColor = true;
            rend.material.color = Color.black; // highlight
        }

        selectedWindow = window;
        //Debug.Log($"Selected window: {window.name}");

        // Update flag depending on tag
        isTemplateFixtureSelected = window.CompareTag("Fixture template");
    }

    /// <summary>
    /// Deselect the current window and reset color and flags.
    /// </summary>
    public void DeselectWindow()
    {
        if (selectedWindow == null)
            return;

        Renderer rend = selectedWindow.GetComponent<Renderer>();
        if (rend != null && hasOriginalColor)
        {
            //rend.material.color = Color.white;
            Debug.Log($"Deselected window: {selectedWindow.name}");
        }

       

        // Reset flag
        isTemplateFixtureSelected = false;

        selectedWindow = null;
        hasOriginalColor = false;
    }

    /// <summary>
    /// Select a fixture point and highlight it.
    /// </summary>
    public void SelectFixturePoint(GameObject fixturePoint)
    {
        if (selectedFixturePoint == fixturePoint)
        {
            DeselectFixturePoint(); // Toggle off
            return;
        }

        if (selectedFixturePoint != null)
        {
            DeselectFixturePoint();
        }

        selectedFixturePoint = fixturePoint;

        Renderer rend = selectedFixturePoint.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = Color.green;

        isFixturePointSelected = true;
        selectedWindow = fixturePoint;

        Debug.Log("Fixture point selected");
    }

    /// <summary>
    /// Deselect the current fixture point and reset its color.
    /// </summary>
    public void DeselectFixturePoint()
    {
        if (selectedFixturePoint == null)
            return;

        Renderer rend = selectedFixturePoint.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.white; // assuming white is default
        }

        //Debug.Log($"Deselected fixture point: {selectedFixturePoint.name}");

        selectedFixturePoint = null;
        isFixturePointSelected = false;
    }

    public void templateFixturebool() => templateFixture = !templateFixture;

    //public void SpawnFixturePoint(Vector3 position, Transform parent)
    //{
    //    if (templateFixture && isFixturePointSelected == false)
    //    {
    //        if (selectedFixturePoint != null)
    //            Destroy(selectedFixturePoint); // Replace previous

    //        // Instantiate WITHOUT parent first (to keep prefab scale)
    //        selectedFixturePoint = Instantiate(fixtureMarkerPrefab, position, Quaternion.identity);

    //        // Now assign parent with worldPositionStays = true to maintain world position
    //        selectedFixturePoint.transform.SetParent(parent, worldPositionStays: true);

    //        Vector3 originalGlobalScale = fixtureMarkerPrefab.transform.lossyScale;
    //        Vector3 parentGlobalScale = parent.lossyScale;

    //        // Adjust local scale to maintain original global scale despite parent scaling
    //        selectedFixturePoint.transform.localScale = new Vector3(
    //            originalGlobalScale.x / parentGlobalScale.x,
    //            originalGlobalScale.y / parentGlobalScale.y,
    //            originalGlobalScale.z / parentGlobalScale.z
    //        );
    //    }
    //}
    public void SpawnFixturePoint(Vector3 position, Transform parent)
    {
        if (templateFixture && isFixturePointSelected == false)
        {
            if (selectedFixturePoint != null)
                Destroy(selectedFixturePoint); // Replace previous

            // Step 1: Instantiate
            selectedFixturePoint = Instantiate(fixtureMarkerPrefab, position, Quaternion.identity);

            // Step 2: Parent
            selectedFixturePoint.transform.SetParent(parent, worldPositionStays: true);

            // Step 3: Reset local scale
            selectedFixturePoint.transform.localScale = fixtureMarkerPrefab.transform.localScale;
            Debug.Log("aaa");
            //selectedFixturePoint.transform.localScale = fixtureOriginalLocalScale
        }
    }



    private void DeleteFixturePoint()
    {
        if (selectedFixturePoint != null)
        {
            //Debug.Log($"Deleted fixture point: {selectedFixturePoint.name}");
            Destroy(selectedFixturePoint);
            selectedFixturePoint = null;
            isFixturePointSelected = false;
        }
    }

}