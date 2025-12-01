using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class GameplayStatUI
{
    public int gameplayNumber;
    
    public TextMeshProUGUI matchText;
    public TextMeshProUGUI strikeRateText;
    public TextMeshProUGUI strikeRateRankText;
    public TextMeshProUGUI battingAverageText;
    public TextMeshProUGUI battingAverageRankText;
}

public class GameplayStatsDisplay : MonoBehaviour
{
    [SerializeField] private GameplayStatUI[] gameplayStats = new GameplayStatUI[15];
    
    // ✅ Error Handling UI (assign in Inspector)
    [Header("Error Handling UI")]
    [SerializeField] private GameObject noInternetPanel;
    [SerializeField] private TextMeshProUGUI errorMessageText;
    [SerializeField] private Button errorPanelCloseButton;

    void Start()
    {
        SetupGameplays();
        InitializeErrorPanel();
    }

    void InitializeErrorPanel()
    {
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
        
        if (errorPanelCloseButton != null)
            errorPanelCloseButton.onClick.AddListener(HideErrorPanel);
    }

    void SetupGameplays()
    {
        int[] gameplayMap = { 1, 2, 3, 12, 13, 14, 4, 5, 6, 15, 16, 17, 7, 8, 9 };
        
        for (int i = 0; i < gameplayMap.Length && i < gameplayStats.Length; i++)
        {
            gameplayStats[i].gameplayNumber = gameplayMap[i];
        }
        
        RefreshAllStats();
    }

    public void RefreshAllStats()
    {
        Debug.Log("🔄 Refreshing all gameplay stats...");
        
        // ✅ Hide error panel initially
        HideErrorPanel();
        
        // ✅ Check internet first - show local stats only if no internet
        if (!FirestoreGameplayLeaderboardManager.HasInternetConnection())
        {
            Debug.LogWarning("⚠️ No internet connection");
            ShowErrorPanel("No internet connection.\nRanks are unavailable.");
            SetAllStatsFromLocalOnly();
            return;
        }
        
        if (PlayerStatsTracker.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerStatsTracker not ready");
            SetAllToNA();
            return;
        }

        foreach (var stat in gameplayStats)
        {
            if (stat == null) continue;
            
            // ✅ Always set Match Text
            if (stat.matchText != null)
                stat.matchText.text = $"GamePlay-{stat.gameplayNumber}";
            
            var matchStats = PlayerStatsTracker.Instance.GetMatchStats(stat.gameplayNumber);
            
            if (matchStats == null || matchStats.TotalAttempts == 0)
            {
                SetDefaultStats(stat);
                continue;
            }

            // ✅ Calculate Strike Rate & Batting Average from local JSON data
            UpdateLocalStats(stat, matchStats);
            
            // ✅ Fetch Ranks from Firestore
            FetchRanksForGameplay(stat);
        }
    }

    void UpdateLocalStats(GameplayStatUI stat, MatchStatistics matchStats)
    {
        float totalRuns = 0;
        float totalBalls = 0;
        
        foreach (var attempt in matchStats.attempts)
        {
            totalRuns += attempt.runsScored;
            totalBalls += attempt.ballsFaced;
        }
        
        float strikeRate = totalBalls > 0 ? (totalRuns * 100f) / totalBalls : 0f;
        float battingAverage = matchStats.TotalOuts > 0 ? totalRuns / matchStats.TotalOuts : totalRuns;
        
        if (stat.strikeRateText != null)
            stat.strikeRateText.text = strikeRate.ToString("F1");
        
        if (stat.battingAverageText != null)
            stat.battingAverageText.text = battingAverage.ToString("F1");
        
        // ✅ Set ranks to "..." while loading
        if (stat.strikeRateRankText != null)
            stat.strikeRateRankText.text = "...";
        
        if (stat.battingAverageRankText != null)
            stat.battingAverageRankText.text = "...";

        Debug.Log($"✅ Gameplay {stat.gameplayNumber}: SR={strikeRate:F1}, BA={battingAverage:F1}");
    }

    void FetchRanksForGameplay(GameplayStatUI stat)
    {
        FirestoreGameplayLeaderboardManager leaderboardManager = FirestoreGameplayLeaderboardManager.GetInstance();
        
        if (leaderboardManager == null)
        {
            Debug.LogWarning("⚠️ FirestoreGameplayLeaderboardManager not found");
            SetRankAsError(stat);
            return;
        }

        leaderboardManager.GetAllRanksForGameplay(stat.gameplayNumber, (rankData) =>
        {
            // ✅ Handle no internet
            if (rankData.noInternet)
            {
                SetRankAsNoInternet(stat);
                return;
            }
            
            // ✅ Handle error fetching data
            if (rankData.hasError)
            {
                SetRankAsError(stat);
                return;
            }
            
            // ✅ Handle user hasn't played this gameplay
            if (!rankData.hasPlayed)
            {
                SetRankAsNA(stat);
                return;
            }

            // ✅ Success - Update Strike Rate Rank
            if (stat.strikeRateRankText != null)
            {
                stat.strikeRateRankText.text = rankData.strikeRateRank > 0 
                    ? $"#{rankData.strikeRateRank}" 
                    : "N/A";
            }

            // ✅ Success - Update Batting Average Rank
            if (stat.battingAverageRankText != null)
            {
                stat.battingAverageRankText.text = rankData.battingAverageRank > 0 
                    ? $"#{rankData.battingAverageRank}" 
                    : "N/A";
            }

            Debug.Log($"✅ Gameplay {stat.gameplayNumber} Ranks - SR: #{rankData.strikeRateRank}, BA: #{rankData.battingAverageRank}");
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // ERROR HANDLING UI
    // ═══════════════════════════════════════════════════════════════

    void ShowErrorPanel(string message)
    {
        if (noInternetPanel != null)
        {
            noInternetPanel.SetActive(true);
            
            if (errorMessageText != null)
                errorMessageText.text = message;
        }
        Debug.Log($"📵 Error Panel: {message}");
    }

    void HideErrorPanel()
    {
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);
    }

    void SetAllStatsFromLocalOnly()
    {
        if (PlayerStatsTracker.Instance == null)
        {
            SetAllToNA();
            return;
        }

        foreach (var stat in gameplayStats)
        {
            if (stat == null) continue;
            
            if (stat.matchText != null)
                stat.matchText.text = $"GamePlay-{stat.gameplayNumber}";
            
            var matchStats = PlayerStatsTracker.Instance.GetMatchStats(stat.gameplayNumber);
            
            if (matchStats == null || matchStats.TotalAttempts == 0)
            {
                SetDefaultStats(stat);
            }
            else
            {
                // ✅ Show local SR & BA values
                UpdateLocalStats(stat, matchStats);
                
                // ✅ Mark ranks as unavailable (no internet)
                SetRankAsNoInternet(stat);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SET STAT HELPERS
    // ═══════════════════════════════════════════════════════════════

    void SetDefaultStats(GameplayStatUI stat)
    {
        if (stat.strikeRateText != null)
            stat.strikeRateText.text = "N/A";
        
        if (stat.strikeRateRankText != null)
            stat.strikeRateRankText.text = "N/A";
        
        if (stat.battingAverageText != null)
            stat.battingAverageText.text = "N/A";
        
        if (stat.battingAverageRankText != null)
            stat.battingAverageRankText.text = "N/A";
    }

    // ✅ User hasn't played this gameplay
    void SetRankAsNA(GameplayStatUI stat)
    {
        if (stat.strikeRateRankText != null)
            stat.strikeRateRankText.text = "N/A";
        
        if (stat.battingAverageRankText != null)
            stat.battingAverageRankText.text = "N/A";
    }

    // ✅ No internet - show "--"
    void SetRankAsNoInternet(GameplayStatUI stat)
    {
        if (stat.strikeRateRankText != null)
            stat.strikeRateRankText.text = "--";
        
        if (stat.battingAverageRankText != null)
            stat.battingAverageRankText.text = "--";
    }

    // ✅ Error fetching data - show "ERR"
    void SetRankAsError(GameplayStatUI stat)
    {
        if (stat.strikeRateRankText != null)
            stat.strikeRateRankText.text = "ERR";
        
        if (stat.battingAverageRankText != null)
            stat.battingAverageRankText.text = "ERR";
    }

    void SetAllToNA()
    {
        foreach (var stat in gameplayStats)
        {
            if (stat != null)
            {
                if (stat.matchText != null)
                    stat.matchText.text = $"GamePlay-{stat.gameplayNumber}";
                    
                SetDefaultStats(stat);
            }
        }
    }

    void OnDestroy()
    {
        if (errorPanelCloseButton != null)
            errorPanelCloseButton.onClick.RemoveAllListeners();
    }
}