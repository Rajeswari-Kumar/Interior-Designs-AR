using UnityEngine;

public class AssignCanvasCamera : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = Camera.main;
            if (canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }
        }
    }
}
