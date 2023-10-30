using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLife : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Die();
            Debug.Log("Death");
        }
    }

    private void Die() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
