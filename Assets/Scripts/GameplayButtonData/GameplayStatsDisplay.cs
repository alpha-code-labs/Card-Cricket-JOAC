using UnityEngine;
using TMPro;

[System.Serializable]
public class GameplayStatUI
{
    public int gameplayNumber;
    public TextMeshProUGUI matchText;
    public TextMeshProUGUI attemptsText;
    public TextMeshProUGUI averageScoreText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI sixesText;
    public TextMeshProUGUI foursText;
    public TextMeshProUGUI outText;
}

public class GameplayStatsDisplay : MonoBehaviour
{
    [SerializeField] private GameplayStatUI[] gameplayStats = new GameplayStatUI[19];

    void Start()
    {
        // ✅ Setup all gameplays
        SetupGameplays();
    }

    void SetupGameplays()
    {
        // Mapping: G-1=1, G-2=2, ... G-8=8, G-12=12, ... G-17=17
        int[] gameplayMap = { 1, 2, 3, 12, 13, 14, 4, 5,6, 15, 16, 17, 7,8,9};
        
        for (int i = 0; i < gameplayMap.Length; i++)
        {
            gameplayStats[i].gameplayNumber = gameplayMap[i];
        }
        
        RefreshAllStats();
    }

    public void RefreshAllStats()
    {
        Debug.Log("🔄 Refreshing all gameplay stats...");
        
        if (PlayerStatsTracker.Instance == null)
        {
            Debug.LogWarning("⚠️ PlayerStatsTracker not ready");
            return;
        }

        foreach (var stat in gameplayStats)
        {
            if (stat == null) continue;
            
            var matchStats = PlayerStatsTracker.Instance.GetMatchStats(stat.gameplayNumber);
            
            if (matchStats == null)
            {
                SetDefaultStats(stat);
                continue;
            }

            // ✅ Update UI for this gameplay
            stat.matchText.text = $"Gameplay {stat.gameplayNumber}";
            stat.attemptsText.text = matchStats.TotalAttempts.ToString();
            stat.averageScoreText.text = matchStats.AverageRuns.ToString("F1");
            stat.bestScoreText.text = matchStats.BestScore.ToString();
            stat.sixesText.text = CalculateTotalSixes(matchStats).ToString();
            stat.foursText.text = CalculateTotalFours(matchStats).ToString();
            stat.outText.text = matchStats.TotalOuts.ToString();

            Debug.Log($"✅ Updated Gameplay {stat.gameplayNumber}: Attempts={matchStats.TotalAttempts}");
        }
    }

    void SetDefaultStats(GameplayStatUI stat)
    {
        stat.matchText.text = $"Gameplay {stat.gameplayNumber}";
        stat.attemptsText.text = "0";
        stat.averageScoreText.text = "0";
        stat.bestScoreText.text = "0";
        stat.sixesText.text = "0";
        stat.foursText.text = "0";
        stat.outText.text = "0";
    }

    private int CalculateTotalSixes(MatchStatistics stats)
    {
        int total = 0;
        foreach (var attempt in stats.attempts)
            total += attempt.sixes;
        return total;
    }

    private int CalculateTotalFours(MatchStatistics stats)
    {
        int total = 0;
        foreach (var attempt in stats.attempts)
            total += attempt.fours;
        return total;
    }
}