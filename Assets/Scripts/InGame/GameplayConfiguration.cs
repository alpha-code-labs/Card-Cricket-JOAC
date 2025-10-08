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

    public GameplayConfig(int number, string dateStr, int ballCount, int target = 0, PitchCondition pitchCondition = PitchCondition.Friendly)
    {
        gameplayNumber = number;
        date = dateStr;
        balls = ballCount;
        targetScore = target;
        isBattingFirst = (target == 0);
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
            {"1989/01/31", new GameplayConfig(1, "1989/01/31", 12, 0, PitchCondition.Friendly)}, // Batting first
            {"1989/02/01", new GameplayConfig(2, "1989/02/01", 24, 0, PitchCondition.Friendly)}, // Batting first
            {"1989/02/02", new GameplayConfig(3, "1989/02/02", 12, 15, PitchCondition.Friendly)}, // Chase 15 runs
            {"1990/03/15", new GameplayConfig(4, "1990/03/15", 60, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/03/16", new GameplayConfig(5, "1990/03/16", 60, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/03/17", new GameplayConfig(6, "1990/03/17", 60, 120, PitchCondition.Friendly)}, // Chase 120 runs
            {"1990/04/11", new GameplayConfig(7, "1990/04/11", 90, 0, PitchCondition.Friendly)}, // Batting first
            {"1990/04/12", new GameplayConfig(8, "1990/04/12", 30, 30, PitchCondition.Friendly)}, // Chase 30 runs
            {"1990/04/13", new GameplayConfig(9, "1990/04/13", 90, 185, PitchCondition.Hostile)} // Chase 185 runs
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