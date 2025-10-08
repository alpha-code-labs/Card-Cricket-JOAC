using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private static GraphicRaycaster persistentDialogueRaycaster;
    
    [Header("Game Configuration")]
    private GameplayConfig currentGameplayConfig;
    private bool isBattingFirst;
    private int initialWickets;
    
    void Awake()
    {
        disableRaycasterOnMainDialogueSystem();
        
        // Get gameplay configuration
        currentGameplayConfig = GameplayConfiguration.Instance.GetCurrentGameplayConfig();
        if (currentGameplayConfig != null)
        {
            TargetScore = currentGameplayConfig.targetScore;
            MaxBalls = currentGameplayConfig.balls;
            isBattingFirst = currentGameplayConfig.isBattingFirst;
            
            Debug.Log($"Gameplay {currentGameplayConfig.gameplayNumber} - Date: {currentGameplayConfig.date}");
            Debug.Log($"Batting First: {isBattingFirst}, Target: {TargetScore}, Balls: {MaxBalls}");
        }
        
        if (GameManager.instance != null)
            wickets = baseWickets + GameManager.instance.currentSaveData.humility;
        else 
            wickets = baseWickets;
            
        initialWickets = wickets;
        Instance = this;
    }
    
    public int currentRuns = 0;
    public int TargetScore = 40;
    public int MaxBalls = 24;
    public int baseWickets = 2;
    private int wickets;
    
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI currentRunsText;
    [SerializeField] TextMeshProUGUI totalRunsNeededText;
    [SerializeField] TextMeshProUGUI remainingBallsText;
    [SerializeField] TextMeshProUGUI remainingWicketsText;
    [SerializeField] TextMeshProUGUI totalWicketsText;
    [SerializeField] TextMeshProUGUI ballsAndOversText;
    [SerializeField] Button redrawButton;
    [SerializeField] TextMeshProUGUI redrawButtonText;
    
    [Header("Batter Animation")]
    [SerializeField] Image BatterImage;
    [SerializeField] float swingDistance = 100f;
    [SerializeField] float swingDuration = 0.15f;
    [SerializeField] float returnDuration = 0.3f;
    [SerializeField] AnimationCurve swingEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AudioSource gameAudioSource;
    
    [Header("Game Over UI")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI gameOverText;
    
    private bool gameEnded = false;
    
    public int getCurrentWickets()
    {
        return wickets;
    }
    
    public void UpdateBallsAndOvers(int ballsBowled)
    {
        int overs = ballsBowled / 6;
        int balls = ballsBowled % 6;
        int ballDisplay = balls + 1;
        int overDisplay = overs + 1;
        int ballsRemain = MaxBalls - ballsBowled;
        remainingBallsText.text = ballsRemain.ToString();
        
        string statusText = $"Ball {ballDisplay} of over {overDisplay}\n total balls remain {ballsRemain}\n Wickets: {wickets}";
        
        if (!isBattingFirst)
        {
            int runsNeeded = TargetScore - currentRuns;
            statusText += $"\nRuns needed: {runsNeeded}";
        }
        
        ballsAndOversText.text = statusText;
        
        // Check for game over conditions
        if (!gameEnded)
        {
            if (ballsBowled >= MaxBalls)
            {
                HandleGameOver("lost_runs", "All balls used!");
            }
            else if (wickets <= 0)
            {
                HandleGameOver("lost_wickets", "All wickets lost!");
            }
        }
    }
    
    bool targetReached = false;
    private Vector3 batterOriginalPosition;
    private Tween currentBatterTween;
    
    public void UpdateScore(int runs)
    {
        if (gameEnded) return;
        
        if (runs > 0)
            currentRuns += runs;
            
        currentRunsText.text = currentRuns.ToString();
        
        if (isBattingFirst)
        {
            totalRunsNeededText.text = "";
            scoreText.text = "Score: " + currentRuns.ToString();
        }
        else
        {
            totalRunsNeededText.text = "/ " + TargetScore.ToString();
            scoreText.text = "Score: " + currentRuns.ToString() + " / " + TargetScore.ToString();
        }
        
        remainingWicketsText.text = wickets.ToString();
        
        // Check if target is reached (when chasing)
        if (!isBattingFirst && currentRuns >= TargetScore && !targetReached)
        {
            targetReached = true;
            HandleGameOver("won", "Target reached! You win!");
        }
        
        if (runs == -1)
        {
            LooseWicket();
        }
        if (runs == -3) // Wide ball
        {
            currentRuns += 1;
            if (isBattingFirst)
            {
                scoreText.text = "Score: " + currentRuns.ToString();
            }
            else
            {
                scoreText.text = "Score: " + currentRuns.ToString() + " / " + TargetScore.ToString();
            }
        }
    }
    
    public void LooseWicket()
    {
        wickets--;
        remainingWicketsText.text = wickets.ToString();
        
        if (wickets <= 0 && !gameEnded)
        {
            HandleGameOver("lost_wickets", "All wickets lost!");
        }
    }
    
    private void HandleGameOver(string result, string message)
    {
        if (gameEnded) return;
        gameEnded = true;
        
        Debug.Log($"Game Over: {message}");
        
        // Stop the game
        Timer.Instance.EndTurnTimer();
        CardsPoolManager.Instance.SetCardsInteractable(false);
        
        // Set Yarn variables for scene navigation
        SetYarnVariables(result);
        
        // Show game over UI if available
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null)
            {
                gameOverText.text = message + $"\nFinal Score: {currentRuns}";
                if (!isBattingFirst)
                {
                    gameOverText.text += $" / {TargetScore}";
                }
            }
        }
        
        // Call event end after a delay
        StartCoroutine(EndGameAfterDelay());
    }
    
    private void SetYarnVariables(string result)
    {
        var storage = YarnDialogSystemSingleTonMaker.instance.dialogueRunner.VariableStorage;
        
        // Set gameplay outcome variables
        storage.SetValue("$gameplayNumber", currentGameplayConfig.gameplayNumber); // Changed from $gameplayDate to $gameplayNumber
        storage.SetValue("$finalScore", currentRuns);
        storage.SetValue("$targetScore", TargetScore);
        storage.SetValue("$wicketsLost", initialWickets - wickets);
        storage.SetValue("$gameResult", result);
        
        Debug.Log($"Yarn Variables Set - Gameplay Number: {currentGameplayConfig.gameplayNumber}, Score: {currentRuns}, " +
                  $"Target: {TargetScore}, Wickets Lost: {initialWickets - wickets}, Result: {result}");
    }
    
    private IEnumerator EndGameAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        
        // Re-enable the main dialogue system
        enableRaycasterOnMainDialogueSystem();
        
        // Call NewDayManager to end the event
        NewDayManager.EndEvent();
    }
    
    public void PlayExcelBattingStrategy(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        StartCoroutine(PlayCardSequence(battingStrategy, cardObject, cardSprite));
    }
    
    private IEnumerator PlayCardSequence(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        Timer.Instance.PauseTimer();
        AnimateBatterSwing();
        gameAudioSource.Play();
        
        BallThrow currentBallThrow = CardsPoolManager.Instance.CurrentBallThrow;
        PitchCondition pitchCondition = currentBallThrow.pitchCondition;
        Debug.Log($"Current Ball Throw: \n{currentBallThrow}\n Pitch Condition: {pitchCondition}");
        OutCome outcome = ExcelDataSOManager.Instance.outComeCalculator.CalculateOutcome(
            battingStrategy, currentBallThrow, BattingTiming.Perfect, pitchCondition);
        
        if (CardPlayAnimationController.Instance != null)
        {
            yield return CardPlayAnimationController.Instance.PlayCardSequence(
                cardObject, cardSprite, battingStrategy, outcome);
            UpdateScore((int)outcome);
            CardsPoolManager.Instance.DestroyCurrentBallCard();
            yield return new WaitForSeconds(.5f);
        }
        else
        {
            UpdateScore((int)outcome);
            CardsPoolManager.Instance.DestroyCurrentBallCard();
            yield return new WaitForSeconds(1f);
        }
        
        // Check if game should continue
        if (!gameEnded)
        {
            CardsPoolManager.Instance.EndTurn(MaxBalls, (int)outcome != -3);
            yield return new WaitForSeconds(3f);
            Timer.Instance.EndTurnTimer();
            
            // Only start new turn if game hasn't ended
            if (!gameEnded)
            {
                CardsPoolManager.Instance.StartTurn((int)outcome != -3);
            }
        }
    }
    
    [SerializeField] TextMeshProUGUI outcomeText;
    void AnimateOnScreenText(BattingStrategy battingStrategy, OutCome outcome)
    {
        outcomeText.text = $"Played card: {battingStrategy},\n Outcome: {outcome} ";
        outcomeText.color = Color.white;
        
        outcomeText.DOKill(true);
        outcomeText.rectTransform.DOKill(true);
        
        var c = outcomeText.color; c.a = 0f; outcomeText.color = c;
        outcomeText.rectTransform.localScale = Vector3.one;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(outcomeText.DOFade(1f, 1f));
        seq.Join(outcomeText.rectTransform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 8, 0.8f));
        seq.AppendInterval(0.5f);
        seq.Append(outcomeText.DOFade(0f, 0.4f));
    }
    
    void AnimateBatterSwing()
    {
        if (BatterImage == null) return;
        
        if (currentBatterTween != null && currentBatterTween.IsActive())
        {
            currentBatterTween.Kill();
            BatterImage.rectTransform.anchoredPosition = batterOriginalPosition;
        }
        
        Sequence swingSequence = DOTween.Sequence();
        swingSequence.Append(BatterImage.rectTransform.DOAnchorPosX(batterOriginalPosition.x + swingDistance, swingDuration)
            .SetEase(Ease.OutQuad));
        swingSequence.Append(BatterImage.rectTransform.DOAnchorPosX(batterOriginalPosition.x, returnDuration)
            .SetEase(Ease.InOutSine));
        
        currentBatterTween = swingSequence;
        swingSequence.OnComplete(() => Debug.Log("Batter swing animation completed"));
    }
    
    void OnRedrawButtonClicked()
    {
        CardsPoolManager.Instance.RedrawHand();
        UpdateRedrawButton();
    }
    
    void UpdateRedrawButton()
    {
        if (redrawButton == null) return;
        
        bool canRedraw = CardsPoolManager.Instance.CanRedraw();
        Debug.Log($"Redraw Button - Can Redraw: {canRedraw}");
        redrawButton.interactable = canRedraw;
        
        if (redrawButtonText != null)
        {
            int remaining = CardsPoolManager.Instance.GetRedrawsRemaining();
            redrawButtonText.text = $"Redraw ({remaining})";
            redrawButtonText.color = canRedraw ? Color.white : Color.gray;
        }
    }
    
    void Start()
    {
        // Update UI based on game mode
        if (isBattingFirst)
        {
            totalRunsNeededText.text = "";
            totalWicketsText.text = "/ " + wickets.ToString();
        }
        else
        {
            totalRunsNeededText.text = "/ " + TargetScore.ToString();
            totalWicketsText.text = "/ " + wickets.ToString();
        }
        
        UpdateScore(0);
        UpdateBallsAndOvers(0);

        if (redrawButton != null)
        {
            redrawButton.onClick.AddListener(OnRedrawButtonClicked);
            StartCoroutine(UpdateRedrawButtonRoutine()); // Initial update after delay
        }
        
        if (BatterImage != null)
        {
            batterOriginalPosition = BatterImage.rectTransform.anchoredPosition;
        }
    }
    
    IEnumerator UpdateRedrawButtonRoutine()
    {
        yield return new WaitForSeconds(6f);
        UpdateRedrawButton();
    }
    
    // Keep existing utility methods unchanged
    public void disableRaycasterOnMainDialogueSystem()
    {
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System")
            .ToArray();

        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                GraphicRaycaster raycaster = dialogueSystem.GetComponentInChildren<GraphicRaycaster>();
                if (raycaster != null)
                {
                    raycaster.enabled = false;
                    persistentDialogueRaycaster = raycaster;
                    Debug.Log($"Disabled DontDestroyOnLoad Dialogue System raycaster from scene: {dialogueSystem.scene.name}");
                }
                break;
            }
        }
    }
    
    public void enableRaycasterOnMainDialogueSystem()
    {   
        if (persistentDialogueRaycaster != null)
        {
            persistentDialogueRaycaster.enabled = true;
            Debug.Log("Re-enabled DontDestroyOnLoad Dialogue System raycaster (from reference)");
            return;
        }
        
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System")
            .ToArray();
            
        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                GraphicRaycaster raycaster = dialogueSystem.GetComponentInChildren<GraphicRaycaster>();
                if (raycaster != null)
                {
                    raycaster.enabled = true;
                    persistentDialogueRaycaster = raycaster;
                    Debug.Log($"Re-enabled DontDestroyOnLoad Dialogue System raycaster from scene: {dialogueSystem.scene.name}");
                }
                break;
            }
        }
    }
}