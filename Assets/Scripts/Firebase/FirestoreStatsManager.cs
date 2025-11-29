using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class FirestoreStatsManager : MonoBehaviour
{
    private static FirestoreStatsManager instance;
    private FirebaseFirestore db;
    private string deviceId;
    
    // ✅ COLLECTION NAME
    private string playerStatsCollection = "playerStats";

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
        Debug.Log($"✅ Firestore Stats initialized. Device ID: {deviceId}");
    }

    // ✅ SAVE INDIVIDUAL MATCH ATTEMPT
    public void SaveMatchAttemptToFirestore(int gameplayNumber, string matchDate, int runsScored, 
        int ballsFaced, int wicketsLost, int fours, int sixes, bool wonMatch, int attemptNumber)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        Dictionary<string, object> attemptData = new Dictionary<string, object>
        {
            { "attemptNumber", attemptNumber },
            { "runsScored", runsScored },
            { "ballsFaced", ballsFaced },
            { "wicketsLost", wicketsLost },
            { "fours", fours },
            { "sixes", sixes },
            { "wonMatch", wonMatch },
            { "attemptDate", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
            { "userName", GameManager.instance.currentSaveData.userName }
        };

        // ✅ Structure: playerStats/{deviceId}/gameplays/{gameplayNumber}/attempts/{attemptNumber}
        db.Collection(playerStatsCollection)
            .Document(deviceId)
            .Collection("gameplays")
            .Document(gameplayNumber.ToString())
            .Collection("attempts")
            .Document(attemptNumber.ToString())
            .SetAsync(attemptData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ Match attempt saved to Firestore - Gameplay {gameplayNumber}, Attempt {attemptNumber}");
                }
                else
                {
                    Debug.LogError($"❌ Error saving match attempt: {task.Exception}");
                }
            });
    }

    // ✅ SAVE ALL GAMEPLAY STATS
    public void SaveGameplayStatsToFirestore(MatchStatistics stats)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        Dictionary<string, object> gameplayData = new Dictionary<string, object>
        {
            { "gameplayNumber", stats.gameplayNumber },
            { "matchDate", stats.matchDate },
            { "totalAttempts", stats.TotalAttempts },
            { "averageRuns", stats.AverageRuns },
            { "bestScore", stats.BestScore },
            { "averageStrikeRate", stats.AverageStrikeRate },
            { "mostBoundaries", stats.MostBoundaries },
            { "totalOuts", stats.TotalOuts },
            { "wins", stats.Wins },
            { "winRate", stats.WinRate }
        };

        // ✅ Save gameplay summary
        db.Collection(playerStatsCollection)
            .Document(deviceId)
            .Collection("gameplays")
            .Document(stats.gameplayNumber.ToString())
            .SetAsync(gameplayData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ Gameplay {stats.gameplayNumber} stats saved to Firestore");
                }
                else
                {
                    Debug.LogError($"❌ Error saving gameplay stats: {task.Exception}");
                }
            });

        // ✅ Save individual attempts
        foreach (var attempt in stats.attempts)
        {
            SaveMatchAttemptToFirestore(stats.gameplayNumber, stats.matchDate, attempt.runsScored,
                attempt.ballsFaced, attempt.wicketsLost, attempt.fours, attempt.sixes, 
                attempt.wonMatch, attempt.attemptNumber);
        }
    }

    // ✅ LOAD ALL GAMEPLAY STATS
    public void LoadGameplayStatsFromFirestore(int gameplayNumber, System.Action<MatchStatistics> onComplete)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        db.Collection(playerStatsCollection)
            .Document(deviceId)
            .Collection("gameplays")
            .Document(gameplayNumber.ToString())
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    
                    if (snapshot.Exists)
                    {
                        string matchDate = snapshot.GetValue<string>("matchDate");
                        var stats = new MatchStatistics(gameplayNumber, matchDate);
                        
                        // Load attempts
                        LoadGameplayAttemptsFromFirestore(gameplayNumber, stats, onComplete);
                    }
                    else
                    {
                        Debug.Log($"📋 No stats found for Gameplay {gameplayNumber}");
                        onComplete?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError($"❌ Error loading gameplay stats: {task.Exception}");
                    onComplete?.Invoke(null);
                }
            });
    }

    // ✅ LOAD ATTEMPTS FOR A GAMEPLAY
    private void LoadGameplayAttemptsFromFirestore(int gameplayNumber, MatchStatistics stats, System.Action<MatchStatistics> onComplete)
    {
        db.Collection(playerStatsCollection)
            .Document(deviceId)
            .Collection("gameplays")
            .Document(gameplayNumber.ToString())
            .Collection("attempts")
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    QuerySnapshot querySnapshot = task.Result;
                    
                    foreach (DocumentSnapshot doc in querySnapshot.Documents)
                    {
                        var attempt = new MatchAttempt(
                            doc.GetValue<int>("attemptNumber"),
                            doc.GetValue<int>("runsScored"),
                            doc.GetValue<int>("ballsFaced"),
                            doc.GetValue<int>("wicketsLost"),
                            doc.GetValue<int>("fours"),
                            doc.GetValue<int>("sixes"),
                            doc.GetValue<bool>("wonMatch")
                        );
                        stats.AddAttempt(attempt);
                    }
                    
                    Debug.Log($"✅ Loaded {stats.TotalAttempts} attempts for Gameplay {gameplayNumber}");
                    onComplete?.Invoke(stats);
                }
                else
                {
                    Debug.LogError($"❌ Error loading attempts: {task.Exception}");
                    onComplete?.Invoke(null);
                }
            });
    }

    public static FirestoreStatsManager GetInstance()
    {
        return instance;
    }
}