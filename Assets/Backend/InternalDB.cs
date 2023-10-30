using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using Backend.Database;

namespace Backend
{
    public class InternalDB : MonoBehaviour
    {
        public DBHandler dbh;
        public GameObject claimButton;
        /*public TMP_Text usernameText;*/
        public Text TotalScoreText;
        public TMP_Text TotalTokenText;
        public GameObject maxScore;

        public string auth()
        {
            string a = PlayerPrefs.GetString("Auth");
            return a;
        }

        public string node()
        {
            string n = PlayerPrefs.GetString("node");
            return n;
        }

        public string bearerTestnet()
        {
            string b = PlayerPrefs.GetString("bearerTestnet");
            return b;
        }

        public string bearerMainnet()
        {
            string b = PlayerPrefs.GetString("bearerMainnet");
            return b;
        }

        public string jwt()
        {
            string j = PlayerPrefs.GetString("jwt");
            return j;
        }

        public string nama()
        {
            string n = PlayerPrefs.GetString("username");
            return n;
        }

        public string token_id()
        {
            string token = PlayerPrefs.GetString("token_id");
            return token;
        }

        public int CurrentScore()
        {
            int cs = PlayerPrefs.GetInt("CurrentScore");
            return cs;
        }

        public int limitDailyScore()
        {
            int limit = PlayerPrefs.GetInt("limit");
            return limit;
        }

        public int DailyScore()
        {
            int ds = PlayerPrefs.GetInt("DailyScore");
            return ds;
        }

        public int TotalScore()
        {
            int ts = PlayerPrefs.GetInt("TotalScore");
            return ts;
        }

        public int TokensRequested()
        {
            int tr = PlayerPrefs.GetInt("TokensReqested");
            return tr;
        }

        public int TokensClaimed()
        {
            int tc = PlayerPrefs.GetInt("TokensClaimed");
            return tc;
        }

        public void GameStart()
        {
            string username = nama();
            dbh.Login();
            Debug.Log("user in db: " + username);
            Debug.Log("token id: " + token_id());

        }
    }

}
