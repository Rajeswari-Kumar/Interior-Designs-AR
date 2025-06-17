using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fix_the_template : MonoBehaviour
{
    public bool isFixed = false;
    public void fix_template()
    {
        isFixed = true;
        GameObject[] allWindows = GameObject.FindGameObjectsWithTag("Window");

        foreach (GameObject window in allWindows)
        {
            Scale_windows scaler = window.GetComponent<Scale_windows>();
            if (scaler != null)
            {
                Destroy(scaler);
                //scaler.enabled = false;
                //Debug.Log($"Disabled Scale_windows on {window.name}");
            }
        }
         foreach (GameObject window in allWindows)
        {
            Move_around_windows Mover = window.GetComponent<Move_around_windows>();
            if (Mover != null)
            {
                Destroy(Mover);
                //Debug.Log($"Disabled Scale_windows on {window.name}");
            }
        }

       // Debug.Log($"Processed {allWindows.Length} window(s).");
    }
}
