using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Procedural_generation : MonoBehaviour
{
    public float buildingWidth = 10f;
    public float buildingHeight = 3f;
    public float wallThickness = 0.2f;

    public GameObject wallPrefab;
    public GameObject windowPrefab;
    public GameObject doorPrefab;

    public int windowsPerWall = 2;
    public TMP_InputField widthInput;
    public TMP_InputField heightInput;
    public TMP_InputField thicknessInput;
    public TMP_InputField windowsInput;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void Update()
    {
        
    }


    public void OnGenerateButtonClick()
    {
        if (float.TryParse(widthInput.text, out float width))
            buildingWidth = width;

        if (float.TryParse(heightInput.text, out float height))
            buildingHeight = height;

        if (float.TryParse(thicknessInput.text, out float thickness))
            wallThickness = thickness;

        if (int.TryParse(windowsInput.text, out int windows))
            windowsPerWall = windows;

        // Clear previous generation if necessary
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Call generate
        GenerateBuilding();
    }

    void GenerateBuilding()
    {
        
        if(SceneManager.GetActiveScene().name == "Room designing scene")
        {
            // FRONT WALL (with door)
            GameObject frontWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, buildingWidth / 2), Quaternion.identity);
            //PlaceDoor(frontWall.transform, buildingWidth, buildingHeight);
            
            // BACK WALL (with windows)
            GameObject backWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, -buildingWidth / 2), Quaternion.Euler(0, 180, 0));
            PlaceWindows(backWall.transform, buildingWidth);

            // LEFT WALL (with windows)
            GameObject leftWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(-buildingWidth / 2, buildingHeight / 2, 0), Quaternion.Euler(0, -90, 0));
            PlaceWindows(leftWall.transform, buildingWidth);

            // RIGHT WALL (with windows)
            GameObject rightWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(buildingWidth / 2, buildingHeight / 2, 0), Quaternion.Euler(0, 90, 0));
            PlaceWindows(rightWall.transform, buildingWidth);

            GenerateCeiling();
        }          

        else if(SceneManager.GetActiveScene().name == "Wall designing scene")
        {
            GameObject backWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, -buildingWidth / 2), Quaternion.Euler(0, 180, 0));
            PlaceWindows(backWall.transform, buildingWidth);
        }
    }

    GameObject InstantiateWall(float width, float height, Vector3 position, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, position, rotation, transform);
        wall.transform.localScale = new Vector3(width, height, wallThickness);
        return wall;
    }

    //void PlaceDoor(Transform wall, float wallWidth)
    //{
    //    if (doorPrefab == null) return;
    //    Vector3 doorPos = wall.position + new Vector3(0, 1, -wallThickness / 2);
    //    Instantiate(doorPrefab, doorPos, wall.rotation, wall);
    //}

    void PlaceDoor(Transform wall, float wallWidth, float wallHeight)
    {
        if (doorPrefab == null) return;

        //float doorHeight = doorPrefab.transform.localScale.y;
        float doorHeight = wallHeight / 10f;
        Vector3 doorPos = wall.position + new Vector3(0, -wall.localScale.y, -wallThickness / 2);

        GameObject door = Instantiate(doorPrefab, doorPos, wall.rotation, wall);
        door.transform.localScale = new Vector3(1f, doorHeight, wallThickness / 2); // Adjust width/height as needed
    }


    public void PlaceWindows(Transform wall, float wallWidth)
    {
        if (windowPrefab == null || windowsPerWall <= 0) return;
        GameObject WindowPrefab = windowPrefab;
        float spacing = wallWidth / (windowsPerWall + 1);
        for (int i = 1; i <= windowsPerWall; i++)
        {
            Vector3 windowPos = wall.position + wall.right * (spacing * i - wallWidth / 2);
            windowPos.y = buildingHeight/2; // typical window height
            windowPos.z -= wallThickness / 5;
            GameObject window = Instantiate(WindowPrefab, windowPos, wall.rotation);
            window.transform.parent = wall.transform;
            Vector3 scaleWindow = new Vector3(window.transform.localScale.x,window.transform.localScale.y, window.transform.localScale.z + wallThickness + 2);
            window.transform.localScale = scaleWindow;
            Canvas[] canvases = window.GetComponentsInChildren<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                {
                    canvas.worldCamera = Camera.main;
                }
            }
        }
    }

    void GenerateCeiling()
    {
        float ceilingHeight = buildingHeight; // Top of the building
        Vector3 ceilingPos = new Vector3(0, ceilingHeight, 0);
        Quaternion ceilingRot = Quaternion.Euler(-90, 0, 0); // Horizontal orientation

        GameObject ceiling = Instantiate(wallPrefab, ceilingPos, ceilingRot, transform);
        ceiling.transform.localScale = new Vector3(buildingWidth, buildingWidth, wallThickness);
    }


    void MaintainWorldScale(Transform child, Transform parent)
    {
        Vector3 parentScale = parent.lossyScale;
        child.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );
    }

}
