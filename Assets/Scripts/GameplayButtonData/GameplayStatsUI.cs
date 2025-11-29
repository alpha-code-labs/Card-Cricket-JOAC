using UnityEngine;
using TMPro;

public class GameplayStatsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI matchText;
    [SerializeField] TextMeshProUGUI attemptsText;
    [SerializeField] TextMeshProUGUI averageScoreText;
    [SerializeField] TextMeshProUGUI bestScoreText;
    [SerializeField] TextMeshProUGUI sixes;
    [SerializeField] TextMeshProUGUI fours;
    [SerializeField] TextMeshProUGUI outText;
    
    private int gameplayNumber;

    void Start()
    {
        // ✅ AUTO-DETECT gameplay number from this button's GameplayButtonHandler
        GameplayButtonHandler handler = GetComponent<GameplayButtonHandler>();
        if (handler != null)
        {
            int detectedGameplayNum = ExtractGameplayNumber(handler);
            SetupStats(detectedGameplayNum);
            Debug.Log($"✅ GameplayStatsUI auto-setup for Gameplay {detectedGameplayNum}");
        }
        else
        {
            Debug.LogWarning("No GameplayButtonHandler found on this button!");
            SetDefaultStats();
        }
    }

    // ✅ Extract gameplay number from event name
    int ExtractGameplayNumber(GameplayButtonHandler handler)
    {
        // Get the event name from the button handler
        // We'll search the calendar to find what gameplay this corresponds to
        
        if (PlayerStatsTracker.Instance == null)
            return 0;
        
        // Get all stats to find matching gameplay
        var allStats = PlayerStatsTracker.Instance.GetAllStats();
        
        // For now, return based on button order (G-1=1, G-2=2, etc)
        // Or you can hardcode: G-1=1, G-2=2, G-3=3, etc.
        
        // Better approach: search calendar and find gameplay number
        return FindGameplayNumberFromButton();
    }

    // ✅ Find gameplay number based on button hierarchy position
    int FindGameplayNumberFromButton()
    {
        // Mapping of button order to gameplay number
        int buttonIndex = transform.GetSiblingIndex();
        
        // Map based on your button order in the scroll view
        int[] gameplayMap = { 1, 2, 3, 4, 5, 6, 7, 8, 15, 12, 13, 14, 9};
        
        if (buttonIndex >= 0 && buttonIndex < gameplayMap.Length)
        {
            return gameplayMap[buttonIndex];
        }
        
        return 0; // Default
    }

    public void SetupStats(int gameplayNum)
    {
        gameplayNumber = gameplayNum;
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (PlayerStatsTracker.Instance == null)
        {
            Debug.LogWarning($"⚠️ PlayerStatsTracker not ready");
            SetDefaultStats();
            return;
        }

        var matchStats = PlayerStatsTracker.Instance.GetMatchStats(gameplayNumber);
        
        if (matchStats == null)
        {
            Debug.Log($"📊 No stats yet for Gameplay {gameplayNumber}");
            SetDefaultStats();
            return;
        }

        // ✅ Update UI with stats
        matchText.text = $"Gameplay {gameplayNumber}";
        attemptsText.text = matchStats.TotalAttempts.ToString();
        averageScoreText.text = matchStats.AverageRuns.ToString("F1");
        bestScoreText.text = matchStats.BestScore.ToString();
        sixes.text = CalculateTotalSixes(matchStats).ToString();
        fours.text = CalculateTotalFours(matchStats).ToString();
        outText.text = matchStats.TotalOuts.ToString();

        Debug.Log($"✅ Updated Gameplay {gameplayNumber} stats: Attempts={matchStats.TotalAttempts}, Best={matchStats.BestScore}");
    }

    void SetDefaultStats()
    {
        matchText.text = $"Gameplay {gameplayNumber}";
        attemptsText.text = "0";
        averageScoreText.text = "0";
        bestScoreText.text = "0";
        sixes.text = "0";
        fours.text = "0";
        outText.text = "0";
    }

    private int CalculateTotalSixes(MatchStatistics stats)
    {
        int total = 0;
        foreach (var attempt in stats.attempts)
        {
            total += attempt.sixes;
        }
        return total;
    }

    private int CalculateTotalFours(MatchStatistics stats)
    {
        int total = 0;
        foreach (var attempt in stats.attempts)
        {
            total += attempt.fours;
        }
        return total;
    }
}