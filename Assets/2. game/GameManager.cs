using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private FallDamage fallDamage;
    public MonoBehaviour cameraController;

    public Text coinText;

    public float currentTime = 0f;
    public float startingTime = 30f;

    [SerializeField] TMP_Text countdownText;
    [SerializeField] AudioClip countdownSound;
    [SerializeField] AudioClip coinCollectionSound;

    public GameObject QuitPanel;

    private AudioSource audioSource;
    private bool isShaking = false;
    private Vector3 originalTextPosition;
    private Coroutine countdownSoundCoroutine;
    private bool isPaused = false;

    void Start()
    {
        currentTime = startingTime;
        originalTextPosition = countdownText.transform.localPosition;
        audioSource = GetComponent<AudioSource>();

        // Find the player GameObject and get its FallDamage component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            fallDamage = player.GetComponent<FallDamage>();
        }
    }

    void Update()
    {
        if (fallDamage != null && fallDamage.IsDead) return;

        if (!isPaused)
        {
            currentTime -= 1 * Time.deltaTime;
            countdownText.text = currentTime.ToString("0");
        }

        if (currentTime <= 10)
        {
            countdownText.color = Color.red;
            countdownText.fontSize = 140;

            if (countdownSoundCoroutine == null)
            {
                countdownSoundCoroutine = StartCoroutine(PlayCountdownSoundSynced());
            }

            if (currentTime <= 5)
            {
                countdownText.color = Color.red;
                countdownText.fontSize = 200;

                if (!isShaking)
                {
                    StartCoroutine(ShakeText());
                }
            }
        }
        else
        {
            countdownText.color = Color.white;
            countdownText.fontSize = 72;
            StopCoroutine(ShakeText());
            countdownText.transform.localPosition = originalTextPosition;
            isShaking = false;
            StopCountdownSound();
        }

        if (currentTime <= 0)
        {
            currentTime = 0;
        }

        if (currentTime == 0)
        {
            SceneManager.LoadScene("End");
        }

        // Handle Quit Panel
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            QuitPanel.SetActive(!QuitPanel.gameObject.activeSelf);

            if (QuitPanel.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Time.timeScale = 0f;
                isPaused = true;

                // Disable the camera script
                if (cameraController != null)
                {
                    cameraController.enabled = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                Time.timeScale = 1f;
                isPaused = false;

                // Enable the camera script
                if (cameraController != null)
                {
                    cameraController.enabled = true;
                }
            }
        }
    }

    public void Unpause()
    {
        if (QuitPanel.activeSelf)
        {
            QuitPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
            isPaused = false;

            // Enable the camera script
            if (cameraController != null)
            {
                cameraController.enabled = true;
            }
        }
    }

    IEnumerator ShakeText()
    {
        isShaking = true;
        float shakeDuration = 0.1f;
        float shakeMagnitude = 5f;

        while (isShaking)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-shakeMagnitude, shakeMagnitude), Random.Range(-shakeMagnitude, shakeMagnitude), 0);
            countdownText.transform.localPosition = originalTextPosition + randomOffset;

            yield return new WaitForSeconds(shakeDuration);
        }

        countdownText.transform.localPosition = originalTextPosition;
    }

    IEnumerator PlayCountdownSoundSynced()
    {
        while (currentTime <= 10)
        {
            audioSource.PlayOneShot(countdownSound);
            yield return new WaitForSeconds(1);
        }
    }

        private void StopCountdownSound()
    {
        if (countdownSoundCoroutine != null)
        {
            StopCoroutine(countdownSoundCoroutine);
            countdownSoundCoroutine = null;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);

            currentTime += startingTime;

            int coin = PlayerPrefs.GetInt("Coin");
            coin = coin + 1;
            PlayerPrefs.SetInt("Coin", coin);
            int pointOut = PlayerPrefs.GetInt("Coin");

            Debug.Log("POINTS in PP : " + pointOut);
            coinText.text = pointOut + "";

            audioSource.PlayOneShot(coinCollectionSound);
        }
    }
}

