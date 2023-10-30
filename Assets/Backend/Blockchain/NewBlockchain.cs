using System;
using System.Net.Http;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using Backend.Database;

public class NewBlockchain : MonoBehaviour
{
    public NewDBHandler dbh;
    public NewInternalDB idb;
    public static NewBlockchain Instance { get; private set; }

    public string baseUrl_testnet = "https://xar-autosigner-2.proximaxtest.com";
    public string baseUrl_mainnet = "https://xar-autosigner.proximaxtest.com";
    public string dbUrl_testnet = "https://metx-demo.vercel.app/api/v1";
    public string dbUrl_mainnet = "https://metx-superclimber.vercel.app/api/v1";
    public string gameId = "D16E060D72E12794";
    public string auth;


    public string tokenId;

    public int limitScore;
    public string username;
    public HttpClient client = new HttpClient();
    public string deeplinkURL;

    public TMP_Text usernameText;

    /*Claim panel*/
    public GameObject panelClose;
    public GameObject panelOpen1;
    public GameObject panelOpen2;
    public GameObject YesButton;
    public TMP_Text amountTokenText;
    public GameObject notiText;

    public GameObject UnknownUserPanel;
    public GameObject MenuPanel;
    //public GameObject TitleText;
    public GameObject ScorePanel;
    public GameObject OfflineLabel;
    public TMP_InputField newUserText;
    public GameObject ErrorClaim;
    public GameObject ChangeUserButton;
    public GameObject UserError;
    public GameObject AskUser;


    /*-----TIME------*/
    int sec = System.DateTime.Now.Second;
    int min = System.DateTime.Now.Minute;
    int hour = System.DateTime.Now.Hour;
    int year = System.DateTime.Now.Year;
    int month = System.DateTime.Now.Month;
    int day = System.DateTime.Now.Day;

    private void Awake()
    {
        int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
        PlayerPrefs.SetInt("limit", limitScore);

        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Store token ID in PlayerPrefs for persistence across game sessions
        PlayerPrefs.SetString("token_id", tokenId);

        // Update UI elements
        idb.TotalScoreText.text = idb.TotalScore().ToString();
        idb.TotalTokenText.text = idb.TotalScore() + " " + tokenId;

        // Process deep link activation
        Application.deepLinkActivated += onDeepLinkActivated;
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            // Cold start and Application.absoluteURL not null so process Deep Link.
            onDeepLinkActivated(Application.absoluteURL);
        }
        else
        {
            deeplinkURL = "[none]";
        }

        int lastLoginTime = PlayerPrefs.GetInt("LoginTime") + 5;

        if (lastLoginTime > currentTimeStamp)
        {
            PlayerPrefs.DeleteAll();
        }

    }


    public void Start()
    {
        dbh.Bearer();
        CallAuth();
        Time.timeScale = 1;
        client = new HttpClient();
        Debug.Log("auth: " + idb.auth());

    }



    IEnumerator delayGetData()
    {
        yield return new WaitForSeconds(10);
        dbh.GetJSON();
        idb.TotalScoreText.text = idb.TotalScore() + "";
        idb.TotalTokenText.text = idb.TotalScore() + " " + tokenId;
    }

    public class ForceAcceptAll : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    private void CallAuth()
    {
#if UNITY_WEBGL
        GetAuthFromWebGL();
#endif

#if UNITY_EDITOR
        PlayerPrefs.SetString("Auth", auth);
        StartCoroutine(GetUserTestnet(() =>
        {
            GetDBToken();
        }));
#endif
    }

    public void GetAuthFromWebGL()
    {
        int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
        int pm = Application.absoluteURL.IndexOf("?");
        if (pm != -1)
        {
            auth = Application.absoluteURL.Split("?"[0])[1].Split("=")[1];
            if (auth != idb.auth())
            {
                PlayerPrefs.SetString("Auth", auth);
                Debug.Log("new user: " + idb.auth());
                StartCoroutine(GetUserTestnet(() =>
                {
                    GetDBToken();
                }));
            }
            else if (auth == idb.auth())
            {
                PlayerPrefs.SetInt("isLogin", 1);
            }

        }
    }

    public void GetDBToken()
    {

    }

    public IEnumerator GetUserTestnet(Action callback)
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_testnet}/api/v1/users/{idb.auth()}");
        request.SetRequestHeader("Authorization", "Bearer " + idb.bearerTestnet());
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
                Debug.Log("Node: " + idb.node());
                usernameText.text = "" + username;
                MenuUIPanel(false, false, true, true, false, false);
                idb.GameStart();
            }
            callback.Invoke();
        }
        /* idb.GameStart(); */
    }

    private void MenuUIPanel(bool unknown, bool change, bool menu, bool score, bool ask, bool error)
    {
        UnknownUserPanel.SetActive(unknown);
        ChangeUserButton.SetActive(change);
        MenuPanel.SetActive(menu);
        ScorePanel.SetActive(score);
        AskUser.SetActive(ask);
        UserError.SetActive(error);
    }

    public IEnumerator GetUserMainnet()
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_mainnet}/api/v1/users/{idb.auth()}");
        request.SetRequestHeader("Authorization", "Bearer " + idb.bearerMainnet());
        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Login Mainnet Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
            // Retrieve the boolean value from PlayerPrefs
            int loginValue = PlayerPrefs.GetInt("isLogin");
            bool isLogin = loginValue == 1;
            if (isLogin)
            {
                // The player is already logged in, so we can skip the authentication step
                usernameText.text = idb.nama();
                MenuUIPanel(false, true, true, false, true, false);
            }
            else
            {
                PlayerPrefs.SetString("username", "OFFLINE");
                usernameText.text = "OFFLINE";
                MenuUIPanel(true, false, false, false, true, false);
            }
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            if (this.username == null || this.username == "")
            {
                // Retrieve the boolean value from PlayerPrefs
                int loginValue = PlayerPrefs.GetInt("isLogin");
                bool isLogin = loginValue == 1;
                if (isLogin)
                {
                    // The player is already logged in, so we can skip the authentication step
                    usernameText.text = idb.nama();
                    MenuUIPanel(false, true, true, false, true, false);
                }
                else
                {
                    PlayerPrefs.SetString("username", "OFFLINE");
                    usernameText.text = "OFFLINE";
                    MenuUIPanel(true, false, false, false, true, false);
                }
            }
            else
            {
                PlayerPrefs.SetString("username", username);
                Debug.Log("User retrieved. Username: " + username);
                PlayerPrefs.SetString("node", "mainnet");
                usernameText.text = "" + username;
                Debug.Log("Node: " + idb.node());
                MenuUIPanel(false, false, true, true, false, false);
                idb.GameStart();
            }
        }
    }

    public void ActivateAutosigner()
    {
        StartCoroutine(GetUserTestnetIfASFail(() =>
        {
            GetDBToken();
        }));
    }

    public IEnumerator GetUserTestnetIfASFail(Action callback)
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_testnet}/api/v1/users/{idb.auth()}");
        request.SetRequestHeader("Authorization", "Bearer " + idb.bearerTestnet());
        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Login Testnet Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
            StartCoroutine(GetUserMainnetIfASFail());
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            if (this.username == null || this.username == "")
            {
                StartCoroutine(GetUserMainnetIfASFail());
            }
            else
            {
                PlayerPrefs.SetString("username", username);
                Debug.Log("User retrieved. Username: " + username);
                PlayerPrefs.SetString("node", "testnet");
                Debug.Log("Node: " + idb.node());
                usernameText.text = "" + username;
                MenuUIPanel(false, false, true, true, false, false);
                idb.GameStart();
            }
            callback.Invoke();
        }
        /* idb.GameStart(); */
    }

    public IEnumerator GetUserMainnetIfASFail()
    {
        var cert = new ForceAcceptAll();
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl_mainnet}/api/v1/users/{idb.auth()}");
        request.SetRequestHeader("Authorization", "Bearer " + idb.bearerMainnet());
        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Login Mainnet Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
            MenuUIPanel(false, false, false, false, false, true);
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            if (this.username == null || this.username == "")
            {
                Debug.Log("Login Mainnet Error :(");
                // onErrorCallback(request.result);
                Debug.LogError(request.error, this);
                MenuUIPanel(false, false, false, false, false, true);
            }
            else
            {
                PlayerPrefs.SetString("username", username);
                Debug.Log("User retrieved. Username: " + username);
                PlayerPrefs.SetString("node", "mainnet");
                usernameText.text = "" + username;
                Debug.Log("Node: " + idb.node());

                MenuUIPanel(false, false, true, true, false, false);
                idb.GameStart();
            }
        }
    }

    private void onDeepLinkActivated(string url)
    {
        //bearer = PlayerPrefs.GetString("Bearer");
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://mylink?scene1
        this.auth = url.Split("?"[0])[1].Split("=")[1];
        //authText.text = this.auth;
        StartCoroutine(GetUserTestnet(() =>
        {
            GetDBToken();
        }));

        Debug.Log("Opened from deeplink!");
    }

    public void VoidClaimReq()
    {
        Debug.Log("Clicked claim button");
        YesButton.SetActive(false);
        if (idb.node() == "testnet")
        {
            StartCoroutine(DistributeToken(baseUrl_testnet, idb.bearerTestnet()));
        }
        else if (idb.node() == "mainnet")
        {
            StartCoroutine(DistributeToken(baseUrl_mainnet, idb.bearerMainnet()));
        }

    }

    public IEnumerator DistributeToken(string baseUrl, string bearer)
    {
        var amount = PlayerPrefs.GetInt("TotalScore");
        if (amount > 2000) { amount = 2000; }
        else { amount = PlayerPrefs.GetInt("TotalScore"); }

        /*int amount = PlayerPrefs.GetInt("TotalScore");*/
        var id_igt = tokenId;
        var cert = new ForceAcceptAll();

        //var amount = PlayerPrefs.GetInt("IGT");
        Debug.Log("Claiming Token " + amount + " " + id_igt);

        UnityWebRequest request = UnityWebRequest.Post($"{baseUrl}/api/v1/transactions/distribute?TokenId={id_igt}&Amount={amount}&Auth={idb.auth()}", new WWWForm());
        request.SetRequestHeader("Authorization", "Bearer " + bearer);

        request.certificateHandler = cert;

        // Send
        cert?.Dispose();

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            ErrorClaim.SetActive(true);
            StartCoroutine(ErrorPanelDelay());
            Debug.Log("Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            string hash = jsonData["viewModel"]["hash"];
            Debug.Log("Txn hash: " + hash);
            PlayerPrefs.SetInt("TokensRequested", amount);
            TokensReq(amount, hash);
            GetDBToken();
            Debug.Log("Claimed Token. Please check Xarcade.");
        }
    }

    private void TokensReq(int amount, string hash)
    {
        int tr = amount;
        string txnHash = hash;
        int ts = 0;
        Debug.Log("Processing in db ...");
        PlayerPrefs.SetInt("TotalScore", 0);
        dbh.PostTokensReq(tr, txnHash);
        amountTokenText.text = ts + " " + tokenId;
        StartCoroutine(ClosePanelDelayed());
    }


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
                dbh.PostTotalScore();
                amountTokenText.text = newts + " " + idb.token_id();
                idb.TotalScoreText.text = newts + "";
                idb.claimButton.SetActive(true);
                PlayerPrefs.SetInt("TokensRequested", 0);
                dbh.PostTokensReq(0, null);
            }
        }
    }

    public void voidTxnStatusOT(int ts, int tr, string hash, int tc)
    {
        if (idb.node() == "testnet")
        {
            StartCoroutine(txnStatusOT(baseUrl_testnet, idb.bearerTestnet(), ts, tr, hash, tc));
        }
        else if (idb.node() == "mainnet")
        {
            StartCoroutine(txnStatusOT(baseUrl_mainnet, idb.bearerMainnet(), ts, tr, hash, tc));
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
                amountTokenText.text = ts + " " + idb.token_id();
                idb.TotalScoreText.text = ts + "";
                idb.claimButton.SetActive(false);
            }
        }
    }

    public void voidTxnStatusBT(int ts, int tr, string hash, int tc)
    {
        if (idb.node() == "testnet")
        {
            StartCoroutine(txnStatusBT(baseUrl_testnet, idb.bearerTestnet(), ts, tr, hash, tc));
        }
        else if (idb.node() == "mainnet")
        {
            StartCoroutine(txnStatusBT(baseUrl_mainnet, idb.bearerMainnet(), ts, tr, hash, tc));
        }
    }

    //fix this
    private void ClaimingToken(int tr, int tc)
    {
        Debug.Log("Processing in db ...");
        PlayerPrefs.SetInt("TokensClaimed", tc + tr);
        tc = tc + tr;
        dbh.PostTokensClaim(tc);
        amountTokenText.text = 0 + " " + idb.token_id();
        idb.TotalScoreText.text = 0 + "";

    }


    public IEnumerator ClosePanelDelayed()
    {
        yield return new WaitForSeconds(5f);    // Wait for 5 seconds
        panelClose.SetActive(false);
        panelOpen1.SetActive(true);
        panelOpen2.SetActive(true);
        YesButton.SetActive(true);
        notiText.SetActive(false);
    }

    public IEnumerator ErrorPanelDelay()
    {
        yield return new WaitForSeconds(2f);    // Wait for 5 seconds
        ErrorClaim.SetActive(false);
    }

    public void UserOffline()
    {
        PlayerPrefs.SetString("username", "OFFLINE");
        Debug.Log("User retrieved. Username: OFFLINE");
        usernameText.text = "OFFLINE";
    }

    public void NewUser()
    {
        int currentTimeStamp = min + (60 * hour) + (1440 * day) + (43800 * month) + (525600 * year);
        PlayerPrefs.SetString("username", newUserText.text);
        Debug.Log("User retrieved. Username: " + newUserText.text);
        usernameText.text = newUserText.text;
        PlayerPrefs.SetInt("isLogin", 1);
        PlayerPrefs.SetString("node", "testnet");
        Debug.Log("Node: " + idb.node());
        MenuUIPanel(false, true, true, false, false, false);
        PlayerPrefs.SetInt("LoginTime", currentTimeStamp);
        idb.GameStart();
    }


}