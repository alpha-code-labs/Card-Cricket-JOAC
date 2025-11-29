using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class PlayerStatsTableUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private PauseGame pauseGame;
    [SerializeField] private GameObject handCards;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private Button closeButton;
    
    [Header("Table Components")]
    [SerializeField] private Transform tableContainer;
    [SerializeField] private GameObject tableHeaderRow;
    [SerializeField] private GameObject matchRowPrefab;
    [SerializeField] private GameObject careerTotalRow;
    [SerializeField] private ScrollRect scrollView;
    
    [Header("Column Headers")]
    [SerializeField] private TextMeshProUGUI matchNumberHeader;
    [SerializeField] private TextMeshProUGUI attemptsHeader;
    [SerializeField] private TextMeshProUGUI avgScoreHeader;
    [SerializeField] private TextMeshProUGUI bestScoreHeader;
    [SerializeField] private TextMeshProUGUI sixesHeader;
    [SerializeField] private TextMeshProUGUI foursHeader;
    [SerializeField] private TextMeshProUGUI outsHeader;
    
    [Header("Career Total Fields")]
    [SerializeField] private TextMeshProUGUI careerAttemptsText;
    [SerializeField] private TextMeshProUGUI careerAvgScoreText;
    [SerializeField] private TextMeshProUGUI careerBestScoreText;
    [SerializeField] private TextMeshProUGUI careerSixesText;
    [SerializeField] private TextMeshProUGUI careerFoursText;
    [SerializeField] private TextMeshProUGUI careerOutsText;
    
    [Header("Additional Career Stats (Add these to your UI)")]
    [SerializeField] private TextMeshProUGUI careerTotalRunsText;
    [SerializeField] private TextMeshProUGUI careerTotalBallsText;
    [SerializeField] private TextMeshProUGUI careerStrikeRateText;
    [SerializeField] private TextMeshProUGUI careerBattingAverageText;
    
    [Header("Visual Settings")]
    [SerializeField] private Color evenRowColor = new Color(0.95f, 0.95f, 0.95f, 1f);
    [SerializeField] private Color oddRowColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(0.9f, 0.95f, 1f, 1f);
    [SerializeField] private Color goodPerformanceColor = new Color(0.8f, 1f, 0.8f, 1f);
    [SerializeField] private Color poorPerformanceColor = new Color(1f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color unplayedGameColor = new Color(0.9f, 0.9f, 0.9f, 0.7f);
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float rowAnimationDelay = 0.05f;
    
    private PlayerStatsTracker statsTracker;
    private List<MatchStatistics> allMatchStats;
    private List<GameObject> matchRows = new List<GameObject>();
    
    private class MatchRowData
    {
        public GameObject rowObject;
        public MatchStatistics stats;
        public int actualGameplayNumber;  // The real gameplay number from GameplayConfiguration
        public int displayNumber;          // The number shown to the player
        public string gameplayDate;
        public bool isPlayed;
        public int totalSixes;
        public int totalFours;
        public int totalOuts;
        
        public TextMeshProUGUI matchNumberText;
        public TextMeshProUGUI attemptsText;
        public TextMeshProUGUI avgScoreText;
        public TextMeshProUGUI bestScoreText;
        public TextMeshProUGUI sixesText;
        public TextMeshProUGUI foursText;
        public TextMeshProUGUI outsText;
        public Image backgroundImage;
        public Button expandButton;
    }
    
    private List<MatchRowData> rowDataList = new List<MatchRowData>();
    
    // Mapping: Display Number -> (Actual Gameplay Number, Date)
    // Based on: 1->1, 2->2, 3->3, 12->4, 13->5, 14->6, 4->7, 5->8, 6->9, 15->10, 16->11, 17->12, 7->13, 8->14, 9->15
    private readonly List<(int actualGameplay, int displayNumber, string date)> gameplayOrder = new List<(int, int, string)>
    {
        (1, 1, "1989/01/31"),   // Display 1 -> Actual 1
        (2, 2, "1989/02/01"),   // Display 2 -> Actual 2
        (3, 3, "1989/02/02"),   // Display 3 -> Actual 3
        (12, 4, "1990/03/05"),  // Display 4 -> Actual 12
        (13, 5, "1990/03/06"),  // Display 5 -> Actual 13
        (14, 6, "1990/03/07"),  // Display 6 -> Actual 14
        (4, 7, "1990/03/15"),   // Display 7 -> Actual 4
        (5, 8, "1990/03/16"),   // Display 8 -> Actual 5
        (6, 9, "1990/03/17"),   // Display 9 -> Actual 6
        (15, 10, "1990/03/28"), // Display 10 -> Actual 15
        (16, 11, "1990/03/29"), // Display 11 -> Actual 16
        (17, 12, "1990/03/30"), // Display 12 -> Actual 17
        (7, 13, "1990/04/11"),  // Display 13 -> Actual 7
        (8, 14, "1990/04/12"),  // Display 14 -> Actual 8
        (9, 15, "1990/04/13")   // Display 15 -> Actual 9
    };
    
    private void Start()
    {
        statsTracker = PlayerStatsTracker.Instance;
        
        if (statsPanel != null)
            statsPanel.SetActive(false);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(HideStatsPanel);
    }
    
    public void ShowStatsPanel()
    {
        if (statsTracker == null)
        {
            Debug.LogError("PlayerStatsTracker instance not found!");
            return;
        }

        // pauseGame.Pause();
        // Timer.Instance.PauseTimer();
        CardPlayAnimationController.Instance.StopCommentary();
        handCards.SetActive(false);
        
        statsPanel.SetActive(true);
        RefreshTable();
        AnimatePanelIn();
        Time.timeScale = 0f;
    }
    
    public void HideStatsPanel()
    {
        // Timer.Instance.ResumeTimer();
        handCards.SetActive(true);
        AnimatePanelOut(() => statsPanel.SetActive(false));
        // pauseGame.Resume();
        Time.timeScale = 1f;
    }
    
    private void RefreshTable()
    {
        // Clear existing rows
        foreach (var row in matchRows)
        {
            Destroy(row);
        }
        matchRows.Clear();
        rowDataList.Clear();
        
        // Get all match stats
        allMatchStats = statsTracker.GetAllStats();
        
        // Create rows for ALL configured gameplays in the specified order
        CreateAllGameplayRows();
        
        // Update career totals with enhanced statistics
        UpdateCareerTotals();
    }
    
    private void CreateAllGameplayRows()
    {
        int rowIndex = 0;
        
        // Create rows in the fixed order defined by gameplayOrder
        foreach (var (actualGameplayNum, displayNum, date) in gameplayOrder)
        {
            GameObject newRow = Instantiate(matchRowPrefab, tableContainer);
            matchRows.Add(newRow);

            if(newRow == null)
            {
                Debug.LogError("Match Row Prefab is not assigned or could not be instantiated.");
                continue;
            }

            MatchRowData rowData = new MatchRowData();
            rowData.rowObject = newRow;
            rowData.actualGameplayNumber = actualGameplayNum;
            rowData.displayNumber = displayNum;
            rowData.gameplayDate = date;
            
            // Check if this gameplay has been played
            MatchStatistics matchStat = allMatchStats?.FirstOrDefault(m => m.gameplayNumber == actualGameplayNum);
            rowData.stats = matchStat;
            rowData.isPlayed = (matchStat != null && matchStat.TotalAttempts > 0);
            
            // Get UI components from the prefab
            rowData.matchNumberText = newRow.transform.Find("Background Image/HeaderCells/MatchNumber")?.GetComponent<TextMeshProUGUI>();
            rowData.attemptsText = newRow.transform.Find("Background Image/HeaderCells/Attempts")?.GetComponent<TextMeshProUGUI>();
            rowData.avgScoreText = newRow.transform.Find("Background Image/HeaderCells/AvgScore")?.GetComponent<TextMeshProUGUI>();
            rowData.bestScoreText = newRow.transform.Find("Background Image/HeaderCells/BestScore")?.GetComponent<TextMeshProUGUI>();
            rowData.sixesText = newRow.transform.Find("Background Image/HeaderCells/Sixes")?.GetComponent<TextMeshProUGUI>();
            rowData.foursText = newRow.transform.Find("Background Image/HeaderCells/Fours")?.GetComponent<TextMeshProUGUI>();
            rowData.outsText = newRow.transform.Find("Background Image/HeaderCells/Outs")?.GetComponent<TextMeshProUGUI>();
            rowData.backgroundImage = newRow.GetComponent<Image>();
            
            // Check if any required components are missing
            if (rowData.matchNumberText == null || 
                rowData.attemptsText == null || 
                rowData.avgScoreText == null || 
                rowData.bestScoreText == null || 
                rowData.sixesText == null || 
                rowData.foursText == null || 
                rowData.outsText == null)
            {
                Debug.LogError("Match Row Prefab is missing required UI components.");
                continue;
            }
            
            // Set the display number (not the actual gameplay number)
            rowData.matchNumberText.text = $"#{displayNum}";
            
            if (rowData.isPlayed)
            {
                // Calculate totals for played games
                rowData.totalSixes = CalculateTotalSixes(matchStat);
                rowData.totalFours = CalculateTotalFours(matchStat);
                rowData.totalOuts = matchStat.TotalOuts;
                
                // Display actual stats
                rowData.attemptsText.text = matchStat.TotalAttempts.ToString();
                rowData.avgScoreText.text = matchStat.AverageRuns.ToString("F1");
                rowData.bestScoreText.text = matchStat.BestScore.ToString();
                rowData.sixesText.text = rowData.totalSixes.ToString();
                rowData.foursText.text = rowData.totalFours.ToString();
                rowData.outsText.text = rowData.totalOuts.ToString();
                
                // Set row color with performance tinting
                Color baseColor = (rowIndex % 2 == 0) ? evenRowColor : oddRowColor;
                
                // Apply performance-based tinting
                if (matchStat.WinRate > 75f)
                {
                    baseColor = Color.Lerp(baseColor, goodPerformanceColor, 0.3f);
                }
                else if (matchStat.WinRate < 25f && matchStat.TotalAttempts > 0)
                {
                    baseColor = Color.Lerp(baseColor, poorPerformanceColor, 0.3f);
                }
                
                if (rowData.backgroundImage != null)
                {
                    rowData.backgroundImage.color = baseColor;
                }
                
                // Add hover effect
                AddHoverEffect(newRow, baseColor);
                
                // Optional: Add expand button for detailed view
                Button expandBtn = newRow.GetComponentInChildren<Button>();
                if (expandBtn != null)
                {
                    rowData.expandButton = expandBtn;
                    expandBtn.onClick.AddListener(() => ShowMatchDetails(matchStat));
                }
            }
            else
            {
                // Display "-" for unplayed games
                rowData.attemptsText.text = "-";
                rowData.avgScoreText.text = "-";
                rowData.bestScoreText.text = "-";
                rowData.sixesText.text = "-";
                rowData.foursText.text = "-";
                rowData.outsText.text = "-";
                
                // Set default values for internal use
                rowData.totalSixes = 0;
                rowData.totalFours = 0;
                rowData.totalOuts = 0;
                
                // Use a dimmer color for unplayed games
                Color baseColor = unplayedGameColor;
                if (rowData.backgroundImage != null)
                {
                    rowData.backgroundImage.color = baseColor;
                }
                
                // Add hover effect
                AddHoverEffect(newRow, baseColor);
            }
            
            // Add tooltip with date and game info
            AddTooltip(newRow, matchStat, date, rowData.isPlayed);
            
            rowDataList.Add(rowData);
            rowIndex++;
        }
        
        // Position career total row at the bottom
        if (careerTotalRow != null)
        {
            careerTotalRow.transform.SetAsLastSibling();
        }
    }
    
    private int CalculateTotalSixes(MatchStatistics matchStat)
    {
        if (matchStat == null) return 0;
        int total = 0;
        foreach (var attempt in matchStat.attempts)
        {
            total += attempt.sixes;
        }
        return total;
    }
    
    private int CalculateTotalFours(MatchStatistics matchStat)
    {
        if (matchStat == null) return 0;
        int total = 0;
        foreach (var attempt in matchStat.attempts)
        {
            total += attempt.fours;
        }
        return total;
    }
    
    private void UpdateCareerTotals()
    {
        var playerStats = statsTracker.GetPlayerStats();
        if (playerStats == null) return;
        
        int totalAttempts = 0;
        int totalSixes = 0;
        int totalFours = 0;
        int totalOuts = 0;
        float totalRuns = 0;
        float totalBalls = 0;
        int bestScore = 0;
        
        // Only count stats from games that have been played
        foreach (var matchStat in allMatchStats)
        {
            if(matchStat.gameplayNumber <= 0) continue;
            totalAttempts += matchStat.TotalAttempts;
            totalOuts += matchStat.TotalOuts;
            
            foreach (var attempt in matchStat.attempts)
            {
                totalRuns += attempt.runsScored;
                totalBalls += attempt.ballsFaced;
                totalSixes += attempt.sixes;
                totalFours += attempt.fours;
                
                if (attempt.runsScored > bestScore)
                    bestScore = attempt.runsScored;
            }
        }
        
        // Calculate different statistics:
        // 1. Average per attempt (what was shown before as "Avg Score")
        float avgScorePerAttempt = totalAttempts > 0 ? totalRuns / totalAttempts : 0;
        
        // 2. Batting Average (total runs / times out) - the true cricket batting average
        // If player has never been out but has scored runs, show runs with a * (not out indicator)
        float battingAverage = totalOuts > 0 ? totalRuns / totalOuts : totalRuns;
        
        // 3. Overall Strike Rate (runs per 100 balls)
        float overallStrikeRate = totalBalls > 0 ? (totalRuns * 100f) / totalBalls : 0;

    GameManager.instance.currentSaveData.strikeRate = overallStrikeRate;
    GameManager.instance.currentSaveData.battingAverage = battingAverage;
    Debug.Log($"✅ Stats saved to SaveData: SR={overallStrikeRate:F1}, BA={battingAverage:F1}");

    SaveSystem.SaveStatsToLocal();

    GameManager.instance.SaveStatsToFirestore(overallStrikeRate, battingAverage);
        
        // Update basic career row texts
        if (careerAttemptsText != null) 
            careerAttemptsText.text = totalAttempts.ToString();
        
        // Display batting average (runs/outs) in the average score field
        if (careerAvgScoreText != null) 
        {
            if (totalOuts > 0)
                careerAvgScoreText.text = battingAverage.ToString("F1");
            else if (totalRuns > 0)
                careerAvgScoreText.text = totalRuns.ToString("F0") + "*"; // * indicates not out
            else
                careerAvgScoreText.text = "0.0";
        }
        
        if (careerBestScoreText != null) 
            careerBestScoreText.text = bestScore.ToString();
        
        if (careerSixesText != null) 
            careerSixesText.text = totalSixes.ToString();
        
        if (careerFoursText != null) 
            careerFoursText.text = totalFours.ToString();
        
        if (careerOutsText != null) 
            careerOutsText.text = totalOuts.ToString();
        
        // Update additional career stats if UI elements are assigned
        if (careerTotalRunsText != null)
            careerTotalRunsText.text = totalRuns.ToString("F0");
        
        if (careerTotalBallsText != null)
            careerTotalBallsText.text = totalBalls.ToString("F0");
        
        if (careerStrikeRateText != null)
            careerStrikeRateText.text = "Strike Rate: " + overallStrikeRate.ToString("F1");
        
        if (careerBattingAverageText != null)
        {
            if (totalOuts > 0)
                careerBattingAverageText.text = battingAverage.ToString("F2");
            else if (totalRuns > 0)
                careerBattingAverageText.text = totalRuns.ToString("F0") + "*";
            else
                careerBattingAverageText.text = "0.00";
            
            careerBattingAverageText.text = "Batting Avg: " + careerBattingAverageText.text;
        }
        
        // Display comprehensive stats in console for debugging
        // Debug.Log($"=============== CAREER STATISTICS ===============");
        // Debug.Log($"Total Matches Played: {allMatchStats.Count}");
        // Debug.Log($"Total Attempts: {totalAttempts}");
        // Debug.Log($"Total Runs: {totalRuns:F0}");
        // Debug.Log($"Total Balls Faced: {totalBalls:F0}");
        // Debug.Log($"Total Outs: {totalOuts}");
        // Debug.Log($"Batting Average (Runs/Outs): {(totalOuts > 0 ? battingAverage.ToString("F2") : totalRuns > 0 ? totalRuns.ToString("F0") + "*" : "0.00")}");
        // Debug.Log($"Overall Strike Rate: {overallStrikeRate:F2}");
        // Debug.Log($"Average per Attempt: {avgScorePerAttempt:F2}");
        // Debug.Log($"Best Score: {bestScore}");
        // Debug.Log($"Total Boundaries: {totalFours + totalSixes} (4s: {totalFours}, 6s: {totalSixes})");
        // Debug.Log($"================================================");
    }
    
    private void AddHoverEffect(GameObject row, Color baseColor)
    {
        EventTrigger trigger = row.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => 
        {
            Image bg = row.GetComponent<Image>();
            if (bg != null) bg.color = highlightColor;
        });
        trigger.triggers.Add(enterEntry);
        
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => 
        {
            Image bg = row.GetComponent<Image>();
            if (bg != null) bg.color = baseColor;
        });
        trigger.triggers.Add(exitEntry);
    }
    
    private void AddTooltip(GameObject row, MatchStatistics stats, string gameplayDate, bool isPlayed)
    {
        // You can implement a tooltip system here to show:
        // - Match date
        // - Target score
        // - Win rate (if played)
        // - Status (played/not played)
        // - Strike rate for this match
        // This is optional and depends on your tooltip implementation
        
        // Example tooltip text:
        string tooltipText = $"Date: {gameplayDate}\n";
        if (isPlayed && stats != null)
        {
            tooltipText += $"Win Rate: {stats.WinRate:F1}%\n";
            tooltipText += $"Attempts: {stats.TotalAttempts}\n";
            tooltipText += $"Average Strike Rate: {stats.AverageStrikeRate:F1}";
        }
        else
        {
            tooltipText += "Status: Not Played Yet";
        }
        
        // You would apply this tooltip text to your tooltip system here
    }
    
    private void ShowMatchDetails(MatchStatistics matchStat)
    {
        // Optional: Show detailed breakdown of individual attempts
        if (matchStat == null)
        {
            Debug.Log("No match data available for this game.");
            return;
        }
        
        Debug.Log($"=============== MATCH DETAILS ===============");
        Debug.Log($"Match (Actual Gameplay #{matchStat.gameplayNumber})");
        Debug.Log($"Date: {matchStat.matchDate}");
        Debug.Log($"Total Attempts: {matchStat.TotalAttempts}");
        Debug.Log($"Wins: {matchStat.Wins} ({matchStat.WinRate:F1}%)");
        Debug.Log($"Average: {matchStat.AverageRuns:F1} runs");
        Debug.Log($"Best Score: {matchStat.BestScore}");
        Debug.Log($"Average Strike Rate: {matchStat.AverageStrikeRate:F1}");
        Debug.Log($"");
        Debug.Log($"Individual Attempts:");
        
        foreach (var attempt in matchStat.attempts)
        {
            Debug.Log($"  Attempt {attempt.attemptNumber}:");
            Debug.Log($"    Runs: {attempt.runsScored} off {attempt.ballsFaced} balls");
            Debug.Log($"    Boundaries: {attempt.fours} fours, {attempt.sixes} sixes");
            Debug.Log($"    Wickets Lost: {attempt.wicketsLost}");
            Debug.Log($"    Strike Rate: {attempt.strikeRate:F1}");
            Debug.Log($"    Result: {(attempt.wonMatch ? "Won" : "Lost")}");
        }
        Debug.Log($"============================================");
    }
    
    private void AnimatePanelIn()
    {
        if (statsPanel == null) return;
        
        CanvasGroup canvasGroup = statsPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = statsPanel.AddComponent<CanvasGroup>();
            
        canvasGroup.alpha = 0f;
        statsPanel.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(canvasGroup.DOFade(1f, fadeInDuration).SetUpdate(true));
        sequence.Join(statsPanel.transform.DOScale(1f, fadeInDuration).SetEase(Ease.OutBack).SetUpdate(true));
        
        // Animate rows appearing one by one
        for (int i = 0; i < matchRows.Count; i++)
        {
            GameObject row = matchRows[i];
            CanvasGroup rowCanvas = row.GetComponent<CanvasGroup>();
            if (rowCanvas == null)
                rowCanvas = row.AddComponent<CanvasGroup>();
                
            rowCanvas.alpha = 0f;
            row.transform.localScale = new Vector3(1f, 0f, 1f);
            
            sequence.Insert(fadeInDuration + (i * rowAnimationDelay), 
                           rowCanvas.DOFade(1f, 0.3f));
            sequence.Insert(fadeInDuration + (i * rowAnimationDelay), 
                           row.transform.DOScaleY(1f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true));
        }
    }
    
    private void AnimatePanelOut(System.Action onComplete)
    {
        if (statsPanel == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        CanvasGroup canvasGroup = statsPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(canvasGroup.DOFade(0f, fadeInDuration * 0.5f).SetUpdate(true));
        sequence.Join(statsPanel.transform.DOScale(0.9f, fadeInDuration * 0.5f).SetEase(Ease.InBack).SetUpdate(true));
        sequence.OnComplete(() => onComplete?.Invoke());
    }
}