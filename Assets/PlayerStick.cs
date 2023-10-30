using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStick : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.tag == "Moving Platform")
        {
            transform.parent = other.transform;
            Debug.Log("test test");
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.tag == "Moving Platform")
        {
            transform.parent = null;
        }
    }
}
