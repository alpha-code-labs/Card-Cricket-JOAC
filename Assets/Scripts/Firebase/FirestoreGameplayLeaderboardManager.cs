using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System;
using System.IO;
using System.Collections.Generic;

public class FirestoreGameplayLeaderboardManager : MonoBehaviour
{
    private static FirestoreGameplayLeaderboardManager instance;
    private FirebaseFirestore db;
    private string deviceId;
    
    private const string LEADERBOARD_GAMEPLAY_COLLECTION = "leaderboard_gameplay";
    
    // ✅ Pending uploads queue
    private static string PendingUploadsPath => Path.Combine(Application.persistentDataPath, "pending_leaderboard_uploads.json");
    private PendingUploadsData pendingUploads;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirestore();
            LoadPendingUploads();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ✅ Try to upload pending data when game starts
        RetryPendingUploads();
    }

    private void InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;
        deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log($"✅ FirestoreGameplayLeaderboardManager initialized. Device ID: {deviceId}");
    }

    public static FirestoreGameplayLeaderboardManager GetInstance()
    {
        return instance;
    }

    // ═══════════════════════════════════════════════════════════════
    // INTERNET CHECK
    // ═══════════════════════════════════════════════════════════════
    
    public static bool HasInternetConnection()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    // ═══════════════════════════════════════════════════════════════
    // PENDING UPLOADS SYSTEM
    // ═══════════════════════════════════════════════════════════════

    private void LoadPendingUploads()
    {
        try
        {
            if (File.Exists(PendingUploadsPath))
            {
                string json = File.ReadAllText(PendingUploadsPath);
                pendingUploads = JsonUtility.FromJson<PendingUploadsData>(json);
                Debug.Log($"📂 Loaded {pendingUploads.uploads.Count} pending uploads");
            }
            else
            {
                pendingUploads = new PendingUploadsData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error loading pending uploads: {e.Message}");
            pendingUploads = new PendingUploadsData();
        }
    }

    private void SavePendingUploads()
    {
        try
        {
            string json = JsonUtility.ToJson(pendingUploads, true);
            File.WriteAllText(PendingUploadsPath, json);
            Debug.Log($"💾 Saved {pendingUploads.uploads.Count} pending uploads");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error saving pending uploads: {e.Message}");
        }
    }

    private void AddToPendingQueue(PendingUploadEntry entry)
    {
        // ✅ Remove existing entry for same gameplay (update with latest)
        pendingUploads.uploads.RemoveAll(x => x.gameplayNumber == entry.gameplayNumber);
        
        pendingUploads.uploads.Add(entry);
        SavePendingUploads();
        Debug.Log($"📝 Added Gameplay {entry.gameplayNumber} to pending uploads queue");
    }

    private void RemoveFromPendingQueue(int gameplayNumber)
    {
        pendingUploads.uploads.RemoveAll(x => x.gameplayNumber == gameplayNumber);
        SavePendingUploads();
        Debug.Log($"✅ Removed Gameplay {gameplayNumber} from pending queue");
    }

    public void RetryPendingUploads()
    {
        if (!HasInternetConnection())
        {
            Debug.Log("📵 No internet - skipping pending uploads retry");
            return;
        }

        if (pendingUploads == null || pendingUploads.uploads.Count == 0)
        {
            Debug.Log("📭 No pending uploads to retry");
            return;
        }

        Debug.Log($"🔄 Retrying {pendingUploads.uploads.Count} pending uploads...");

        // ✅ Create a copy to iterate (avoid modification during iteration)
        List<PendingUploadEntry> uploadsToRetry = new List<PendingUploadEntry>(pendingUploads.uploads);

        foreach (var entry in uploadsToRetry)
        {
            UploadPendingEntry(entry);
        }
    }

    private void UploadPendingEntry(PendingUploadEntry entry)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        string documentId = $"{deviceId}_{entry.gameplayNumber}";

        Dictionary<string, object> leaderboardData = new Dictionary<string, object>
        {
            { "deviceId", deviceId },
            { "userName", entry.userName },
            { "gameplayNumber", entry.gameplayNumber },
            { "strikeRate", entry.strikeRate },
            { "battingAverage", entry.battingAverage },
            { "totalRuns", entry.totalRuns },
            { "totalBallsFaced", entry.totalBallsFaced },
            { "totalOuts", entry.totalOuts },
            { "attempts", entry.attempts },
            { "bestScore", entry.bestScore },
            { "wins", entry.wins },
            { "lastUpdated", FieldValue.ServerTimestamp }
        };

        db.Collection(LEADERBOARD_GAMEPLAY_COLLECTION)
            .Document(documentId)
            .SetAsync(leaderboardData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ Pending upload SUCCESS - Gameplay {entry.gameplayNumber}");
                    RemoveFromPendingQueue(entry.gameplayNumber);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Pending upload FAILED - Gameplay {entry.gameplayNumber}: {task.Exception?.Message}");
                    // Keep in queue for next retry
                }
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPLOAD GAMEPLAY STATS TO LEADERBOARD
    // ═══════════════════════════════════════════════════════════════
    
    public void UploadGameplayStats(int gameplayNumber, MatchStatistics stats)
    {
        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            return;
        }

        if (stats == null)
        {
            Debug.LogError("❌ MatchStatistics is null!");
            return;
        }

        string userName = GameManager.instance.currentSaveData.userName;
        
        float totalRuns = 0;
        float totalBalls = 0;
        int totalOuts = stats.TotalOuts;

        foreach (var attempt in stats.attempts)
        {
            totalRuns += attempt.runsScored;
            totalBalls += attempt.ballsFaced;
        }

        float strikeRate = totalBalls > 0 ? (totalRuns * 100f) / totalBalls : 0f;
        float battingAverage = totalOuts > 0 ? totalRuns / totalOuts : totalRuns;

        // ✅ Create pending entry (used for both immediate upload and queue)
        PendingUploadEntry entry = new PendingUploadEntry
        {
            gameplayNumber = gameplayNumber,
            userName = userName,
            strikeRate = strikeRate,
            battingAverage = battingAverage,
            totalRuns = totalRuns,
            totalBallsFaced = totalBalls,
            totalOuts = totalOuts,
            attempts = stats.TotalAttempts,
            bestScore = stats.BestScore,
            wins = stats.Wins,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        // ✅ Check internet - if no internet, queue for later
        if (!HasInternetConnection())
        {
            Debug.LogWarning($"⚠️ No internet - queuing Gameplay {gameplayNumber} for later upload");
            AddToPendingQueue(entry);
            return;
        }

        // ✅ Has internet - try to upload immediately
        string documentId = $"{deviceId}_{gameplayNumber}";

        Dictionary<string, object> leaderboardData = new Dictionary<string, object>
        {
            { "deviceId", deviceId },
            { "userName", userName },
            { "gameplayNumber", gameplayNumber },
            { "strikeRate", strikeRate },
            { "battingAverage", battingAverage },
            { "totalRuns", totalRuns },
            { "totalBallsFaced", totalBalls },
            { "totalOuts", totalOuts },
            { "attempts", stats.TotalAttempts },
            { "bestScore", stats.BestScore },
            { "wins", stats.Wins },
            { "lastUpdated", FieldValue.ServerTimestamp }
        };

        db.Collection(LEADERBOARD_GAMEPLAY_COLLECTION)
            .Document(documentId)
            .SetAsync(leaderboardData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log($"✅ Gameplay {gameplayNumber} leaderboard stats uploaded - SR: {strikeRate:F1}, BA: {battingAverage:F1}");
                    // ✅ Remove from pending queue if it was there
                    RemoveFromPendingQueue(gameplayNumber);
                }
                else
                {
                    Debug.LogWarning($"⚠️ Upload failed - queuing Gameplay {gameplayNumber} for retry: {task.Exception?.Message}");
                    // ✅ Add to pending queue for retry
                    AddToPendingQueue(entry);
                }
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // FETCH ALL RANKS FOR A GAMEPLAY
    // ═══════════════════════════════════════════════════════════════
    
    public void GetAllRanksForGameplay(int gameplayNumber, Action<GameplayRankData> onComplete)
    {
        GameplayRankData rankData = new GameplayRankData
        {
            gameplayNumber = gameplayNumber,
            strikeRateRank = -1,
            battingAverageRank = -1,
            strikeRate = 0f,
            battingAverage = 0f,
            hasPlayed = false,
            hasError = false,
            noInternet = false
        };

        if (!HasInternetConnection())
        {
            Debug.LogWarning($"⚠️ No internet - cannot fetch ranks for Gameplay {gameplayNumber}");
            rankData.noInternet = true;
            onComplete?.Invoke(rankData);
            return;
        }

        if (db == null)
        {
            Debug.LogError("❌ Firestore not initialized!");
            rankData.hasError = true;
            onComplete?.Invoke(rankData);
            return;
        }

        string userDocumentId = $"{deviceId}_{gameplayNumber}";
        Debug.Log($"🔍 Looking for user document: {userDocumentId}");

        db.Collection(LEADERBOARD_GAMEPLAY_COLLECTION)
            .Document(userDocumentId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"❌ Error fetching user document: {task.Exception}");
                    rankData.hasError = true;
                    onComplete?.Invoke(rankData);
                    return;
                }

                DocumentSnapshot userDoc = task.Result;

                if (!userDoc.Exists)
                {
                    Debug.Log($"📋 User document not found for Gameplay {gameplayNumber}");
                    rankData.hasPlayed = false;
                    onComplete?.Invoke(rankData);
                    return;
                }

                rankData.hasPlayed = true;
                
                try
                {
                    rankData.strikeRate = (float)userDoc.GetValue<double>("strikeRate");
                    rankData.battingAverage = (float)userDoc.GetValue<double>("battingAverage");
                    Debug.Log($"📊 User stats found - SR: {rankData.strikeRate:F1}, BA: {rankData.battingAverage:F1}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ Error parsing user stats: {e.Message}");
                    rankData.hasError = true;
                    onComplete?.Invoke(rankData);
                    return;
                }

                Debug.Log($"🔍 Fetching all players for Gameplay {gameplayNumber}...");
                
                db.Collection(LEADERBOARD_GAMEPLAY_COLLECTION)
                    .WhereEqualTo("gameplayNumber", gameplayNumber)
                    .GetSnapshotAsync()
                    .ContinueWithOnMainThread(allDocsTask =>
                    {
                        if (allDocsTask.IsFaulted)
                        {
                            Debug.LogError($"❌ Error fetching all players: {allDocsTask.Exception}");
                            rankData.hasError = true;
                            onComplete?.Invoke(rankData);
                            return;
                        }

                        QuerySnapshot allDocs = allDocsTask.Result;
                        Debug.Log($"📊 Found {allDocs.Count} total players for Gameplay {gameplayNumber}");

                        if (allDocs.Count == 0)
                        {
                            rankData.strikeRateRank = 1;
                            rankData.battingAverageRank = 1;
                            onComplete?.Invoke(rankData);
                            return;
                        }

                        int srPlayersAbove = 0;
                        int baPlayersAbove = 0;

                        foreach (DocumentSnapshot doc in allDocs.Documents)
                        {
                            try
                            {
                                float otherSR = (float)doc.GetValue<double>("strikeRate");
                                float otherBA = (float)doc.GetValue<double>("battingAverage");

                                if (otherSR > rankData.strikeRate)
                                    srPlayersAbove++;

                                if (otherBA > rankData.battingAverage)
                                    baPlayersAbove++;
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning($"⚠️ Error parsing document {doc.Id}: {e.Message}");
                            }
                        }

                        rankData.strikeRateRank = srPlayersAbove + 1;
                        rankData.battingAverageRank = baPlayersAbove + 1;

                        Debug.Log($"✅ Gameplay {gameplayNumber} Final Ranks - SR: #{rankData.strikeRateRank}, BA: #{rankData.battingAverageRank} (out of {allDocs.Count} players)");
                        onComplete?.Invoke(rankData);
                    });
            });
    }

    // ✅ Call this when app comes back to foreground or internet is restored
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Debug.Log("📱 App focused - checking pending uploads...");
            RetryPendingUploads();
        }
    }
}

// ═══════════════════════════════════════════════════════════════
// DATA CLASSES
// ═══════════════════════════════════════════════════════════════

[System.Serializable]
public class GameplayRankData
{
    public int gameplayNumber;
    public bool hasPlayed;
    public float strikeRate;
    public float battingAverage;
    public int strikeRateRank;
    public int battingAverageRank;
    public bool hasError;
    public bool noInternet;
}

[System.Serializable]
public class PendingUploadEntry
{
    public int gameplayNumber;
    public string userName;
    public float strikeRate;
    public float battingAverage;
    public float totalRuns;
    public float totalBallsFaced;
    public int totalOuts;
    public int attempts;
    public int bestScore;
    public int wins;
    public string timestamp;
}

[System.Serializable]
public class PendingUploadsData
{
    public List<PendingUploadEntry> uploads = new List<PendingUploadEntry>();
}