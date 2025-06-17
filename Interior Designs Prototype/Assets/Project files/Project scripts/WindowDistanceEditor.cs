using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class WindowDistanceEditor : MonoBehaviour
{
    public TMP_InputField distanceInputHorizontal;
    public TMP_InputField distanceInputVertical;
    private Transform CanvasInput;
    private WindowEdgeDistanceDisplay display;
    public bool isEditing = false;

    private bool updatingFromScript = false;

    void Start()
    {
        WindowReferenceHolder holder = GetComponent<WindowReferenceHolder>();
        if (holder == null || holder.windowDisplay == null)
        {
           //   Debug.LogWarning("WindowReferenceHolder not set on canvas.");
            return;
        }
        display = holder.windowDisplay;

        if (display == null)
        {
            //Debug.LogWarning("WindowEdgeDistanceDisplay not found on this object.");
            return;
        }

        CanvasInput = display.gameObject.transform.parent;
        if (CanvasInput == null)
        {
           // Debug.LogWarning("Window is not parented to a wall.");
            return;
        }

        TMP_InputField[] inputs = this.gameObject.GetComponentsInChildren<TMP_InputField>();
        //Debug.Log(inputs.Length);
        if (inputs.Length >= 2)
        {
            distanceInputHorizontal = inputs[0];
            distanceInputVertical = inputs[1];
            distanceInputHorizontal.onEndEdit.AddListener(OnHorizontalDistanceEdited);
            distanceInputVertical.onEndEdit.AddListener(OnVerticalDistanceEdited);
            distanceInputHorizontal.onSelect.AddListener(OnStartEditing);
            distanceInputHorizontal.onDeselect.AddListener(OnEndEditing);
            distanceInputVertical.onSelect.AddListener(OnStartEditing);
            distanceInputVertical.onDeselect.AddListener(OnEndEditing);

        }
    }
    public void OnStartEditing(string _)
    {
        isEditing = true;
        //Debug.Log("editing");
    }

    public void OnEndEditing(string _)
    {
        isEditing = false;
    }

    void OnHorizontalDistanceEdited(string input)
    {
        isEditing = false;
        if (updatingFromScript) return;

        if (TryExtractFloat(input, out float targetDistance))
        {
            MoveWindowHorizontally(targetDistance);
        }
    }

    void OnVerticalDistanceEdited(string input)
    {
        isEditing = false;
        if (updatingFromScript) return;

        if (TryExtractFloat(input, out float targetDistance))
        {
            MoveWindowVertically(targetDistance);
        }
    }



    void MoveWindowHorizontally(float newDist)
    {
        Vector3 windowCenter = display.transform.position;
        Vector3 windowRight = display.transform.right;
        float halfWidth = display.transform.lossyScale.x / 2f;

        Vector3 leftEdge = windowCenter - windowRight * halfWidth;
        Vector3 rightEdge = windowCenter + windowRight * halfWidth;

        Vector3 wallCenter = CanvasInput.position;
        Vector3 wallRight = CanvasInput.right;
        float halfWallWidth = CanvasInput.lossyScale.x / 2f;

        Vector3 leftEdgeWall = wallCenter - wallRight * halfWallWidth;
        Vector3 rightEdgeWall = wallCenter + wallRight * halfWallWidth;

        float distLeft = Vector3.Distance(leftEdge, leftEdgeWall);
        float distRight = Vector3.Distance(rightEdge, rightEdgeWall);
        bool closerToLeft = distLeft < distRight;

        // Determine wall alignment
        Vector3 wallLocalX = CanvasInput.right;
        bool wallAlignedWithX = Mathf.Abs(Vector3.Dot(wallLocalX.normalized, Vector3.right)) > Mathf.Abs(Vector3.Dot(wallLocalX.normalized, Vector3.forward));

        Vector3 targetWallEdge;

        if (closerToLeft)
        {
            targetWallEdge = new Vector3(
                wallAlignedWithX ? leftEdgeWall.x : leftEdge.x,
                leftEdge.y,
                wallAlignedWithX ? leftEdge.z : leftEdgeWall.z);
        }
        else
        {
            targetWallEdge = new Vector3(
                wallAlignedWithX ? rightEdgeWall.x : rightEdge.x,
                rightEdge.y,
                wallAlignedWithX ? rightEdge.z : rightEdgeWall.z);
        }

        Vector3 direction = closerToLeft ? windowRight : -windowRight;
        display.transform.position = targetWallEdge + direction * newDist + direction * halfWidth;

        RefreshDisplay();
    }



    //void MoveWindowHorizontally(float newDist)
    //{
    //    Vector3 windowCenter = display.transform.position;
    //    Vector3 windowRight = display.transform.right;
    //    float halfWidth = display.transform.lossyScale.x / 2f;

    //    Vector3 leftEdge = windowCenter - windowRight * halfWidth;
    //    Vector3 rightEdge = windowCenter + windowRight * halfWidth;

    //    Vector3 wallCenter = CanvasInput.position;
    //    Vector3 wallRight = CanvasInput.right;
    //    float halfWallWidth = CanvasInput.lossyScale.x / 2f;

    //    Vector3 leftEdgeWall = wallCenter - wallRight * halfWallWidth;
    //    Vector3 rightEdgeWall = wallCenter + wallRight * halfWallWidth;

    //    float distLeft = Vector3.Distance(leftEdge, leftEdgeWall);
    //    float distRight = Vector3.Distance(rightEdge, rightEdgeWall);
    //    bool closerToLeft = distLeft < distRight;

    //    Vector3 targetWallEdge = closerToLeft
    //        ? new Vector3(leftEdgeWall.x, leftEdge.y, leftEdge.z)
    //        : new Vector3(rightEdgeWall.x, rightEdge.y, rightEdge.z);



    //    Vector3 direction = closerToLeft ? windowRight : -windowRight;
    //    display.transform.position = targetWallEdge + direction * newDist + direction * halfWidth;

    //    RefreshDisplay();
    //}

    void MoveWindowVertically(float newDist)
    {
        Vector3 windowCenter = display.transform.position;
        Vector3 windowUp = display.transform.up;
        float halfHeight = display.transform.lossyScale.y / 2f;

        Vector3 topEdge = windowCenter + windowUp * halfHeight;
        Vector3 bottomEdge = windowCenter - windowUp * halfHeight;

        Vector3 wallCenter = CanvasInput.position;
        Vector3 wallUp = CanvasInput.up;
        float halfWallHeight = CanvasInput.lossyScale.y / 2f;

        Vector3 topEdgeWall = wallCenter + wallUp * halfWallHeight;
        Vector3 bottomEdgeWall = wallCenter - wallUp * halfWallHeight;

        float distTop = Vector3.Distance(topEdge, topEdgeWall);
        float distBottom = Vector3.Distance(bottomEdge, bottomEdgeWall);
        bool closerToBottom = distBottom < distTop;

        Vector3 targetWallEdge = closerToBottom
            ? new Vector3(bottomEdge.x, bottomEdgeWall.y, bottomEdge.z)
            : new Vector3(topEdge.x, topEdgeWall.y, topEdge.z);

        Vector3 direction = closerToBottom ? windowUp : -windowUp;
        display.transform.position = targetWallEdge + direction * newDist + direction * halfHeight;

        RefreshDisplay();
    }


    void RefreshDisplay()
    {
        updatingFromScript = true;
        display.UpdateCanvasAndLines();
        updatingFromScript = false;
    }

    bool TryExtractFloat(string input, out float value)
{
    value = 0f;
    Match match = Regex.Match(input, @"[-+]?[0-9]*\.?[0-9]+");
    if (match.Success)
    {
        return float.TryParse(match.Value, out value);
    }
    return false;
}

}
