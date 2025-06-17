using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ProportionalOrManualScaling" , menuName = "ScriptableObjects/ScalingMode", order = 0)]
public class ProportionalOrManualScaling : ScriptableObject
{
    [SerializeField] public bool ProportionalScalingWindow = false;
    [SerializeField] public bool ManualScalingWindow = false;


    public void ProportionalWindowScaling(bool value)
    {
        ProportionalScalingWindow = value;
    }
    public void ManualWindowScaling(bool value)
    {
        ManualScalingWindow = value;
    }

}
