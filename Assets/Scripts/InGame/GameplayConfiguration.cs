using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameplayConfig
{
    public string date;
    public int gameplayNumber;
    public int balls;
    public int targetScore; // 0 means batting first
    public bool isBattingFirst;
    public PitchCondition pitchCondition;
    public int initialScore;
    public int winScore;

    public GameplayConfig(int number, string dateStr, int ballCount, int target = 0, int score = 0, int winScore = 0, PitchCondition pitchCondition = PitchCondition.Friendly)
    {
        gameplayNumber = number;
        date = dateStr;
        balls = ballCount;
        targetScore = target;
        isBattingFirst = (target == 0);
        initialScore = score;
        this.winScore = winScore;
        this.pitchCondition = pitchCondition;
    }
}

public class GameplayConfiguration : MonoBehaviour
{
    public static GameplayConfiguration Instance;

    private Dictionary<string, GameplayConfig> gameplayConfigs;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeConfigurations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

void InitializeConfigurations()
{
    gameplayConfigs = new Dictionary<string, GameplayConfig>
    {
        // Tutorial gameplay
        { "1988/07/23", new GameplayConfig(0, "1988/07/23", 6, 0, 0, 8, PitchCondition.Friendly) },

        // =========================
        // Updated as per new balls & runs mapping
        // {gameplay}->{balls},{runs}
        // 1->24,48 (batting first)
        // 2->24,52 (batting first)
        // 3->24,44 (Chase)
        // 4->48,96 (batting first)
        // 5->48,88 (batting first)
        // 6->48,104 (Chase)
        // 7->72,144 (batting first)
        // 8->72,156 (batting first)
        // 9->72,180 (Chase)
        // 12->36,66 (Chase)
        // 13->36,72 (Chase)
        // 14->36,78 (Chase)
        // 15->60,110 (Chase)
        // 16->60,120 (Chase)
        // 17->60,130 (Chase)
        // =========================

        // 1: Batting first, winScore = 65 + 48 = 113
        { "1989/01/31", new GameplayConfig(1, "1989/01/31", 24, 0, 65, 113, PitchCondition.Friendly) },

        // 2: Batting first, winScore = 58 + 52 = 110
        { "1989/02/01", new GameplayConfig(2, "1989/02/01", 24, 0, 58, 110, PitchCondition.Friendly) },

        // 3: Chasing 44 -> winScore = 59 + 44 = 103
        { "1989/02/02", new GameplayConfig(3, "1989/02/02", 24, 44, 59, 103, PitchCondition.Friendly) },

        // 4: Batting first, winScore = 0 + 96 = 96
        { "1990/03/15", new GameplayConfig(4, "1990/03/15", 48, 0, 0, 96, PitchCondition.Friendly) },

        // 5: Batting first, winScore = 0 + 88 = 88
        { "1990/03/16", new GameplayConfig(5, "1990/03/16", 48, 0, 0, 88, PitchCondition.Friendly) },

        // 6: Chasing 104 -> winScore = 0 + 104 = 104
        { "1990/03/17", new GameplayConfig(6, "1990/03/17", 48, 104, 0, 104, PitchCondition.Friendly) },

        // 7: Batting first, winScore = 0 + 144 = 144
        { "1990/04/11", new GameplayConfig(7, "1990/04/11", 72, 0, 0, 144, PitchCondition.Friendly) },

        // 8: Batting first, winScore = 0 + 156 = 156
        { "1990/04/12", new GameplayConfig(8, "1990/04/12", 72, 0, 0, 156, PitchCondition.Friendly) },

        // 9: Chasing 180 -> winScore = 0 + 180 = 180
        { "1990/04/13", new GameplayConfig(9, "1990/04/13", 72, 180, 0, 180, PitchCondition.Friendly) },

        // 12: Chasing 66 -> winScore = 75 + 66 = 141
        { "1990/03/05", new GameplayConfig(12, "1990/03/05", 36, 66, 75, 141, PitchCondition.Friendly) },

        // 13: Chasing 72 -> winScore = 63 + 72 = 135
        { "1990/03/06", new GameplayConfig(13, "1990/03/06", 36, 72, 63, 135, PitchCondition.Friendly) },

        // 14: Chasing 78 -> winScore = 79 + 78 = 157
        { "1990/03/07", new GameplayConfig(14, "1990/03/07", 36, 78, 79, 157, PitchCondition.Friendly) },

        // 15: Chasing 110 -> winScore = 0 + 110 = 110
        { "1990/03/28", new GameplayConfig(15, "1990/03/28", 60, 110, 0, 110, PitchCondition.Friendly) },

        // 16: Chasing 120 -> winScore = 0 + 120 = 120
        { "1990/03/29", new GameplayConfig(16, "1990/03/29", 60, 120, 0, 120, PitchCondition.Friendly) },

        // 17: Chasing 130 -> winScore = 0 + 130 = 130
        { "1990/03/30", new GameplayConfig(17, "1990/03/30", 60, 130, 0, 130, PitchCondition.Friendly) }
    };
}

    public GameplayConfig GetConfigForDate(string date)
    {
        if (gameplayConfigs.ContainsKey(date))
        {
            return gameplayConfigs[date];
        }

        Debug.LogError($"No gameplay configuration found for date: {date}");
        return null;
    }

    public GameplayConfig GetCurrentGameplayConfig()
    {
        if (NewDayManager.currentDateRecord != null)
        {
            string currentDate = NewDayManager.currentDateRecord.date;
            return GetConfigForDate(currentDate);
        }

        Debug.LogError("No current date record available!");
        return null;
    }
}