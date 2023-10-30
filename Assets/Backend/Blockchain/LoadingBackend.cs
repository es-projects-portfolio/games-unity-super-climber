using System;
using System.Net.Http;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using Backend.Database;
using System.Text;
using UnityEngine.UI;

public class LoadingBackend : MonoBehaviour
{
    public static LoadingBackend Instance { get; private set; }

    public string baseUrl_testnet = "https://xar-autosigner-2.proximaxtest.com";
    public string baseUrl_mainnet = "https://xar-autosigner.proximaxtest.com";
    public string dbUrl_testnet = "https://metx-demo.vercel.app/api/v1";
    public string dbUrl_mainnet = "https://metx-superclimber.vercel.app/api/v1";

    public HttpClient client = new HttpClient();
    public string gameId = "D16E060D72E12794";
    public string auth;
    public string username;
    public string tokenId;

    public Text usernameText;

    int sec = System.DateTime.Now.Second;
    int min = System.DateTime.Now.Minute;
    int hour = System.DateTime.Now.Hour;
    int year = System.DateTime.Now.Year;
    int month = System.DateTime.Now.Month;
    int day = System.DateTime.Now.Day;

    /*--------- CACHE VARIABLE------- */

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

    public int TotalScore()
    {
        int ts = PlayerPrefs.GetInt("TotalScore");
        return ts;
    }

    public class ForceAcceptAll : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public void Start(){
        PlayerPrefs.SetString("auth", auth);
        Bearer();
        Time.timeScale = 1;
        DontDestroyOnLoad(this.gameObject);
        client = new HttpClient();
        CallAuth();
    }

    //Get Bearer Token
    public void Bearer(){
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

    private void CallAuth()
    {
#if UNITY_WEBGL
        GetAuthFromWebGL();
#endif

#if UNITY_EDITOR
        StartCoroutine(GetUserTestnet(() =>
        {
            GetDBToken();
        }));
#endif
    }

    public void GetAuthFromWebGL()
    {
        int pm = Application.absoluteURL.IndexOf("?");
        if (pm != -1)
        {
            auth = Application.absoluteURL.Split("?"[0])[1].Split("=")[1];
            PlayerPrefs.SetString("auth", auth);
            Debug.Log("new user: " + auth);
        }
    }

    public void GetDBToken()
    {

    }

    public IEnumerator GetUserTestnet(Action callback)
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_testnet}/api/v1/users/{auth}");
        request.SetRequestHeader("Authorization", "Bearer " + bearerTestnet());
        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Login Testnet Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
            StartCoroutine(GetUserMainnet());
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            if (this.username == null || this.username == "")
            {
                StartCoroutine(GetUserMainnet());
            }
            else
            {
                PlayerPrefs.SetString("username", username);
                Debug.Log("User retrieved. Username: " + username);
                PlayerPrefs.SetString("node", "testnet");
                Debug.Log("Node: " + node());
                usernameText.text = "" + username;
                Login();
            }
            callback.Invoke();
        }
    }

    public IEnumerator GetUserMainnet()
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_mainnet}/api/v1/users/{auth}");
        request.SetRequestHeader("Authorization", "Bearer " + bearerMainnet());
        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Login Mainnet Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
            PlayerPrefs.SetString("username", "OFFLINE");
            usernameText.text = "OFFLINE";
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            if (this.username == null || this.username == "")
            {
                PlayerPrefs.SetString("username", "OFFLINE");
                Debug.Log("User retrieved. Username: OFFLINE");
                usernameText.text = "OFFLINE";
            }
            else
            {
                PlayerPrefs.SetString("username", username);
                Debug.Log("User retrieved. Username: " + username);
                PlayerPrefs.SetString("node", "mainnet");
                usernameText.text = "" + username;
                Debug.Log("Node: " + node());
                Login();
            }
        }
    }

    private void Login()
    {
        if (node() == "testnet"){
            StartCoroutine(PostLogin(dbUrl_testnet));
        }
        else if (node() == "mainnet"){
            StartCoroutine(PostLogin(dbUrl_mainnet));
        }
        
    }
    private IEnumerator PostLogin(string dbUrl)
    {
        var cert = new ForceAcceptAll();
        string url = $"{dbUrl}/{gameId}/{username}/login?username={username}&tokenID={tokenId}";
        UnityWebRequest request = UnityWebRequest.Put(url, "");
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(username));
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
            /* GetJSON(); */
        }
    }

    public void GetJSON() {
        if (node() == "testnet"){
            StartCoroutine(GetAllData(dbUrl_testnet));
        }
        else if (node() == "mainnet"){
            StartCoroutine(GetAllData(dbUrl_mainnet));
        }
    }
    private IEnumerator GetAllData(string dbUrl)
    {
        var cert = new ForceAcceptAll();
        string url = $"{dbUrl}/{gameId}/{username}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Bearer " + jwt());
        request.certificateHandler = cert;
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error Get Data :(");
            Debug.LogError(request.error, this);
        }
        else
        {
            Debug.Log("User retrieved successfully");
            // Do something with the response
            var jsonData = JSON.Parse(request.downloadHandler.text);
            int ts = jsonData["b_Score"]["d_TotalScore"];
            Debug.Log("TS = " + ts);
            PlayerPrefs.SetInt("TotalScore", ts);

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
        if (ts == 0) //total score must be more than 0 to allowusers to click
        {
            hashFunction(ts, tr, hash, tc, pts);
        }
        else if (ts >= 2000)
        {
            ts = 2000;
            PlayerPrefs.SetInt("TotalScore", ts);
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

        if (hash != null || hash != "")
        {
            if (currentTimeStamp > prevTimeStamp)
            {
                voidTxnStatusOT(ts, tr, hash, tc);
            }
            else if (currentTimeStamp < prevTimeStamp)
            {
                voidTxnStatusBT(ts, tr, hash, tc);
            }
        }
        
    }



    /* ------BLOCKCHAIN--------- */

    private IEnumerator txnStatusOT(string baseUrl, string bearer, int ts, int tr, string hash, int tc) //more than 5 mins
    {
        bool status = false;
        var cert = new ForceAcceptAll();
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/api/v1/transactions/{hash}");
        request.SetRequestHeader("Authorization", "Bearer " + bearer);
        request.certificateHandler = cert;
        cert?.Dispose();
        Debug.Log("Checking tx status before ..");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error :(");
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
                ClaimingToken(tr, tc);
            }
            else if (status == false)
            {
                Debug.Log("Pending txn, or txn fail");
                int newts = ts + tr;
                PlayerPrefs.SetInt("TotalScore", newts);
                PostTotalScore();
                PlayerPrefs.SetInt("TokensRequested", 0);
                PostTokensReq(0, null);
            }
        }
    }

    public void voidTxnStatusOT(int ts, int tr, string hash, int tc)
    {
        if (node() == "testnet"){
            StartCoroutine(txnStatusOT(baseUrl_testnet, bearerTestnet(), ts, tr, hash, tc));
        }
        else if (node() == "mainnet"){
            StartCoroutine(txnStatusOT(baseUrl_mainnet, bearerMainnet(), ts, tr, hash, tc));
        }
        
    }

    private IEnumerator txnStatusBT(string baseUrl, string bearer, int ts, int tr, string hash, int tc) //less than 5 mins
    {
        bool status = false;
        var cert = new ForceAcceptAll();
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/api/v1/transactions/{hash}");
        request.SetRequestHeader("Authorization", "Bearer " + bearer);
        request.certificateHandler = cert;
        cert?.Dispose();
        Debug.Log("Checking tx status before ..");

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error :(");
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
                ClaimingToken(tr, tc);
            }
            else if (status == false)
            {
                Debug.Log("Pending txn");
            }
        }
    }

    public void voidTxnStatusBT(int ts, int tr, string hash, int tc)
    {
        if (node() == "testnet"){
            StartCoroutine(txnStatusBT(baseUrl_testnet, bearerTestnet(), ts, tr, hash, tc));
        }
        else if (node() == "mainnet"){
            StartCoroutine(txnStatusBT(baseUrl_mainnet, bearerMainnet(), ts, tr, hash, tc));
        }
    }

    private void ClaimingToken(int tr, int tc)
    {
        Debug.Log("Processing in db ...");
        PlayerPrefs.SetInt("TokensClaimed", tc + tr);
        tc = tc + tr;
        PostTokensClaim(tc);
    }

    /* ---------DATABASE----------- */

    public void PostTotalScore(){
        if (node() == "testnet"){
            StartCoroutine(newTotalScore(dbUrl_testnet));
        }
        else if (node() == "mainnet"){
            StartCoroutine(newTotalScore(dbUrl_mainnet));
        }
    }
    private IEnumerator newTotalScore(string dbUrl)
    {
        var cert = new ForceAcceptAll();
        string url = $"{dbUrl}/{gameId}/{username}/totalscore?score={TotalScore()}";
        UnityWebRequest request = UnityWebRequest.Put(url, "");
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + jwt());
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
        }
    }


    public void PostTokensReq(int amount, string hash)
    {
        int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
        if (node() == "testnet"){
            StartCoroutine(newTokensRequest(dbUrl_testnet, amount, hash, currentTimeStamp));
        }
        else if (node() == "mainnet"){
            StartCoroutine(newTokensRequest(dbUrl_mainnet, amount, hash, currentTimeStamp));
        }
        PostTotalScore();
    }
    private IEnumerator newTokensRequest(string dbUrl, int amount, string hash, int timestamp)
    {
        var cert = new ForceAcceptAll();
        string url = $"{dbUrl}/{gameId}/{username}/tokensreq?amount={amount}&hash={hash}&timestamp={timestamp}";
        UnityWebRequest request = UnityWebRequest.Put(url, new byte[0]);
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.SetRequestHeader("Authorization", "Bearer " + jwt());
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
        if (node() == "testnet"){
            StartCoroutine(newTokensClaim(dbUrl_testnet, amount));
        }
        else if (node() == "mainnet"){
            StartCoroutine(newTokensClaim(dbUrl_mainnet, amount));
        }
        PlayerPrefs.SetInt("TokensRequested", 0);
        PostTokensReq(0, null);
    }
    private IEnumerator newTokensClaim(string dbUrl, int amount)
    {
        var cert = new ForceAcceptAll();
        string url = $"{dbUrl}/{gameId}/{username}/tokensclaim?amount={amount}";
        UnityWebRequest request = UnityWebRequest.Put(url, new byte[0]);
        request.method = UnityWebRequest.kHttpVerbPUT;
        request.SetRequestHeader("Authorization", "Bearer " + jwt());
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
