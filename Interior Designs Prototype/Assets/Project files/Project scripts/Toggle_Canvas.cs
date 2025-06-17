
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toggle_Canvas : MonoBehaviour
{
    public GameObject ToggleCanvas;
    public void ToggleCanvasFunction()
    {
        ToggleCanvas.SetActive(!ToggleCanvas.activeSelf);
    }
}
