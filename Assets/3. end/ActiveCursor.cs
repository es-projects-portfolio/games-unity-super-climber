using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCursor : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
