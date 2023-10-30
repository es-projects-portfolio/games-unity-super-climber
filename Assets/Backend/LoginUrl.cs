using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginUrl : MonoBehaviour
{
    // Start is called before the first frame update
    public void OpenURL()
    {
        // Open the URL
        Application.OpenURL("https://games.metaxar.io/login");
    }
}
