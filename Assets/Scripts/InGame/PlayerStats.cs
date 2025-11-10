using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchAttempt
{
    public int attemptNumber;
    public int runsScored;
    public int ballsFaced;
    public int wicketsLost;
    public int boundaries; // 4s and 6s combined
    public int fours;
    public int sixes;
    public bool wonMatch;
    public DateTime attemptDate;
    public float strikeRate;

    public MatchAttempt(int attempt, int runs, int balls, int wickets, int fours, int sixes, bool won)
    {
        attemptNumber = attempt;
        runsScored = runs;
        ballsFaced = balls;
        wicketsLost = wickets;
        this.fours = fours;
        this.sixes = sixes;
        boundaries = fours + sixes;
        wonMatch = won;
        attemptDate = DateTime.Now;
        strikeRate = balls > 0 ? (runs * 100f) / balls : 0f;
    }
}

[System.Serializable]
public class MatchStatistics
{
    public int gameplayNumber;
    public string matchDate;
    public List<MatchAttempt> attempts = new List<MatchAttempt>();
    
    // Aggregated stats
    public float AverageRuns => CalculateAverageRuns();
    public int BestScore => CalculateBestScore();
    public float AverageStrikeRate => CalculateAverageStrikeRate();
    public int MostBoundaries => CalculateMostBoundaries();
    public int TotalOuts => CalculateTotalOuts();
    public int TotalAttempts => attempts.Count;
    public int Wins => CalculateWins();
    public float WinRate => TotalAttempts > 0 ? (Wins * 100f) / TotalAttempts : 0f;

    public MatchStatistics(int gameplayNum, string date)
    {
        gameplayNumber = gameplayNum;
        matchDate = date;
    }

    private float CalculateAverageRuns()
    {
        if (attempts.Count == 0) return 0;
        float total = 0;
        foreach (var attempt in attempts)
        {
            total += attempt.runsScored;
        }
        return total / attempts.Count;
    }

    private int CalculateBestScore()
    {
        int best = 0;
        foreach (var attempt in attempts)
        {
            if (attempt.runsScored > best)
                best = attempt.runsScored;
        }
        return best;
    }

    private float CalculateAverageStrikeRate()
    {
        if (attempts.Count == 0) return 0;
        float total = 0;
        foreach (var attempt in attempts)
        {
            total += attempt.strikeRate;
        }
        return total / attempts.Count;
    }

    private int CalculateMostBoundaries()
    {
        int most = 0;
        foreach (var attempt in attempts)
        {
            if (attempt.boundaries > most)
                most = attempt.boundaries;
        }
        return most;
    }

    private int CalculateTotalOuts()
    {
        int total = 0;
        foreach (var attempt in attempts)
        {
            total += attempt.wicketsLost;
        }
        return total;
    }

    private int CalculateWins()
    {
        int wins = 0;
        foreach (var attempt in attempts)
        {
            if (attempt.wonMatch) wins++;
        }
        return wins;
    }

    public void AddAttempt(MatchAttempt attempt)
    {
        attempts.Add(attempt);
    }
}

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Cricket/Player Stats")]
public class PlayerStats : ScriptableObject
{
    [SerializeField] private Dictionary<int, MatchStatistics> matchStats = new Dictionary<int, MatchStatistics>();
    [SerializeField] private List<MatchStatistics> serializedMatchStats = new List<MatchStatistics>(); // For Unity serialization
    
    // Overall career stats
    public int TotalMatchesPlayed => matchStats.Count;
    public int TotalRuns => CalculateTotalRuns();
    public int TotalBoundaries => CalculateTotalBoundaries();
    public float CareerStrikeRate => CalculateCareerStrikeRate();
    public int CareerBestScore => CalculateCareerBest();
    
    private void OnEnable()
    {
        // Convert serialized list back to dictionary
        matchStats.Clear();
        foreach (var stat in serializedMatchStats)
        {
            matchStats[stat.gameplayNumber] = stat;
        }
    }
    
    private void OnDisable()
    {
        // Convert dictionary to serialized list for saving
        serializedMatchStats.Clear();
        foreach (var kvp in matchStats)
        {
            serializedMatchStats.Add(kvp.Value);
        }
    }

    public void RecordMatchAttempt(int gameplayNumber, string matchDate, int runs, int ballsFaced, 
        int wicketsLost, int fours, int sixes, bool won)
    {
        if (!matchStats.ContainsKey(gameplayNumber))
        {
            matchStats[gameplayNumber] = new MatchStatistics(gameplayNumber, matchDate);
        }

        int attemptNumber = matchStats[gameplayNumber].attempts.Count + 1;
        var attempt = new MatchAttempt(attemptNumber, runs, ballsFaced, wicketsLost, fours, sixes, won);
        matchStats[gameplayNumber].AddAttempt(attempt);
        
        // Update serialized list
        OnDisable();
        OnEnable();
    }

    public MatchStatistics GetMatchStats(int gameplayNumber)
    {
        return matchStats.ContainsKey(gameplayNumber) ? matchStats[gameplayNumber] : null;
    }

    public List<MatchStatistics> GetAllMatchStats()
    {
        return new List<MatchStatistics>(matchStats.Values);
    }

    private int CalculateTotalRuns()
    {
        int total = 0;
        foreach (var match in matchStats.Values)
        {
            foreach (var attempt in match.attempts)
            {
                total += attempt.runsScored;
            }
        }
        return total;
    }

    private int CalculateTotalBoundaries()
    {
        int total = 0;
        foreach (var match in matchStats.Values)
        {
            foreach (var attempt in match.attempts)
            {
                total += attempt.boundaries;
            }
        }
        return total;
    }

    private float CalculateCareerStrikeRate()
    {
        float totalRuns = 0;
        float totalBalls = 0;
        foreach (var match in matchStats.Values)
        {
            foreach (var attempt in match.attempts)
            {
                totalRuns += attempt.runsScored;
                totalBalls += attempt.ballsFaced;
            }
        }
        return totalBalls > 0 ? (totalRuns * 100f) / totalBalls : 0f;
    }

    private int CalculateCareerBest()
    {
        int best = 0;
        foreach (var match in matchStats.Values)
        {
            if (match.BestScore > best)
                best = match.BestScore;
        }
        return best;
    }

    public void ResetStats()
    {
        matchStats.Clear();
        serializedMatchStats.Clear();
    }
}