using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ChangeSScene : MonoBehaviour
{
    // Start is called before the first frame update
    public string SceneName = "";

    public void LoadTargetScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}
