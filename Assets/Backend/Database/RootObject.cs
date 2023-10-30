using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Backend.Database
{
    [Serializable]
    public class RootObject
    {
        public b_Score b_Score;
        public c_TokensReq c_TokensReq;
        public d_TokensClaim d_TokensClaim;

        public string bearerToken;
    }


    [Serializable]
    public class b_Score
    {
        public int d_TotalScore;
    }

    [Serializable]
    public class c_TokensReq
    {
        public int a_TokensReq;
        public string b_TxnHash;
        public int d_TimeStamp;

    }

    [Serializable]
    public class d_TokensClaim
    {
        public int g_TokensClaimed;
    }


}
