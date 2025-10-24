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

    public GameplayConfig(int number, string dateStr, int ballCount, int target = 0, int score = 0, PitchCondition pitchCondition = PitchCondition.Friendly)
    {
        gameplayNumber = number;
        date = dateStr;
        balls = ballCount;
        targetScore = target;
        isBattingFirst = (target == 0);
        initialScore = score;
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
            //Tutorial gameplay
            { "1988/07/23", new GameplayConfig(0, "1988/07/23", 6, 0, 0,  PitchCondition.Friendly) }, // Tutorial gameplay
            // Original gameplays (1-9)
            {"1989/01/31", new GameplayConfig(1, "1989/01/31", 12, 0, 65, PitchCondition.Friendly)}, // Batting first
            {"1989/02/01", new GameplayConfig(2, "1989/02/01", 24, 0, 58, PitchCondition.Friendly)}, // Batting first
            {"1989/02/02", new GameplayConfig(3, "1989/02/02", 12, 15, 59, PitchCondition.Friendly)}, // Chase 15 runs
            {"1990/03/15", new GameplayConfig(4, "1990/03/15", 60, 0, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/03/16", new GameplayConfig(5, "1990/03/16", 60, 0, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/03/17", new GameplayConfig(6, "1990/03/17", 60, 120, 0, PitchCondition.Friendly)}, // Chase 120 runs
            {"1990/04/11", new GameplayConfig(7, "1990/04/11", 90, 0, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/04/12", new GameplayConfig(8, "1990/04/12", 90, 90, 0, PitchCondition.Friendly)}, // Chase 30 runs
            {"1990/04/13", new GameplayConfig(9, "1990/04/13", 90, 185, 0, PitchCondition.Hostile)}, // Chase 185 runs
            
            // New gameplays (12-17)
            {"1990/03/05", new GameplayConfig(12, "1990/03/05", 15, 0, 75, PitchCondition.Friendly)}, // Batting first
            {"1990/03/06", new GameplayConfig(13, "1990/03/06", 20, 40, 63, PitchCondition.Friendly)}, // Chase 40 runs
            {"1990/03/07", new GameplayConfig(14, "1990/03/07", 15, 35, 79, PitchCondition.Friendly)}, // Chase 35 runs
            {"1990/03/28", new GameplayConfig(15, "1990/03/28", 40, 0, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/03/29", new GameplayConfig(16, "1990/03/29", 50, 110, 0, PitchCondition.Friendly)}, // Chase 110 runs
            {"1990/03/30", new GameplayConfig(17, "1990/03/30", 40, 100, 0, PitchCondition.Friendly)} // Chase 100 runs
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