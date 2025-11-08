using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerStatsTracker : MonoBehaviour
{
    public static PlayerStatsTracker Instance;
    
    [Header("Stats Configuration")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private bool createNewStatsIfMissing = true;
    
    // Tracking current match attempt
    private int currentMatchRuns = 0;
    private int currentBallsFaced = 0;
    private int currentWicketsLost = 0;
    private int currentFours = 0;
    private int currentSixes = 0;
    private int currentGameplayNumber;
    private string currentMatchDate;
    private bool matchInProgress = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadOrCreateStats();
        }
        else
        {
            Destroy(gameObject);
        }
        
        if (playerStats != null)
        {
            var savedData = PlayerStatsSaveSystem.LoadStats();
            PlayerStatsSaveSystem.LoadIntoPlayerStats(playerStats, savedData);
        }
    }
    
    private void LoadOrCreateStats()
    {
        // Try to load existing stats from Resources
        playerStats = Resources.Load<PlayerStats>("PlayerStats");
        
        if (playerStats == null && createNewStatsIfMissing)
        {
            // Create new stats asset if it doesn't exist
            playerStats = ScriptableObject.CreateInstance<PlayerStats>();
            
            #if UNITY_EDITOR
            // Save the asset in editor
            string path = "Assets/Resources/PlayerStats.asset";
            UnityEditor.AssetDatabase.CreateAsset(playerStats, path);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("Created new PlayerStats asset at: " + path);
            #endif
        }
        
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats asset not found and could not be created!");
        }
    }
    
    public void StartTrackingMatch(int gameplayNumber, string matchDate)
    {
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats is null! Cannot track match.");
            return;
        }
        
        currentGameplayNumber = gameplayNumber;
        currentMatchDate = matchDate;
        currentMatchRuns = 0;
        currentBallsFaced = 0;
        currentWicketsLost = 0;
        currentFours = 0;
        currentSixes = 0;
        matchInProgress = true;
        
        Debug.Log($"Started tracking match: Gameplay #{gameplayNumber} - {matchDate}");
    }
    
    public void RecordRuns(int runs, bool ballFaced = true)
    {
        if (!matchInProgress) return;
        
        currentMatchRuns += runs;
        
        if (ballFaced)
        {
            currentBallsFaced++;
        }
        
        // Track boundaries
        if (runs == 4)
        {
            currentFours++;
        }
        else if (runs == 6)
        {
            currentSixes++;
        }
        
        Debug.Log($"Recorded {runs} runs. Total: {currentMatchRuns}, Balls: {currentBallsFaced}");
    }
    
    public void RecordWicketLost()
    {
        if (!matchInProgress) return;
        
        currentWicketsLost++;
        Debug.Log($"Wicket lost. Total wickets lost: {currentWicketsLost}");
    }

    public void EndMatch(bool won)
    {
        if (!matchInProgress || playerStats == null) return;

        // Record the match attempt
        playerStats.RecordMatchAttempt(
            currentGameplayNumber,
            currentMatchDate,
            currentMatchRuns,
            currentBallsFaced,
            currentWicketsLost,
            currentFours,
            currentSixes,
            won
        );

        matchInProgress = false;
        PlayerStatsSaveSystem.SaveStats(playerStats);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(playerStats);
        UnityEditor.AssetDatabase.SaveAssets();
#endif

        Debug.Log($"Match ended. Won: {won}, Runs: {currentMatchRuns}, Balls: {currentBallsFaced}, " +
                  $"Wickets: {currentWicketsLost}, 4s: {currentFours}, 6s: {currentSixes}");
    }
    
    public int GetTotalSixes(int gameplayNumber)
    {
        var stats = GetMatchStats(gameplayNumber);
        if (stats == null) return 0;
        
        int total = 0;
        foreach (var attempt in stats.attempts)
            total += attempt.sixes;
        return total;
    }

    public int GetTotalFours(int gameplayNumber)
    {
        var stats = GetMatchStats(gameplayNumber);
        if (stats == null) return 0;
        
        int total = 0;
        foreach (var attempt in stats.attempts)
            total += attempt.fours;
        return total;
    }    
    public MatchStatistics GetCurrentMatchStats()
    {
        if (playerStats == null) return null;
        return playerStats.GetMatchStats(currentGameplayNumber);
    }
    
    public MatchStatistics GetMatchStats(int gameplayNumber)
    {
        if (playerStats == null) return null;
        return playerStats.GetMatchStats(gameplayNumber);
    }
    
    public List<MatchStatistics> GetAllStats()
    {
        if (playerStats == null) return new List<MatchStatistics>();
        return playerStats.GetAllMatchStats();
    }
    
    public PlayerStats GetPlayerStats()
    {
        return playerStats;
    }
    
    // Helper method to get formatted stats for display
    public string GetFormattedMatchStats(int gameplayNumber)
    {
        var stats = GetMatchStats(gameplayNumber);
        if (stats == null || stats.TotalAttempts == 0)
        {
            return "No attempts yet";
        }
        
        return $"Attempts: {stats.TotalAttempts} | Wins: {stats.Wins} ({stats.WinRate:F1}%)\n" +
               $"Average: {stats.AverageRuns:F1} | Best: {stats.BestScore}\n" +
               $"Strike Rate: {stats.AverageStrikeRate:F1} | Most Boundaries: {stats.MostBoundaries}\n" +
               $"Total Outs: {stats.TotalOuts}";
    }
    
    public string GetFormattedCareerStats()
    {
        if (playerStats == null) return "No stats available";
        
        return $"Total Matches: {playerStats.TotalMatchesPlayed}\n" +
               $"Total Runs: {playerStats.TotalRuns}\n" +
               $"Career Best: {playerStats.CareerBestScore}\n" +
               $"Career Strike Rate: {playerStats.CareerStrikeRate:F1}\n" +
               $"Total Boundaries: {playerStats.TotalBoundaries}";
    }
}