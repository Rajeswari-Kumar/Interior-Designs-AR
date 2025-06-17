using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Delete_Interior_component : MonoBehaviour
{
    public void Delete_component()
    {
        Destroy(transform.gameObject);
    }

}
