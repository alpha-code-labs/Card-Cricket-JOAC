using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirestoreManager : MonoBehaviour
{
    private static FirestoreManager instance;
    private FirebaseFirestore db;
    private string deviceId;
    
    // ✅ THREE COLLECTIONS
    private string usersCollection = "users";
    private string strikeRatesCollection = "strikeRates";
    private string battingAveragesCollection = "battingAverages";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFirestore();
    }

    private void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
        deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log($"✅ Firestore initialized. Device ID: {deviceId}");
    }

    // ✅ SAVE USERNAME TO FIRESTORE
    // public void SaveUsernameToFirestore(string username)
    // {
    //     if (db == null)
    //     {
    //         Debug.LogError("❌ Firestore not initialized!");
    //         return;
    //     }

    //     Dictionary<string, object> userData = new Dictionary<string, object>
    //     {
    //         { "userName", username }
    //     };

    //     db.Collection(usersCollection)
    //         .Document(deviceId)
    //         .SetAsync(userData)
    //         .ContinueWithOnMainThread(task =>
    //         {
    //             if (task.IsCompleted && !task.IsFaulted)
    //             {
    //                 Debug.Log($"✅ Username saved to Firestore: {username}");
    //             }
    //             else
    //             {
    //                 Debug.LogError($"❌ Error saving username: {task.Exception}");
    //             }
    //         });
    // }



public void SaveUsernameToFirestore(string username)
{
    if (db == null)
    {
        Debug.LogError("❌ Firestore not initialized!");
        return;
    }

    // ✅ Simple combined device info string
    string deviceInfo = $"{SystemInfo.deviceModel} | {SystemInfo.operatingSystem}";

    Dictionary<string, object> userData = new Dictionary<string, object>
    {
        { "userName", username },
        { "deviceInfo", deviceInfo },
        { "lastUpdated", FieldValue.ServerTimestamp }
    };

    db.Collection(usersCollection)
        .Document(deviceId)
        .SetAsync(userData)
        .ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log($"✅ Username saved: {username} | Device: {deviceInfo}");
            }
            else
            {
                Debug.LogError($"❌ Error saving username: {task.Exception}");
            }
        });
}
    // ✅ LOAD USERNAME FROM FIRESTORE
    public void LoadUsernameFromFirestore()
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        db.Collection(usersCollection)
            .Document(deviceId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    
                    if (snapshot.Exists)
                    {
                        string username = snapshot.GetValue<string>("userName");
                        GameManager.instance.currentSaveData.userName = username;
                        Debug.Log($"✅ Username loaded from Firestore: {username}");
                    }
                    else
                    {
                        Debug.Log("📋 No username found in Firestore");
                    }
                }
                else
                {
                    Debug.LogError($"❌ Error loading username: {task.Exception}");
                }
            });
    }

    // ✅ SAVE STATS TO FIRESTORE (SEPARATE COLLECTIONS)
    public void SaveStatsToFirestore(float strikeRate, float battingAverage)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        // ✅ SAVE TO STRIKE RATES COLLECTION
        SaveStrikeRateToFirestore(strikeRate);

        // ✅ SAVE TO BATTING AVERAGES COLLECTION
        SaveBattingAverageToFirestore(battingAverage);
    }

    // ✅ SAVE STRIKE RATE COLLECTION
    private void SaveStrikeRateToFirestore(float strikeRate)
    {
        Dictionary<string, object> strikeRateData = new Dictionary<string, object>
        {
            { "userName", GameManager.instance.currentSaveData.userName },
            { "strikeRate", strikeRate }
        };

        db.Collection(strikeRatesCollection)
            .Document(deviceId)
            .SetAsync(strikeRateData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ StrikeRate saved to Firestore: {strikeRate:F1}");
                }
                else
                {
                    Debug.LogError($"❌ Error saving StrikeRate: {task.Exception}");
                }
            });
    }

    // ✅ SAVE BATTING AVERAGE COLLECTION
    private void SaveBattingAverageToFirestore(float battingAverage)
    {
        Dictionary<string, object> battingAverageData = new Dictionary<string, object>
        {
            { "userName", GameManager.instance.currentSaveData.userName },
            { "battingAverage", battingAverage }
        };

        db.Collection(battingAveragesCollection)
            .Document(deviceId)
            .SetAsync(battingAverageData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ BattingAverage saved to Firestore: {battingAverage:F1}");
                }
                else
                {
                    Debug.LogError($"❌ Error saving BattingAverage: {task.Exception}");
                }
            });
    }

    public static FirestoreManager GetInstance()
    {
        return instance;
    }
}