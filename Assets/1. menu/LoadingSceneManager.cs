using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] private Slider loadingBar;
    private float loadingTime = 10f;

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float elapsedTime = 0f;

        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / loadingTime);
            loadingBar.value = progress;
            yield return null;
        }

        SceneManager.LoadScene("Menu");
    }
}
