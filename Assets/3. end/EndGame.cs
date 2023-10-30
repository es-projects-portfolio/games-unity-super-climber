using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Backend.Database;

public class EndGame : MonoBehaviour
{
    public NewDBHandler dbh;
    public NewInternalDB idb;

    public Time time;
    public TMP_Text CurrentScoreText;
    public int currentScore = 0;

    public int DailyScore;

    private void Awake()
    {   
        currentScore = PlayerPrefs.GetInt("Coin");
        CurrentScoreText.text = "" + currentScore;
    }


    //This will happen when the user proceed to main menu or play again scene
    //Call CurrentScore (PlayerPrefs) to add with TotalScore (Db)
    //TotalScore = TotalScore + CurrentScore
    public void NextScene()
    {
        currentScore = PlayerPrefs.GetInt("Coin");
        name = PlayerPrefs.GetString("username");

        DailyScore = DailyScoreDB(currentScore);

        dbh.PostDailyScore();
        Debug.Log("Daily Score = " + DailyScore);
        Debug.Log("Current score resets. Current score = " + CurrentScoreReset());

    }


    public int DailyScoreDB(int cs)
    {
        int ScoreFromDB = PlayerPrefs.GetInt("DailyScore");
        int finalScore = ScoreFromDB + cs;
        PlayerPrefs.SetInt("DailyScore", finalScore);
        return finalScore;   
    }

    public int CurrentScoreReset()
    {
        PlayerPrefs.SetInt("Coin", 0);
        currentScore = PlayerPrefs.GetInt("Coin");
        return currentScore;
    }
}
