using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TokenSO : ScriptableObject
{
    // Start is called before the first frame update
    [SerializeField]
    private int _coin;

    public int Coin
    {
        get { return _coin; }
        set { _coin = value; }

    }


    [SerializeField]
    private string _bearer;

    public string Bearer
    {
        get { return _bearer; }
        set { _bearer = value; }
    }

    [SerializeField]
    private string _coinid;

    public string coin_ID
    {
        get { return _coinid; }
        set { _coinid = value; }
    }


}
