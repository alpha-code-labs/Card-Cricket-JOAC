using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    void Awake()
    {
        Instance = this;
    }
    int currentRuns = 0; // Current runs scored
    int TargetScore = 30; // The target score to reach
    int MaxBalls = 12; // Maximum balls in the game (e.g., 6 overs)
    [SerializeField] TextMeshProUGUI scoreText; // Text to display the score
    [SerializeField] TextMeshProUGUI ballsAndOversText; // Text to display balls and overs
    public void UpdateBallsAndOvers(int ballsBowled)
    {
        int overs = ballsBowled / 6;
        int balls = ballsBowled % 6;
        int ballDisplay = balls + 1;
        int overDisplay = overs + 1;
        int ballsRemain = MaxBalls - ballsBowled;
        ballsAndOversText.text = $"Ball {ballDisplay} of over {overDisplay}\n total balls remain {ballsRemain}";
    }
    public void UpdateScore(int runs)
    {
        currentRuns += runs;
        scoreText.text = "Score: " + currentRuns.ToString() + " / " + TargetScore.ToString();
        if (currentRuns >= TargetScore)
        {
            Debug.Log("Target Reached! You win!");
            // Logic to handle winning the game, e.g., showing a win screen
        }
    }
    public void PlayCard(BattingStrategy battingStrategy)
    {
        TimingManager.Instance.StartTimingMeter(battingStrategy);

    }
    void Start()
    {
        UpdateScore(0); // Initialize score display
        UpdateBallsAndOvers(0); // Initialize balls and overs display
    }
}
