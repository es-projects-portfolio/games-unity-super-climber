using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;


public class FallDamage : MonoBehaviour
{
    public Text healthText;
    public float fallThreshold = 5f;
    public int maxFallDamage = 50;
    public int health = 100;
    public Slider healthBar;
    [SerializeField] public GameObject damagePanel;
    [SerializeField] public GameObject deadPanel;

    private CharacterController characterController;
    private float initialY;
    private bool isFalling;

    public bool IsDead { get; private set; } = false;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        healthBar.value = health / 100f;
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            if (isFalling)
            {
                float fallDistance = initialY - transform.position.y;
                if (fallDistance >= fallThreshold)
                {
                    int damage = CalculateFallDamage(fallDistance);
                    ApplyDamage(damage);
                }

                isFalling = false;
            }
            initialY = transform.position.y;
        }
        else
        {
            isFalling = true;
        }
    }

    int CalculateFallDamage(float fallDistance)
    {
        float fallPercentage = (fallDistance - fallThreshold) / (fallThreshold * 2f);
        fallPercentage = Mathf.Clamp(fallPercentage, 0f, 1f);
        return Mathf.RoundToInt(fallPercentage * maxFallDamage);
    }

    void ApplyDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);
        healthBar.value = health / 100f;
        damagePanel.SetActive(true);
        StartCoroutine(DisableDamagePanel());
        Debug.Log($"Fall damage: {damage}. Current health: {health}");
        healthText.text = health + "%";

        if (health <= 0)
        {
            // Handle player death here.
            Debug.Log("Player has died due to fall damage.");
            deadPanel.SetActive(true);

            // Disable player movement and character controller
            characterController.enabled = false;
            GetComponent<FirstPersonController>().enabled = false;

            IsDead = true;

            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator DisableDamagePanel()
    {
        yield return new WaitForSeconds(3f); // Use 'yield return' instead of 'WaitForSeconds'
        damagePanel.SetActive(false);
    }

    private IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(3f); // Use 'yield return' instead of 'WaitForSeconds'
        SceneManager.LoadScene("End");
    }
}
