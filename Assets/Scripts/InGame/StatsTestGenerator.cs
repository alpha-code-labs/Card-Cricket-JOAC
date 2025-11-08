// using System.Collections;
// using UnityEngine;

// public class StatsTestDataGenerator : MonoBehaviour
// {
//     [Header("Test Data Settings")]
//     [SerializeField] private bool generateOnStart = false;
//     [SerializeField] private bool clearExistingData = true;
//     [SerializeField] private int matchesToGenerate = 15;
//     [SerializeField] private int minAttemptsPerMatch = 1;
//     [SerializeField] private int maxAttemptsPerMatch = 5;
    
//     [ContextMenu("Generate Test Data for 15 Matches")]
//     public void GenerateTestData()
//     {
//         GenerateDummyStats(15);
//     }
    
//     [ContextMenu("Generate Test Data for 5 Matches")]
//     public void GenerateSmallTestData()
//     {
//         GenerateDummyStats(5);
//     }
    
//     [ContextMenu("Clear All Stats")]
//     public void ClearAllStats()
//     {
//         if (PlayerStatsTracker.Instance != null && PlayerStatsTracker.Instance.GetPlayerStats() != null)
//         {
//             PlayerStatsTracker.Instance.GetPlayerStats().ResetStats();
//             Debug.Log("All stats cleared!");
//         }
//     }
    
//     private void Start()
//     {
//         if (generateOnStart)
//         {
//             StartCoroutine(GenerateAfterDelay());
//         }
//     }
    
//     private IEnumerator GenerateAfterDelay()
//     {
//         yield return new WaitForSeconds(0.5f); // Wait for systems to initialize
//         GenerateDummyStats(matchesToGenerate);
//     }
    
//     public void GenerateDummyStats(int numberOfMatches)
//     {
//         if (PlayerStatsTracker.Instance == null)
//         {
//             Debug.LogError("PlayerStatsTracker Instance not found! Make sure it exists in the scene.");
//             return;
//         }
        
//         var playerStats = PlayerStatsTracker.Instance.GetPlayerStats();
//         if (playerStats == null)
//         {
//             Debug.LogError("PlayerStats is null!");
//             return;
//         }
        
//         if (clearExistingData)
//         {
//             playerStats.ResetStats();
//             Debug.Log("Cleared existing stats data");
//         }
        
//         // Match configurations based on your GameplayConfiguration
//         MatchConfig[] matchConfigs = new MatchConfig[]
//         {
//             new MatchConfig(1, "1989/01/31", 24, 113, 65),   // Batting first
//             new MatchConfig(2, "1989/02/01", 24, 110, 58),   // Batting first
//             new MatchConfig(3, "1989/02/02", 24, 103, 59),   // Chase
//             new MatchConfig(4, "1990/03/15", 48, 96, 0),     // Batting first
//             new MatchConfig(5, "1990/03/16", 48, 88, 0),     // Batting first
//             new MatchConfig(6, "1990/03/17", 48, 104, 0),    // Chase
//             new MatchConfig(7, "1990/04/11", 72, 144, 0),    // Batting first
//             new MatchConfig(8, "1990/04/12", 72, 156, 0),    // Batting first
//             new MatchConfig(9, "1990/04/13", 72, 180, 0),    // Chase
//             new MatchConfig(12, "1990/03/05", 36, 141, 75),  // Chase
//             new MatchConfig(13, "1990/03/06", 36, 135, 63),  // Chase
//             new MatchConfig(14, "1990/03/07", 36, 157, 79),  // Chase
//             new MatchConfig(15, "1990/03/28", 60, 110, 0),   // Chase
//             new MatchConfig(16, "1990/03/29", 60, 120, 0),   // Chase
//             new MatchConfig(17, "1990/03/30", 60, 130, 0),   // Chase
//         };
        
//         int matchCount = Mathf.Min(numberOfMatches, matchConfigs.Length);
        
//         for (int matchIndex = 0; matchIndex < matchCount; matchIndex++)
//         {
//             var config = matchConfigs[matchIndex];
//             int attempts = Random.Range(minAttemptsPerMatch, maxAttemptsPerMatch + 1);
            
//             Debug.Log($"Generating Match #{config.gameplayNumber} with {attempts} attempts");
            
//             for (int attemptIndex = 0; attemptIndex < attempts; attemptIndex++)
//             {
//                 GenerateMatchAttempt(config, attemptIndex);
//             }
//         }
        
//         Debug.Log($"✅ Generated dummy data for {matchCount} matches!");
//         Debug.Log($"Total attempts created: {GetTotalAttempts(playerStats)}");
        
//         // Save the generated data
//         #if UNITY_EDITOR
//         UnityEditor.EditorUtility.SetDirty(playerStats);
//         UnityEditor.AssetDatabase.SaveAssets();
//         #endif
//     }
    
//     private void GenerateMatchAttempt(MatchConfig config, int attemptIndex)
//     {
//         // Calculate realistic performance based on attempt number and target
//         float performanceMultiplier = GetPerformanceMultiplier(attemptIndex);
//         int targetRuns = config.targetScore - config.initialScore;
        
//         // Generate runs scored (with some randomness)
//         int baseRuns = Mathf.RoundToInt(targetRuns * performanceMultiplier);
//         int variance = Random.Range(-15, 20);
//         int runsScored = Mathf.Max(10, baseRuns + variance + config.initialScore);
        
//         // Determine if won (more likely in later attempts)
//         bool won = runsScored >= config.targetScore;
//         if (!won && attemptIndex >= 2)
//         {
//             // Give a chance to win in later attempts even with lower score
//             won = Random.Range(0f, 1f) < 0.3f;
//             if (won) runsScored = config.targetScore + Random.Range(0, 10);
//         }
        
//         // Calculate balls faced (realistic for the runs scored)
//         int ballsFaced = Mathf.Min(config.maxBalls, Random.Range(
//             Mathf.Max(10, runsScored / 2),
//             Mathf.Min(config.maxBalls, runsScored + 10)
//         ));
        
//         // Calculate boundaries based on runs
//         int totalBoundaryRuns = Mathf.Min(runsScored, Random.Range(runsScored / 3, runsScored / 2));
//         int sixes = Random.Range(0, Mathf.Min(8, totalBoundaryRuns / 6));
//         int remainingBoundaryRuns = totalBoundaryRuns - (sixes * 6);
//         int fours = Mathf.Min(15, remainingBoundaryRuns / 4);
        
//         // Wickets lost (fewer for winning attempts)
//         int wicketsLost = won ? Random.Range(0, 2) : Random.Range(1, 4);
        
//         // Record the attempt
//         PlayerStatsTracker.Instance.GetPlayerStats().RecordMatchAttempt(
//             config.gameplayNumber,
//             config.matchDate,
//             runsScored,
//             ballsFaced,
//             wicketsLost,
//             fours,
//             sixes,
//             won
//         );
        
//         Debug.Log($"  Attempt {attemptIndex + 1}: {runsScored} runs, {ballsFaced} balls, " +
//                  $"{fours} fours, {sixes} sixes, {wicketsLost} wickets, Won: {won}");
//     }
    
//     private float GetPerformanceMultiplier(int attemptIndex)
//     {
//         // Players generally improve with more attempts
//         float[] multipliers = { 0.6f, 0.75f, 0.85f, 0.95f, 1.05f };
        
//         if (attemptIndex >= multipliers.Length)
//             return 1.0f + (attemptIndex - multipliers.Length) * 0.05f;
            
//         return multipliers[attemptIndex];
//     }
    
//     private int GetTotalAttempts(PlayerStats stats)
//     {
//         int total = 0;
//         foreach (var match in stats.GetAllMatchStats())
//         {
//             total += match.TotalAttempts;
//         }
//         return total;
//     }
    
//     [System.Serializable]
//     private class MatchConfig
//     {
//         public int gameplayNumber;
//         public string matchDate;
//         public int maxBalls;
//         public int targetScore;
//         public int initialScore;
        
//         public MatchConfig(int number, string date, int balls, int target, int initial)
//         {
//             gameplayNumber = number;
//             matchDate = date;
//             maxBalls = balls;
//             targetScore = target;
//             initialScore = initial;
//         }
//     }
    
//     // Additional test methods for specific scenarios
    
//     [ContextMenu("Generate Perfect Performance Data")]
//     public void GeneratePerfectData()
//     {
//         if (clearExistingData)
//             ClearAllStats();
            
//         // Generate some perfect games
//         for (int i = 1; i <= 5; i++)
//         {
//             PlayerStatsTracker.Instance.GetPlayerStats().RecordMatchAttempt(
//                 i,                          // gameplayNumber
//                 $"1989/0{i}/01",           // date
//                 150 + i * 10,              // runs (high scores)
//                 60 + i * 5,                // balls faced
//                 0,                         // no wickets lost
//                 10 + i,                    // fours
//                 5 + i,                     // sixes
//                 true                       // won
//             );
//         }
        
//         Debug.Log("Generated perfect performance data!");
//     }
    
//     [ContextMenu("Generate Struggling Performance Data")]
//     public void GenerateStrugglingData()
//     {
//         if (clearExistingData)
//             ClearAllStats();
            
//         // Generate some poor performance games
//         for (int i = 1; i <= 5; i++)
//         {
//             // Multiple failed attempts
//             for (int attempt = 0; attempt < 3; attempt++)
//             {
//                 PlayerStatsTracker.Instance.GetPlayerStats().RecordMatchAttempt(
//                     i,                          // gameplayNumber
//                     $"1989/0{i}/01",           // date
//                     20 + Random.Range(0, 15),  // low runs
//                     30 + Random.Range(0, 10),  // balls faced
//                     2 + attempt,                // wickets lost
//                     Random.Range(0, 2),         // few fours
//                     0,                          // no sixes
//                     false                       // lost
//                 );
//             }
//         }
        
//         Debug.Log("Generated struggling performance data!");
//     }
    
//     [ContextMenu("Generate Mixed Realistic Data")]
//     public void GenerateMixedData()
//     {
//         ClearAllStats();
        
//         // Mix of good, average, and poor performances
//         GenerateDummyStats(10);           // Regular matches
//         GeneratePerfectData();             // Some perfect games
        
//         // Add some matches with many attempts (showing persistence)
//         for (int i = 11; i <= 13; i++)
//         {
//             int attempts = Random.Range(6, 10);
//             for (int a = 0; a < attempts; a++)
//             {
//                 bool finalAttemptWin = (a == attempts - 1) && Random.Range(0f, 1f) > 0.4f;
//                 int runs = finalAttemptWin ? 100 + Random.Range(0, 50) : Random.Range(30, 80);
                
//                 PlayerStatsTracker.Instance.GetPlayerStats().RecordMatchAttempt(
//                     i,
//                     $"1990/04/{10 + i}",
//                     runs,
//                     Random.Range(40, 70),
//                     Random.Range(0, 3),
//                     Random.Range(2, 8),
//                     Random.Range(0, 3),
//                     finalAttemptWin
//                 );
//             }
//         }
        
//         Debug.Log("Generated mixed realistic data!");
//     }
// }