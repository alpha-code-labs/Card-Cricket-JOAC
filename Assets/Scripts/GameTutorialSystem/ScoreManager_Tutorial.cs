// Most the of UI for the game is done via this script
// Also game sounds
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class ScoreManager_Tutorial : MonoBehaviour
{
    public static ScoreManager_Tutorial Instance;

    //load rewards stats to update these hardcoded for now
    public int maxTimeToChooseStrategy = 5; // seconds
    public int maxEnergy = 24;

    void Awake()
    {
        Instance = this;
    }

    int currentRuns = 0; // Current runs scored
    public int TargetScore = 40; // The target score to reach
    public int MaxBalls = 24; // Maximum balls in the game (e.g., 6 overs)
    public int wickets = 3; // Wickets before game over
    [Header("Current score And Target")]
    [SerializeField] GameObject currentScoreAndTarget_parent;
    [SerializeField] TextMeshProUGUI scoreText; // Text to display the score
    [SerializeField] TextMeshProUGUI currentRunsText;
    [SerializeField] TextMeshProUGUI totalRunsNeededText;

    [Header("Remaing Balls")]
    [SerializeField] GameObject RemainingBalls_parent;
    [SerializeField] TextMeshProUGUI remainingBallsText;

    [Header("Remaining Wickets And Total Wickets")]
    [SerializeField] GameObject remaingWicketsAndTotalWickets_parent;
    [SerializeField] TextMeshProUGUI remainingWicketsText;
    [SerializeField] TextMeshProUGUI totalWicketsText;
    [Header("Others")]
    [SerializeField] TextMeshProUGUI ballsAndOversText; // Text to display balls and overs
    [SerializeField] Button redrawButton; // Assign in Inspector
    [SerializeField] TextMeshProUGUI redrawButtonText;
    

    [Header("Batter Animation")]
    [SerializeField] Image BatterImage;
    [SerializeField] float swingDistance = 100f; // How far to move right
    [SerializeField] float swingDuration = 0.15f; // Fast swing duration
    [SerializeField] float returnDuration = 0.3f; // Slower return duration
    [SerializeField] AnimationCurve swingEase = AnimationCurve.EaseInOut(0, 0, 1, 1); // Custom curve if needed
    [SerializeField] AudioSource gameAudioSource;
    [SerializeField] TextMeshProUGUI outText;

    [Header("Yarn Spinner References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private InMemoryVariableStorage variableStorage;

    public void UpdateBallsAndOvers(int ballsBowled)
    {
        int overs = ballsBowled / 6;
        int balls = ballsBowled % 6;
        int ballDisplay = balls + 1;
        int overDisplay = overs + 1;
        int ballsRemain = MaxBalls - ballsBowled;
        remainingBallsText.text = ballsRemain.ToString();

        ballsAndOversText.text = $"Ball {ballDisplay} of over {overDisplay}\n total balls remain {ballsRemain}\n Wickets: {wickets}";
        if (ballsBowled >= MaxBalls)
        {
            ballsAndOversText.text += "\n All Balls Lost! Game Over!";
        }
        if (wickets <= 0)
        {
            // Logic to handle game over, e.g., showing a game over screen
            ballsAndOversText.text += "\n All Wickets Lost! Game Over!";
        }
    }
    bool targetReached = false;
    private Vector3 batterOriginalPosition;
    private Tween currentBatterTween;
    public void UpdateScore(int runs)
    {
        if (runs > 0)
            currentRuns += runs;
        currentRunsText.text = currentRuns.ToString();
        totalRunsNeededText.text = "/ " + TargetScore.ToString();
        remainingWicketsText.text = wickets.ToString();
        scoreText.text = "Score: " + currentRuns.ToString() + " / " + TargetScore.ToString();
        if (currentRuns >= TargetScore && !targetReached)
        {
            targetReached = true;
            Debug.Log("Target Reached! You win!");
            // Logic to handle winning the game, e.g., showing a win screen
        }
        if (runs == -1)
        {
            LooseWicket();
        }
        if (runs == -3)
        {
            currentRuns += 1; // Add one run for wide ball
            scoreText.text = "Score: " + currentRuns.ToString() + " / " + TargetScore.ToString();
        }
    }
    public void LooseWicket()
    {

        wickets--;
        remainingWicketsText.text = wickets.ToString();
        if (wickets <= 0)
        {
            Debug.Log("All Wickets Lost! Game Over!");
            // Logic to handle game over, e.g., showing a game over screen
        }
    }

    public void PlayExcelBattingStrategy(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        if (CardsPoolManager_Tutorial.Instance.currentTutorialBall == "None" || CardsPoolManager_Tutorial.Instance.currentTutorialBall == "fourth") return;
        // Animate the batter image
        StartCoroutine(PlayCardSequence(battingStrategy, cardObject, cardSprite));
    }

    public void showOutText()
    {
        outText.gameObject.SetActive(true);
    }
    public void hideOutText()
    {
        outText.gameObject.SetActive(false);
    }

    private IEnumerator PlayCardSequence(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        // Pause timer during animation
        Timer_Tutorial.Instance.PauseTimer();
        AnimateBatterSwing();
        gameAudioSource.Play();
        // Calculate outcome
        BallThrow currentBallThrow = CardsPoolManager_Tutorial.Instance.CurrentBallThrow;
        PitchCondition pitchCondition = currentBallThrow.pitchCondition;
        Debug.Log($"Current Ball Throw: \n{currentBallThrow}\n Pitch Condition: {pitchCondition}");
        OutCome outcome = ExcelDataSOManager.Instance.outComeCalculator.CalculateOutcome(
            battingStrategy, currentBallThrow, BattingTiming.Perfect, pitchCondition);



        // Play animation sequence
        if (CardPlayAnimationController.Instance != null)
        {
            yield return CardPlayAnimationController.Instance.PlayCardSequence(
                cardObject, cardSprite, battingStrategy, outcome);
            // Update score immediately (but don't show yet)
            UpdateScore((int)outcome);
            CardsPoolManager_Tutorial.Instance.DestroyCurrentBallCard();
            //Processing pause
            yield return new WaitForSeconds(.5f);
        }
        else
        {
            UpdateScore((int)outcome);
            CardsPoolManager_Tutorial.Instance.DestroyCurrentBallCard();
            // Fallback if no animation controller
            yield return new WaitForSeconds(1f);
        }



        //End current turn
        CardsPoolManager_Tutorial.Instance.EndTurn((int)outcome != -3);

        yield return new WaitForSeconds(.7f);
        variableStorage.SetValue("$selection", battingStrategy.ToString());
        Debug.Log("setting variable selection of yarn variable storage to " + battingStrategy);
        dialogueRunner.Stop();
        switch (CardsPoolManager_Tutorial.Instance.currentTutorialBall)
        {
            case "first":
                {
                    Debug.Log("starting firstBall node dialgoues");
                    dialogueRunner.StartDialogue("FirstBall");
                    break;
                }

            case "second":
                {
                    Debug.Log("starting secondBall node dialgoues");
                    dialogueRunner.StartDialogue("SecondBall");
                    break;
                }

            case "third":
                {
                    Debug.Log("starting thirdBall node dialgoues");
                    dialogueRunner.StartDialogue("ThirdBall");
                    break;
                }
        }
        CardsPoolManager_Tutorial.Instance.currentTutorialBall = "None";
        // End turn timer
        Timer_Tutorial.Instance.EndTurnTimer();
        // We will start new turn here
        //CardsPoolManager_Tutorial.Instance.StartTurn((int)outcome != -3);
    }

    void AnimateBatterSwing()
    {
        if (BatterImage == null) return;

        // Kill any existing animation on the batter
        if (currentBatterTween != null && currentBatterTween.IsActive())
        {
            currentBatterTween.Kill();
            BatterImage.rectTransform.anchoredPosition = batterOriginalPosition;
        }

        // Create a sequence for the swing animation
        Sequence swingSequence = DOTween.Sequence();

        // Option 1: Simple horizontal movement
        swingSequence.Append(BatterImage.rectTransform.DOAnchorPosX(batterOriginalPosition.x + swingDistance, swingDuration)
            .SetEase(Ease.OutQuad)); // Fast swing out

        swingSequence.Append(BatterImage.rectTransform.DOAnchorPosX(batterOriginalPosition.x, returnDuration)
            .SetEase(Ease.InOutSine)); // Slower return

        currentBatterTween = swingSequence;

        // Optional: Add callback when animation completes
        swingSequence.OnComplete(() =>
        {
            Debug.Log("Batter swing animation completed");
        });
    }
    void OnRedrawButtonClicked()
    {
        CardsPoolManager_Tutorial.Instance.RedrawHand();
        UpdateRedrawButton();
    }

    void UpdateRedrawButton()
    {
        if (redrawButton == null) return;

        bool canRedraw = CardsPoolManager_Tutorial.Instance.CanRedraw();
        redrawButton.interactable = canRedraw;

        // Update button text if available
        if (redrawButtonText != null)
        {
            int remaining = CardsPoolManager_Tutorial.Instance.GetRedrawsRemaining();
            redrawButtonText.text = $"Redraw ({remaining})";

            // Optional: Change color based on availability
            redrawButtonText.color = canRedraw ? Color.white : Color.gray;
        }
    }

    public void ShowScorePanel()
    {
        currentScoreAndTarget_parent.SetActive(true);
        UpdateScore(0);
        UIHighlightManager.Instance.HighlightObject(currentScoreAndTarget_parent);
    }

    public void ShowBallsPanel()
    {
        RemainingBalls_parent.SetActive(true);
        UpdateBallsAndOvers(0);
        UIHighlightManager.Instance.HighlightObject(RemainingBalls_parent);  
    }

    public void ShowWicketsPanel()
    {
        remaingWicketsAndTotalWickets_parent.SetActive(true);
        UIHighlightManager.Instance.HighlightObject(remaingWicketsAndTotalWickets_parent);
    }

    public void ShowFlipButton()
    {
        Debug.Log("can player redraw" + CardsPoolManager_Tutorial.Instance.CanRedraw());
        redrawButton.gameObject.SetActive(true);
        UIHighlightManager.Instance.HighlightObject(redrawButton.gameObject);
    }

    void Start()
    {
        // totalWicketsText.text = "/ " + wickets.ToString();
        // UpdateScore(0); // Initialize score display
        // UpdateBallsAndOvers(0); // Initialize balls and overs display
        //Disable everything
         // Get references if not assigned
        if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        
        if (variableStorage == null)
            variableStorage = dialogueRunner.GetComponent<InMemoryVariableStorage>();
        currentScoreAndTarget_parent.SetActive(false);
        remaingWicketsAndTotalWickets_parent.SetActive(false);
        RemainingBalls_parent.SetActive(false);
        if (redrawButton != null)
        {
            redrawButton.onClick.AddListener(OnRedrawButtonClicked);
            UpdateRedrawButton();
        }
        if (BatterImage != null)
        {
            batterOriginalPosition = BatterImage.rectTransform.anchoredPosition;
        }
    }
    public void SetTargetFromEventName(string eventName)
    {
        ScriptableObject gameObject = Resources.Load<ScriptableObject>(eventName);
        switch (eventName)
        {
            case "Event1":
                TargetScore = 10;
                MaxBalls = 6;
                break;
            case "Event2":
                TargetScore = 20;
                MaxBalls = 9;
                break;
            case "Event3":
                TargetScore = 30;
                MaxBalls = 12;
                break;
            default:
                TargetScore = 30;
                MaxBalls = 12;
                Debug.LogWarning("No target score set for this event, using default values.");
                break;
        }
        UpdateScore(0); // Reset score display
        UpdateBallsAndOvers(0); // Reset balls and overs display
    }
}
