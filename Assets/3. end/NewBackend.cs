using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using System;

public class NewBackend : MonoBehaviour
{
    public static NewBackend Instance { get; private set; }
    public string auth;
    public string baseUrl;
    public string bearer;
    public string token_name;
    public string token_id;
    public string game_id;
    public string username;

    [SerializeField] private TokenSO token;

    public TMP_Text authText;
    public TMP_Text amountText;

    public HttpClient client = new HttpClient();
    public string deeplinkURL;

    public TMP_Text coinText;
    public float delayTime = 5f;

    int coin = 0;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("Coin");
        amountText.SetText(coin.ToString());

        if (Instance == null)
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
        }
        else
        {
            //Destroy(gameObject);
        }
        //GetDBToken();
        //string user = PlayerPrefs.GetString("User");
        //authText.text = user;
        PlayerPrefs.SetString("Bearer", bearer);
        

    }

    public void Start()
    {
        //Global.backend= this;
        DontDestroyOnLoad(this.gameObject);
        client = new HttpClient();
        // originalFontSize = tokentext.fontSize;
        Debug.Log("baseurl: " + baseUrl);
        Debug.Log("bearer: " + bearer);
        Debug.Log("user: " + auth);
#if UNITY_WEBGL
        GetAuthFromWebGL();
#endif

#if UNITY_EDITOR
        StartCoroutine(GetUser(() =>
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
            Debug.Log("new user: " + auth);
        }
    }

    public IEnumerator GetUser(Action callback)
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.

        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/api/v1/users/{auth}");
        request.SetRequestHeader("Authorization", "Bearer " + bearer);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error :(");
            // onErrorCallback(request.result);
            authText.text = "login error";
            Debug.LogError(request.error, this);
        }
        else
        {
            var jsonData = JSON.Parse(request.downloadHandler.text);
            this.username = jsonData["username"];
            //token.User = username;
            authText.text = username;
            PlayerPrefs.SetString("User", username);
            Debug.Log("User retrieved.");
            callback.Invoke();
        }
    }

    public void GetDBToken()
    {
        //int point = pointSO.Value;
        int coin = PlayerPrefs.GetInt("Coin");
        //this.pointText.text = pointSO.Value + " POINTS!";
        this.coinText.text = coin + "";
        this.amountText.text = coin + "";
        //pointText.text = pointSO.Value + " POINTS!";
        //this.amountText.text = pointSO.Value + "";
    }

    private void onDeepLinkActivated(string url)
    {
        // Update DeepLink Manager global variable, so URL can be accessed from anywhere.
        deeplinkURL = url;
        // Decode the URL to determine action. 
        // In this example, the app expects a link formatted like this:
        // unitydl://mylink?scene1
        this.auth = url.Split("?"[0])[1].Split("=")[1];
        authText.text = this.auth;
        StartCoroutine(GetUser(() =>
        {
            GetDBToken();
        }));

        Debug.Log("Opened from deeplink!");
    }

    public void VoidClaimToken()
    {
        Debug.Log("Clicked claim button");
        Invoke("displayPanel", delayTime);
        StartCoroutine(ClaimTokens());
        PlayerPrefs.SetInt("Coin", 0);

    }

    void displayPanel()
    {
        
    }

    public IEnumerator ClaimTokens()
    {
        //var amount = pointSO.Value;
        int coin = PlayerPrefs.GetInt("Coin");
        var amount = coin;
        bearer = PlayerPrefs.GetString("Bearer");

        //Get from firestore
        Debug.Log("Claiming Token ");

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/api/v1/transactions/distribute?TokenId={token_id}&Amount={amount}&Auth={auth}", "POST");
        request.SetRequestHeader("Authorization", "Bearer " + bearer);

        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error :(");
            // onErrorCallback(request.result);
            Debug.LogError(request.error, this);
        }
        else
        {
            Debug.Log("Claim token amount: " + amount);
            PlayerPrefs.SetInt("Coin", 0);
            GetDBToken();
            Debug.Log("Tokens claimed!");
        }
    }




    // Update is called once per frame
    void Update()
    {

    }
}
