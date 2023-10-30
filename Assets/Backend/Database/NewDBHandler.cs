using System;
using System.Net.Http;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using Backend.Database;
using System.Text;

namespace Backend.Database
{
    public class NewDBHandler : MonoBehaviour
    {
        public NewInternalDB idb;
        public NewBlockchain bc;

        public string dbUrl_testnet = "https://metx-demo.vercel.app/api/v1";
        public string dbUrl_mainnet = "https://metx-superclimber.vercel.app/api/v1";
        public string gameId = "D16E060D72E12794";


        public TMP_Text GetDataText;

        int sec = System.DateTime.Now.Second;
        int min = System.DateTime.Now.Minute;
        int hour = System.DateTime.Now.Hour;
        int year = System.DateTime.Now.Year;
        int month = System.DateTime.Now.Month;
        int day = System.DateTime.Now.Day;

        public class ForceAcceptAll : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        //Get
        public void Bearer()
        {
            BearerTestnet();
            BearerMainnet();
            Debug.Log("Received Bearer");
        }

        private void BearerTestnet() =>
            StartCoroutine(GetBearerTestnet());
        private IEnumerator GetBearerTestnet()
        {
            var cert = new ForceAcceptAll();
            UnityWebRequest request = UnityWebRequest.Get($"{dbUrl_testnet}/~ metx-secure");
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error get bearer");
                // onErrorCallback(request.result);
                Debug.LogError(request.error, this);
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                string bearer = jsonData["bearerToken"];
                PlayerPrefs.SetString("bearerTestnet", bearer);
            }
        }

        private void BearerMainnet() =>
            StartCoroutine(GetBearerMainnet());
        private IEnumerator GetBearerMainnet()
        {
            var cert = new ForceAcceptAll();
            UnityWebRequest request = UnityWebRequest.Get($"{dbUrl_mainnet}/~ metx-secure");
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error get bearer");
                // onErrorCallback(request.result);
                Debug.LogError(request.error, this);
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                string bearer = jsonData["bearerToken"];
                PlayerPrefs.SetString("bearerMainnet", bearer);
            }
        }


        public void GetJSON()
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(GetAllData(dbUrl_testnet));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(GetAllData(dbUrl_mainnet));
                if (idb.TokensRequested() == 0 || idb.TokensClaimed() == 0)
                {
                    if (idb.TotalScore() == 0)
                    {
                        StartCoroutine(GetDataIfNotSignUp());
                        StartCoroutine(GetAllData(dbUrl_mainnet));
                    }
                }
            }
        }
        private IEnumerator GetAllData(string dbUrl)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("User retrieved successfully");
                // Do something with the response
                var jsonData = JSON.Parse(request.downloadHandler.text);

                int ds = jsonData["b_Score"]["a_DailyScore"];
                Debug.Log("DS = " + ds);
                PlayerPrefs.SetInt("DailyScore", ds);
                if (ds >= idb.limitDailyScore())
                {
                    idb.maxScore.SetActive(true);
                } else if (ds < idb.limitDailyScore())
                {
                    idb.maxScore.SetActive(false);
                }
                
                int ts = jsonData["b_Score"]["d_TotalScore"];
                Debug.Log("TS = " + ts);
                PlayerPrefs.SetInt("TotalScore", ts);
                idb.TotalScoreText.text = ts + "";
                idb.TotalTokenText.text = ts + " " + bc.tokenId;

                int tr = jsonData["c_TokensReq"]["a_TokensReq"];
                Debug.Log("TR = " + tr);
                PlayerPrefs.SetInt("TokensReq", tr);

                int tc = jsonData["d_TokensClaim"]["g_TokensClaimed"];
                Debug.Log("TC = " + tc);
                PlayerPrefs.SetInt("TokensClaim", tc);

                string txnHash = jsonData["c_TokensReq"]["b_TxnHash"];
                int prevTimeStamp = jsonData["c_TokensReq"]["d_TimeStamp"];

                GameStartCondition(txnHash, ts, tr, tc, prevTimeStamp);
            }
        }


        private void GameStartCondition(string hash, int ts, int tr, int tc, int pts)
        {

                if (ts == 0 || ts == null) //total score must be more than 0 to allowusers to click
                {
                    idb.claimButton.SetActive(false);
                    hashFunction(ts, tr, hash, tc, pts);
                }
                else
                {
                    hashFunction(ts, tr, hash, tc, pts);
                }
            
        }

        private void hashFunction(int ts, int tr, string hash, int tc, int pts)
        {
            int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
            int prevTimeStamp = 5 + pts;

            if (hash == null || hash == "")
            {
                idb.claimButton.SetActive(true);
            }
            else if (hash != null && hash != "")
            {
                idb.claimButton.SetActive(false);
                if (currentTimeStamp > prevTimeStamp)
                {
                    bc.voidTxnStatusOT(ts, tr, hash, tc);
                }
                else if (currentTimeStamp < prevTimeStamp)
                {
                    bc.voidTxnStatusBT(ts, tr, hash, tc);
                }
            }
        }

        private IEnumerator GetDataIfNotSignUp()
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl_testnet}/{gameId}/{idb.nama()}";
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("User retrieved successfully");
                // Do something with the response
                var jsonData = JSON.Parse(request.downloadHandler.text);
                int tr = jsonData["c_TokensReq"]["a_TokensReq"];
                int tc = jsonData["d_TokensClaim"]["g_TokensClaimed"];
                if (tr == 0 && tc == 0)
                {
                    int ts = jsonData["b_Score"]["d_TotalScore"];
                    Debug.Log("TS = " + ts);
                    PlayerPrefs.SetInt("TotalScore", ts);
                    idb.TotalScoreText.text = ts + "";
                    idb.TotalTokenText.text = ts + " " + bc.tokenId;
                    PostTotalScore();
                }
                else
                {
                    ;
                }
            }
        }


        //Post

        public void Login()
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(PostLogin(dbUrl_testnet));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(PostLogin(dbUrl_mainnet));
            }

        }
        private IEnumerator PostLogin(string dbUrl)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/login?username={idb.nama()}&tokenID={bc.tokenId}";
            UnityWebRequest request = UnityWebRequest.Put(url, "");
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(idb.nama()));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Login Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("Login successful");
                // Retrieve the JWT from the response
                string jsonResponse = request.downloadHandler.text;
                var json = JSON.Parse(jsonResponse);
                string jwt = json["viewModel"]["token"];
                PlayerPrefs.SetString("jwt", jwt);
                GetJSON();
            }
        }

        public void PostTotalScore()
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(newTotalScore(dbUrl_testnet));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newTotalScore(dbUrl_mainnet));
            }
        }
        private IEnumerator newTotalScore(string dbUrl)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/totalscore?score={idb.TotalScore()}";
            UnityWebRequest request = UnityWebRequest.Put(url, "");
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("Score posted successfully");
                // Do something with the response
                idb.TotalScoreText.text = idb.TotalScore() + "";
                idb.TotalTokenText.text = idb.TotalScore() + " " + bc.tokenId;
            }
        }


        public void PostDailyScore()
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(newDailyScore(dbUrl_testnet));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newDailyScore(dbUrl_mainnet));
            }
        }
        private IEnumerator newDailyScore(string dbUrl)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/dailyscore?score={idb.DailyScore()}&limit={idb.limitDailyScore()}";
            UnityWebRequest request = UnityWebRequest.Put(url, "");
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("Score posted successfully");
                // Do something with the response
                idb.TotalScoreText.text = idb.TotalScore() + "";
                idb.TotalTokenText.text = idb.TotalScore() + " " + bc.tokenId;
            }
        }


        public void PostTokensReq(int amount, string hash)
        {
            int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
            if (idb.node() == "testnet")
            {
                StartCoroutine(newTokensRequest(dbUrl_testnet, amount, hash, currentTimeStamp));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newTokensRequest(dbUrl_mainnet, amount, hash, currentTimeStamp));
            }
        }
        private IEnumerator newTokensRequest(string dbUrl, int amount, string hash, int timestamp)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/tokensreq?amount={amount}&hash={hash}&timestamp={timestamp}";
            UnityWebRequest request = UnityWebRequest.Put(url, new byte[0]);
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("Tokens requested successfully");
            }
        }


        public void PostTokensClaim(int amount)
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(newTokensClaim(dbUrl_testnet, amount));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newTokensClaim(dbUrl_mainnet, amount));
            }
            PlayerPrefs.SetInt("TokensRequested", 0);
            PostTokensReq(0, null);
        }
        private IEnumerator newTokensClaim(string dbUrl, int amount)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/tokensclaim?amount={amount}";
            UnityWebRequest request = UnityWebRequest.Put(url, new byte[0]);
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :(");
                Debug.LogError(request.error, this);
            }
            else
            {
                Debug.Log("Tokens claimed successfully");
            }
        }



    }
}

