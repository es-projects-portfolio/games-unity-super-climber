using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CountdownTimer : MonoBehaviour
{
    public float currentTime = 0f;
    public float startingTime = 60f;

    [SerializeField] TMP_Text countdownText;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = startingTime;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime -= 1 * Time.deltaTime;
        countdownText.text = currentTime.ToString("0");
        

        if (currentTime <= 0)
        {
            currentTime = 0;
        }

    }
}
