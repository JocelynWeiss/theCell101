using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;


// Class holding scoring informations
[System.Serializable]
public class ScoreInfo
{
    public string AndroidId;
    public string TimeOLEDateUTC; // UTC OLE date as string
    public int GameLength; // Game duration in seconds
    public int Deaths; // Number of player's death
    public int CodeBonus; // Additional points for the code
    public int FinalScore;
}


// Class holding scoring informations
[System.Serializable]
public class ScoreInfoRecord
{
    public List<ScoreInfo> AllScores;


    // To sort records using Final score from min to max
    public static int CompareByFinalScoreMinToMax(ScoreInfo s1, ScoreInfo s2)
    {
        return s1.FinalScore.CompareTo(s2.FinalScore);
    }


    // To sort records using Final score from max to min
    public static int CompareByFinalScoreMaxToMin(ScoreInfo s1, ScoreInfo s2)
    {
        return s2.FinalScore.CompareTo(s1.FinalScore);
    }


    public static int CompareByDeath(ScoreInfo s1, ScoreInfo s2)
    {
        return s1.Deaths.CompareTo(s2.Deaths);
    }
}


// Class holding scoring informations
[System.Serializable]
public class ScoreCommand
{
    public string Command;
    public ScoreInfo Value;


    public ScoreCommand(ScoreInfo info)
    {
        Command = "TheCellScoring";
        Value = info;
    }
}


// Class handling scoring
public class ScoringClass
{
    public enum SiteIDs
    {
        Undefined = 0,
        Sion,
        Yverdon,
    }

    public string m_fileName = "Scores.json";
    public string PPKEY_HMD_ID = "TheCell_HmdId";
    public string m_HMDid = "Dev_1";
    public bool m_IdIsSet = false;
    public ScoreInfoRecord m_AllScores = new ScoreInfoRecord();


    // Start is called before the first frame update
    public void Init()
    {
        //PlayerPrefs.DeleteKey(PPKEY_HMD_ID);

        m_HMDid = GetHmdId();
        //TestScoreInfo();
        //TestScoreFile();

        LoadScoresToJson();
    }


    // --- Testing ---
    public void TestScoreInfo()
    {
        ScoreInfo info = new ScoreInfo();

        //info.AndroidId = "1PASH8B1TP9563";
        info.AndroidId = m_HMDid;
        DateTime dateUtc = System.DateTime.UtcNow;
        double dateOA = dateUtc.ToOADate();
        info.TimeOLEDateUTC = dateOA.ToString();
        info.GameLength = 300;
        info.Deaths = 1;
        info.CodeBonus = 100;
        info.FinalScore = 1600;

        //string text = JsonUtility.ToJson(info, true);
        //Debug.Log($"===== ScoringInfo ===== \n{text}");

        ScoreCommand cmd = new ScoreCommand(info);
        string text = JsonUtility.ToJson(cmd, true);
        Debug.Log($"===== ScoringInfo ===== \n{text}");

        //TheCellGameMgr.instance.StartCoroutine(SendScore("https://helvetia-games-shop.ch/service", text));
        TheCellGameMgr.instance.StartCoroutine(SendScore("https://henigma.ch/the-cell-scoring/", text)); // sur WordPress

        // Get socres after x sec
        //TheCellGameMgr.instance.StartCoroutine(GetScore("https://helvetia-games-shop.ch/service/ScoreInfo", 3.0f));
    }


    // Send one score to the server
    public IEnumerator SendScore(string url, string json)
    {
        float startTime = Time.fixedTime;
        Debug.Log($"=====UnityWebRequest.Post: {url}");

        UnityWebRequest wwwReq = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        wwwReq.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        wwwReq.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        wwwReq.SetRequestHeader("Content-Type", "application/json");
        Debug.Log($"URI:{wwwReq.uri} ---URL:{wwwReq.url} ---{wwwReq.GetRequestHeader("Content-Type")} ---Length = {wwwReq.uploadedBytes}/{wwwReq.uploadHandler.data.Length}");
        yield return wwwReq.SendWebRequest();

        if (wwwReq.isNetworkError || wwwReq.isHttpError)
        {
            PrintDebugError(wwwReq);
        }
        else
        {
            if (wwwReq.isDone == false)
            {
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"Post done in: {Time.fixedTime - startTime}s");
            string dlText = wwwReq.downloadHandler.text; // cache the text as it should be
            Debug.Log(dlText);
            //m_authReceived = JsonUtility.FromJson<AuthReceived>(dlText); // Jow: Are we receiving anything ?
            //Debug.Log($"authUrl= {m_authReceived.authUrl}");
        }
    }


    // Fetch score data
    public IEnumerator GetScore(string uri, float sec)
    {
        yield return new WaitForSecondsRealtime(sec);

        float startTime = Time.fixedTime;
        Debug.Log($"=====UnityWebRequest.Get @ {startTime}s: {uri}");
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogWarning(www.error);
        }
        else
        {
            if (www.isDone == false)
            {
                yield return new WaitForEndOfFrame();
            }
            Debug.Log($"Get done in: {Time.fixedTime - startTime}s");
            string dlText = www.downloadHandler.text; // cache the text as it should be
            Debug.Log(dlText);
            //ServiceInfoClass loadedData = JsonUtility.FromJson<ServiceInfoClass>(dlText);
            //loadedData.DebugPrintMe();
        }
    }


    // output debug info from www request
    static void PrintDebugError(UnityWebRequest www)
    {
        Debug.LogWarning(www.error);
        Debug.LogWarning(www.downloadHandler.text);
        Dictionary<string, string> dic = www.GetResponseHeaders();
        if (dic != null)
        {
            string allDic = "";
            foreach (KeyValuePair<string, string> p in dic)
            {
                allDic += p.Key + " = " + p.Value + "\n";
            }
            Debug.Log(allDic);
        }
    }


    // Loading this headset scrores from file
    public void LoadScoresToJson()
    {
        m_AllScores.AllScores = new List<ScoreInfo>();

        string filePath;
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, m_fileName);
        if (System.IO.File.Exists(filePath))
        {
            string allLines = System.IO.File.ReadAllText(filePath);
            m_AllScores = JsonUtility.FromJson<ScoreInfoRecord>(allLines);
        }

#if UNITY_EDITOR
        m_AllScores.AllScores.Sort(ScoreInfoRecord.CompareByFinalScoreMinToMax);
        string jsonDic = JsonUtility.ToJson(m_AllScores, true);
        Debug.Log($"Scores file: {jsonDic}");

        m_AllScores.AllScores.Sort(ScoreInfoRecord.CompareByFinalScoreMaxToMin);
        jsonDic = JsonUtility.ToJson(m_AllScores, true);
        Debug.Log($"CompInv===== \n{jsonDic}");
#endif
    }


    //
    public void SaveScoresToFile()
    {
        string filePath;
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, m_fileName);
        string jsonDic = JsonUtility.ToJson(m_AllScores, true);
        System.IO.File.WriteAllText(filePath, jsonDic); // Automatically overwriting if access is ok

#if UNITY_EDITOR
        Debug.Log($"Saving scores to {filePath}\n{jsonDic}");
#endif
    }


    // Testing all scores
    void TestScoreFile()
    {
        // --1-- Load 
        m_AllScores.AllScores = new List<ScoreInfo>();

        string fileName = m_fileName;
        string filePath;
        filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        /*
        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
        www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogWarning(www.error);
        }
        while (!www.isDone)
        {
        }
        String jsonString = www.downloadHandler.text;
        Debug.Log($"Loaded from {filePath} = {jsonString.Length}");
        */
        //string[] allLines = System.IO.File.ReadAllLines(filePath);
        string allLines = System.IO.File.ReadAllText(filePath);
        m_AllScores = JsonUtility.FromJson<ScoreInfoRecord>(allLines);


        // --2-- Add more
        for (int i = 0; i < 5; i++)
        {
            ScoreInfo info = new ScoreInfo();
            //info.AndroidId = "1PASH8B1TP9563";
            info.AndroidId = m_HMDid;
            DateTime dateUtc = System.DateTime.UtcNow;
            double dateOA = dateUtc.ToOADate();
            info.TimeOLEDateUTC = dateOA.ToString();
            info.GameLength = 300;
            info.Deaths = i;
            info.CodeBonus = 100;
            info.FinalScore = 1600;

            m_AllScores.AllScores.Add(info);
        }

        string jsonDic = JsonUtility.ToJson(m_AllScores, true);
        Debug.Log($"===== ScoringInfo ===== \n{jsonDic}");

        // --3-- Save to file
        /*
        UnityWebRequest wwwToSave = UnityWebRequest.Put(filePath, jsonDic);
        wwwToSave.SendWebRequest();
        while (!wwwToSave.isDone)
        {
        }
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogWarning(www.error);
            if (File.Exists(filePath))
                System.IO.File.Delete(filePath); // Doesn't work !
            System.IO.File.WriteAllText(filePath, jsonDic); // JowNext make sure the file is overwritten !
        }
        else
        {
            Debug.Log($"File saved to {filePath}");
        }
        */

        /*
        //string jsonDic = JsonUtility.ToJson(data, true);
        //string filePath = Path.Combine(Application.streamingAssetsPath, "classicalSettings/") + "AiSettings.json";
        using (StreamWriter sw = File.CreateText(filePath))
        {
            sw.WriteLine(jsonDic);
            OzLogger.Debug(() => $"Ai general settings saved to {filePath}");
            sw.Close();
        }
        */


        /*
        if (File.Exists(filePath))
        {
            System.IO.File.Delete(filePath); // Doesn't work !
            Debug.Log($"Deleting {filePath}");
        }
        */
        ///System.IO.File.WriteAllText(filePath, jsonDic); // Automatically overwriting if access is ok

        m_AllScores.AllScores.Sort(ScoreInfoRecord.CompareByDeath);
        jsonDic = JsonUtility.ToJson(m_AllScores, true);
        Debug.Log($"===== Sorted ScoringInfo ===== \n{jsonDic}");
    }


    // Fetch HMD ID
    public string GetHmdId()
    {
        string id = "Dev_1";
        if (PlayerPrefs.HasKey(PPKEY_HMD_ID))
        {
            m_IdIsSet = true;
            id = PlayerPrefs.GetString(PPKEY_HMD_ID);
        }

        return id;
    }


    public void SetHmdId(string newId)
    {
        m_IdIsSet = true;
        m_HMDid = newId;

        // Set in player pref
        PlayerPrefs.SetString(PPKEY_HMD_ID, m_HMDid);
    }


    // Add a new score to the actual list and return its ranking
    public int AddNewScore(float gameDurInSec, int deathNb, int points, int total)
    {
        ScoreInfo info = new ScoreInfo();
        info.AndroidId = m_HMDid;
        DateTime dateUtc = System.DateTime.UtcNow;
        double dateOA = dateUtc.ToOADate();
        info.TimeOLEDateUTC = dateOA.ToString();
        info.GameLength = (int)gameDurInSec;
        info.Deaths = deathNb;
        info.CodeBonus = points;
        info.FinalScore = total;

        m_AllScores.AllScores.Add(info);

        SaveScoresToFile();

        // return its ranking
        m_AllScores.AllScores.Sort(ScoreInfoRecord.CompareByFinalScoreMaxToMin);
        int rank = m_AllScores.AllScores.IndexOf(info) + 1;
        Debug.Log($"Ranking: {rank} / {m_AllScores.AllScores.Count}");

        return rank;
    }
}
