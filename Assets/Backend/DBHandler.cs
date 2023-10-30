using System;
using System.Net.Http;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using System.Text;
using System.Collections.Generic;

namespace Backend.Database
{
    public class DBHandler : MonoBehaviour
    {
        public InternalDB idb;
        public Blockchain bc;

        public string dbUrl_testnet = "https://metx-games-api-demo.vercel.app/api/v1";
        public string dbUrl_mainnet = "https://metx-superclimber.vercel.app/api/v1";
        public string gameId = "D16E060D72E12794";
        public string gameId_autosign = "D16E060D72E12794";


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
                }
                else if (ds < idb.limitDailyScore())
                {
                    idb.maxScore.SetActive(false);
                }

                int ts = jsonData["b_Score"]["d_TotalScore"];
                Debug.Log("TS = " + ts);
                PlayerPrefs.SetInt("TotalScore", ts);
                
                idb.TotalTokenText.text = ts + " " + bc.tokenId;
                idb.TotalScoreText.text = ts + "";

                int tr = jsonData["c_TokensReq"]["a_TokensReq"];
                Debug.Log("TR = " + tr);
                PlayerPrefs.SetInt("TokensReq", tr);

                int tc = jsonData["d_TokensClaim"]["g_TokensClaimed"];
                Debug.Log("TC = " + tc);
                PlayerPrefs.SetInt("TokensClaim", tc);

                string txnHash = jsonData["c_TokensReq"]["b_TxnHash"];

                GameStartCondition(txnHash, ts, tr, tc);
            }
        }


        private void GameStartCondition(string hash, int ts, int tr, int tc)
        {

            if (ts == 0 || ts == null) //total score must be more than 0 to allowusers to click
            {
                idb.claimButton.SetActive(false);
                hashFunction(hash);
            }
            else
            {
                hashFunction(hash);
            }

        }

        private void hashFunction(string hash)
        {

            if (hash == null || hash == "")
            {
                idb.claimButton.SetActive(true);
            }
            else if (hash != null && hash != "")
            {
                idb.claimButton.SetActive(false);
                string node = PlayerPrefs.GetString("node");

                if (node == "testnet")
                {
                    StartCoroutine(GetTransactionStatus(dbUrl_testnet, gameId, idb.nama(), bc.baseUrl_testnet, hash, idb.bearerTestnet()));
                }
                else if (node == "mainnet")
                {
                    StartCoroutine(GetTransactionStatus(dbUrl_mainnet, gameId, idb.nama(), bc.baseUrl_mainnet, hash, idb.bearerMainnet()));
                }

            }
        }

        //Fromthe autosigner api url
        private IEnumerator GetTransactionStatus(string dbUrl, string gameId, string userId, string autosignerUrl, string hash, string bearer)
        {
            bool status = false;
            var cert = new ForceAcceptAll();
            UnityWebRequest request = UnityWebRequest.Get($"{autosignerUrl}/api/v1/transactions/{hash}");
            request.SetRequestHeader("Authorization", "Bearer " + bearer);
            request.certificateHandler = cert;
            cert?.Dispose();
            Debug.Log("Checking tx status before ..");

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error :( . Autosigner problem");
                // onErrorCallback(request.result);
                Debug.LogError(request.error, this);
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                status = jsonData["success"];
                if (status == true)
                {
                    Debug.Log("Tokens claimed :)");
                    //PUT tokens claim succes api
                    StartCoroutine(PUTTokensClaimSuccess(dbUrl, gameId, userId, autosignerUrl, bearer));
                }
                else if (status == false)
                {
                    Debug.Log("Pending txn, or txn fail");
                    //PUT tokens claim fail api
                    StartCoroutine(PUTTokensClaimFail(dbUrl, gameId, userId));

                }
            }
        }

        private IEnumerator PUTTokensClaimSuccess(string dbUrl, string gameId, string userId, string autosignerUrl, string bearer_autosign)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{userId}/tokensclaim/success";
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
                Debug.Log("Tokens claimed success updated");
                string jsonResponse = request.downloadHandler.text;
                var json = JSON.Parse(jsonResponse);
                int ts = json["data"]["totalScore"];
                int tc = json["data"]["tokensClaimedThisTime"];
                idb.TotalTokenText.text = ts + " " + bc.tokenId;
                Debug.Log("tc: " + tc);

                StartCoroutine(PostEventScore(autosignerUrl, bearer_autosign, tc));
            }
        }

        private IEnumerator PUTTokensClaimFail(string dbUrl, string gameId, string userId)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{userId}/tokensclaim/fail";
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
                Debug.Log("Tokens claimed fail updated");
                string jsonResponse = request.downloadHandler.text;
                var json = JSON.Parse(jsonResponse);
                int ts = json["data"]["totalScore"];
                idb.TotalTokenText.text = ts + " " + bc.tokenId;
            }
        }

        private IEnumerator PostEventScore(string autosignerUrl, string bearer, int tokensClaimed)
        {
            int amount = tokensClaimed;
            var cert = new ForceAcceptAll();
            string url = $"{autosignerUrl}/api/v1/events/score?score={amount}&auth={idb.auth()}&gameId={gameId_autosign}";

            UnityWebRequest request = UnityWebRequest.Post(url, new WWWForm());

            request.SetRequestHeader("Authorization", "Bearer " + bearer);

            request.certificateHandler = cert;

            // Send
            cert?.Dispose();

            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log("Error post event score :(");
                // onErrorCallback(request.result);
                Debug.LogError(request.error, this);
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                Debug.Log(jsonData.ToString());
            }
        }

        /*private void hashFunction(string hash)
        {

            if (hash == null || hash == "")
            {
                idb.claimButton.SetActive(true);
            }
            else if (hash != null && hash != "")
            {
                idb.claimButton.SetActive(false);
                StartCoroutine(GetCheckClaimStatusTestnet(dbUrl_testnet, gameId, idb.nama(), bc.baseUrl_mainnet, bc.baseUrl_testnet));
            }
        }


        public IEnumerator GetCheckClaimStatusTestnet(string dbUrl, string gameId, string userId, string autosignerUrl1, string autosignerUrl2)
        {
            var cert = new ForceAcceptAll();

            // Create the JSON object with required properties
            JSONObject requestBodyJson = new JSONObject();
            requestBodyJson.Add("bearerToken", idb.bearerTestnet());

            // Convert JSON to bytes for the request body
            byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBodyJson.ToString());


            UnityWebRequest request = UnityWebRequest.Get($"{dbUrl}/{gameId}/{userId}/claimstatus?autosigner_url={autosignerUrl2}");
            request.uploadHandler = new UploadHandlerRaw(requestBodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();
            

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                idb.claimButton.SetActive(false);
                Debug.LogError(request.error);
                StartCoroutine(GetCheckClaimStatusMainnet(dbUrl, gameId, userId, autosignerUrl1));
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                string message = jsonData["message"];
                Debug.Log(message);
                GetJSON();
            }
        }

        public IEnumerator GetCheckClaimStatusMainnet(string dbUrl, string gameId, string userId, string autosignerUrl1)
        {
            var cert = new ForceAcceptAll();

            // Create the JSON object with required properties
            JSONObject requestBodyJson = new JSONObject();
            requestBodyJson.Add("bearerToken", idb.bearerMainnet());

            // Convert JSON to bytes for the request body
            byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBodyJson.ToString());


            UnityWebRequest request = UnityWebRequest.Get($"{dbUrl}/{gameId}/{userId}/claimstatus?autosigner_url={autosignerUrl1}");
            request.uploadHandler = new UploadHandlerRaw(requestBodyBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + idb.jwt());
            request.certificateHandler = cert;
            cert?.Dispose();

            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                var jsonData = JSON.Parse(request.downloadHandler.text);
                string message = jsonData["message"];
                Debug.Log(jsonData);
                GetJSON();
            }
        }*/


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
                    /*idb.TotalScoreText.text = ts + "";
                    idb.TotalTokenText.text = ts + " " + bc.tokenId;*/
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
            string url = $"{dbUrl}/{gameId}/login";

            // Create the JSON object with required properties
            JSONObject requestBodyJson = new JSONObject();
            requestBodyJson.Add("tokenID", bc.tokenId);
            requestBodyJson.Add("user", idb.nama());

            // Convert JSON to bytes for the request body
            byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBodyJson.ToString());

            UnityWebRequest request = UnityWebRequest.Put(url, "");
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.uploadHandler = new UploadHandlerRaw(requestBodyBytes);
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


        public void PostDailyScore(int score)
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(newDailyScore(dbUrl_testnet, score));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newDailyScore(dbUrl_mainnet, score));
            }
        }
        private IEnumerator newDailyScore(string dbUrl, int score)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/dailyscore?score={score}&limit={idb.limitDailyScore()}";
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
                string jsonResponse = request.downloadHandler.text;
                var json = JSON.Parse(jsonResponse);
                int ts = json["data"]["totalScore"];
                idb.TotalTokenText.text = ts + " " + bc.tokenId;
                PlayerPrefs.SetInt("TotalScore", ts);
            }
        }


        public void PostTokensReq(string hash)
        {
            if (idb.node() == "testnet")
            {
                StartCoroutine(newTokensRequest(dbUrl_testnet, hash));
            }
            else if (idb.node() == "mainnet")
            {
                StartCoroutine(newTokensRequest(dbUrl_mainnet, hash));
            }
        }
        private IEnumerator newTokensRequest(string dbUrl, string hash)
        {
            var cert = new ForceAcceptAll();
            string url = $"{dbUrl}/{gameId}/{idb.nama()}/tokensreq?hash={hash}";
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


    }
}

