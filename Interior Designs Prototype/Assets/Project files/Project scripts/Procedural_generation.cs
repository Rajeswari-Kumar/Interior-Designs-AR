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

    public GameObject frontWall, backWall, leftWall, rightWall, ceiling;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnGenerateButtonClick()
    {
        if (float.TryParse(widthInput.text, out float width)) buildingWidth = width;
        if (float.TryParse(heightInput.text, out float height)) buildingHeight = height;
        if (float.TryParse(thicknessInput.text, out float thickness)) wallThickness = thickness;
        if (int.TryParse(windowsInput.text, out int windows)) windowsPerWall = windows;

        foreach (Transform child in transform) Destroy(child.gameObject);

        GenerateBuilding();
    }

    void GenerateBuilding()
    {
        if (SceneManager.GetActiveScene().name == "Room designing scene" || SceneManager.GetActiveScene().name == "Wall designing scene")
        {
            frontWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, buildingWidth / 2), Quaternion.identity);
            backWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, -buildingWidth / 2), Quaternion.Euler(0, 180, 0));
            leftWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(-buildingWidth / 2, buildingHeight / 2, 0), Quaternion.Euler(0, -90, 0));
            rightWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(buildingWidth / 2, buildingHeight / 2, 0), Quaternion.Euler(0, 90, 0));

            PlaceWindows(backWall.transform, buildingWidth);
            PlaceWindows(leftWall.transform, buildingWidth);
            PlaceWindows(rightWall.transform, buildingWidth);

            GenerateCeiling();
        }
        //else if (SceneManager.GetActiveScene().name == "Wall designing scene")
        //{
        //    backWall = InstantiateWall(buildingWidth, buildingHeight, new Vector3(0, buildingHeight / 2, -buildingWidth / 2), Quaternion.Euler(0, 180, 0));
        //    PlaceWindows(backWall.transform, buildingWidth);
        //}
    }

    GameObject InstantiateWall(float width, float height, Vector3 position, Quaternion rotation)
    {
        GameObject wall = Instantiate(wallPrefab, position, rotation, transform);
        wall.transform.localScale = new Vector3(width, height, wallThickness);
        return wall;
    }

    public void PlaceWindows(Transform wall, float wallWidth)
    {
        if (windowPrefab == null || windowsPerWall <= 0) return;

        float spacing = wallWidth / (windowsPerWall + 1);
        for (int i = 1; i <= windowsPerWall; i++)
        {
            Vector3 windowPos = wall.position + wall.right * (spacing * i - wallWidth / 2);
            windowPos.y = buildingHeight / 2;
            windowPos.z -= wallThickness / 5;

            GameObject window = Instantiate(windowPrefab, windowPos, wall.rotation);
            window.transform.parent = wall.transform;

            Vector3 scaleWindow = new Vector3(window.transform.localScale.x, window.transform.localScale.y, window.transform.localScale.z + wallThickness + 2);
            window.transform.localScale = scaleWindow;

            foreach (Canvas canvas in window.GetComponentsInChildren<Canvas>(true))
            {
                if (canvas.renderMode == RenderMode.WorldSpace && canvas.worldCamera == null)
                    canvas.worldCamera = Camera.main;
            }
        }
    }

    void GenerateCeiling()
    {
        Vector3 ceilingPos = new Vector3(0, buildingHeight, 0);
        Quaternion ceilingRot = Quaternion.Euler(-90, 0, 0);
        ceiling = Instantiate(wallPrefab, ceilingPos, ceilingRot, transform);
        ceiling.transform.localScale = new Vector3(buildingWidth, buildingWidth, wallThickness);
    }

    public void AdjustAllWallsAndCeiling(Transform scaledWall)
    {
        if (frontWall == null || backWall == null || leftWall == null || rightWall == null || ceiling == null)
            return;

        float height = scaledWall.localScale.y;
        float thickness = wallThickness;

        float newX = frontWall.transform.localScale.x;
        float newZ = leftWall.transform.localScale.x;

        if (scaledWall == frontWall.transform || scaledWall == backWall.transform)
            newX = scaledWall.localScale.x;
        else if (scaledWall == leftWall.transform || scaledWall == rightWall.transform)
            newZ = scaledWall.localScale.x;

        frontWall.transform.localScale = new Vector3(newX, height, thickness);
        backWall.transform.localScale = new Vector3(newX, height, thickness);
        leftWall.transform.localScale = new Vector3(newZ, height, thickness);
        rightWall.transform.localScale = new Vector3(newZ, height, thickness);

        frontWall.transform.position = new Vector3(0, height / 2, newZ / 2);
        backWall.transform.position = new Vector3(0, height / 2, -newZ / 2);
        leftWall.transform.position = new Vector3(-newX / 2, height / 2, 0);
        rightWall.transform.position = new Vector3(newX / 2, height / 2, 0);

        ceiling.transform.localScale = new Vector3(newX, newZ, thickness);
        ceiling.transform.position = new Vector3(0, height, 0);
    }
}
