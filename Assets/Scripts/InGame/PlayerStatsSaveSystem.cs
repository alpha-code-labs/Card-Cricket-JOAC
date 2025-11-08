using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerStatsData
{
    public List<MatchStatisticsData> matchStats = new List<MatchStatisticsData>();
    
    [System.Serializable]
    public class MatchStatisticsData
    {
        public int gameplayNumber;
        public string matchDate;
        public List<MatchAttemptData> attempts = new List<MatchAttemptData>();
    }
    
    [System.Serializable]
    public class MatchAttemptData
    {
        public int attemptNumber;
        public int runsScored;
        public int ballsFaced;
        public int wicketsLost;
        public int fours;
        public int sixes;
        public bool wonMatch;
        public string attemptDate;
    }
}

public class PlayerStatsSaveSystem : MonoBehaviour
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "playerstats.json");
    private static string BackupPath => Path.Combine(Application.persistentDataPath, "playerstats_backup.json");
    
    public static void SaveStats(PlayerStats stats)
    {
        try
        {
            // Create backup of existing save
            if (File.Exists(SavePath))
            {
                File.Copy(SavePath, BackupPath, true);
            }
            
            PlayerStatsData data = ConvertToSaveData(stats);
            string json = JsonUtility.ToJson(data, true);
            
            File.WriteAllText(SavePath, json);
            Debug.Log($"Player stats saved successfully to: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save player stats: {e.Message}");
            
            // Restore backup if save failed
            if (File.Exists(BackupPath))
            {
                File.Copy(BackupPath, SavePath, true);
                Debug.Log("Restored backup save file");
            }
        }
    }
    
    public static PlayerStatsData LoadStats()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                PlayerStatsData data = JsonUtility.FromJson<PlayerStatsData>(json);
                Debug.Log("Player stats loaded successfully");
                return data;
            }
            else
            {
                Debug.Log("No save file found, creating new stats");
                return new PlayerStatsData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load player stats: {e.Message}");
            
            // Try to load backup
            if (File.Exists(BackupPath))
            {
                try
                {
                    string json = File.ReadAllText(BackupPath);
                    PlayerStatsData data = JsonUtility.FromJson<PlayerStatsData>(json);
                    Debug.Log("Loaded backup save file");
                    return data;
                }
                catch
                {
                    Debug.LogError("Backup file also corrupted");
                }
            }
            
            return new PlayerStatsData();
        }
    }
    
    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("Save file deleted");
            }
            
            if (File.Exists(BackupPath))
            {
                File.Delete(BackupPath);
                Debug.Log("Backup file deleted");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to delete save files: {e.Message}");
        }
    }
    
    private static PlayerStatsData ConvertToSaveData(PlayerStats stats)
    {
        PlayerStatsData data = new PlayerStatsData();
        
        foreach (var matchStat in stats.GetAllMatchStats())
        {
            var matchData = new PlayerStatsData.MatchStatisticsData
            {
                gameplayNumber = matchStat.gameplayNumber,
                matchDate = matchStat.matchDate
            };
            
            foreach (var attempt in matchStat.attempts)
            {
                var attemptData = new PlayerStatsData.MatchAttemptData
                {
                    attemptNumber = attempt.attemptNumber,
                    runsScored = attempt.runsScored,
                    ballsFaced = attempt.ballsFaced,
                    wicketsLost = attempt.wicketsLost,
                    fours = attempt.fours,
                    sixes = attempt.sixes,
                    wonMatch = attempt.wonMatch,
                    attemptDate = attempt.attemptDate.ToString("yyyy-MM-dd HH:mm:ss")
                };
                
                matchData.attempts.Add(attemptData);
            }
            
            data.matchStats.Add(matchData);
        }
        
        return data;
    }
    
    public static void LoadIntoPlayerStats(PlayerStats stats, PlayerStatsData data)
    {
        if (stats == null || data == null) return;
        
        stats.ResetStats();
        
        foreach (var matchData in data.matchStats)
        {
            foreach (var attemptData in matchData.attempts)
            {
                stats.RecordMatchAttempt(
                    matchData.gameplayNumber,
                    matchData.matchDate,
                    attemptData.runsScored,
                    attemptData.ballsFaced,
                    attemptData.wicketsLost,
                    attemptData.fours,
                    attemptData.sixes,
                    attemptData.wonMatch
                );
            }
        }
        
        Debug.Log($"Loaded {data.matchStats.Count} matches into PlayerStats");
    }
}