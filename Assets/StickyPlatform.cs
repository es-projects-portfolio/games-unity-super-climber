using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyPlatform : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) 
    {
        //this doesn't work
        Debug.Log("player collides with platform");    
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hello Trigger");
        other.transform.parent = transform;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Goodbye Trigger");
        other.transform.parent = null;
    }
}
