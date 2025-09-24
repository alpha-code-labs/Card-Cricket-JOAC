// Most the of UI for the game is done via this script
// Also game sounds
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    //load rewards stats to update these hardcoded for now
    public int maxTimeToChooseStrategy = 5; // seconds
    public int maxEnergy = 24;

    void Awake()
    {
        Instance = this;
    }

    int currentRuns = 0; // Current runs scored
    int TargetScore = 40; // The target score to reach
    internal int MaxBalls = 24; // Maximum balls in the game (e.g., 6 overs)
    public int wickets = 3; // Wickets before game over
    [SerializeField] TextMeshProUGUI scoreText; // Text to display the score
    [SerializeField] TextMeshProUGUI currentRunsText;
    [SerializeField] TextMeshProUGUI totalRunsNeededText;
    [SerializeField] TextMeshProUGUI remainingBallsText;
    [SerializeField] TextMeshProUGUI remainingWicketsText;
    [SerializeField] TextMeshProUGUI totalWicketsText;
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
    // public void PlayExcelBattingStrategy(BattingStrategy battingStrategy)
    // {
    //     // TimingManager.Instance.StartTimingMeter(battingStrategy);

    //     // Temporary outcome for testing
    //     // OutCome outcome = OutCome.Out;
    //     // OutCome outcome = outComeCalculator.CalculateOutcome(battingStrategy, new BallThrow() { ballType = BallType.Straight, bowlerSide = Side.RightHanded, bowlerType = TypeOfBowler.Fast, ballLength = BallLength.GoodLength, ballLine = BallLine.OffStump }, BattingTiming.Perfect);
    //     BallThrow currentBallThrow = CardsPoolManager.Instance.CurrentBallThrow;
    //     PitchCondition pitchCondition = currentBallThrow.pitchCondition;
    //     Debug.Log($"Current Ball Throw: \n{currentBallThrow}\n Pitch Condition: {pitchCondition}");
    //     OutCome outcome = ExcelDataSOManager.Instance.outComeCalculator.CalculateOutcome(battingStrategy, currentBallThrow, BattingTiming.Perfect, pitchCondition);

    //     UpdateScore((int)outcome);
    //     AnimateOnScreenText(battingStrategy, outcome);
    //     CardsPoolManager.Instance.EndTurn((int)outcome != -3); // End turn and increment balls only if not out
    // }

    public void PlayExcelBattingStrategy(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        // Animate the batter image
        StartCoroutine(PlayCardSequence(battingStrategy, cardObject, cardSprite));
    }

    private IEnumerator PlayCardSequence(BattingStrategy battingStrategy, GameObject cardObject, Sprite cardSprite)
    {
        // Pause timer during animation
        Timer.Instance.PauseTimer();
        AnimateBatterSwing();
        gameAudioSource.Play();
        // Calculate outcome
        BallThrow currentBallThrow = CardsPoolManager.Instance.CurrentBallThrow;
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
            CardsPoolManager.Instance.DestroyCurrentBallCard();
            //Processing pause
            yield return new WaitForSeconds(.5f);
        }
        else
        {
            UpdateScore((int)outcome);
            CardsPoolManager.Instance.DestroyCurrentBallCard();
            // Fallback if no animation controller
            yield return new WaitForSeconds(1f);
        }



        //End current turn
        CardsPoolManager.Instance.EndTurn((int)outcome != -3);

        yield return new WaitForSeconds(3f);
        // End turn timer
        Timer.Instance.EndTurnTimer();
        // We will start new turn here
        CardsPoolManager.Instance.StartTurn((int)outcome != -3);
    }

    [SerializeField] TextMeshProUGUI outcomeText;
    void AnimateOnScreenText(BattingStrategy battingStrategy, OutCome outcome)
    {
        outcomeText.text = $"Played card: {battingStrategy},\n Outcome: {outcome} ";
        outcomeText.color = Color.white;
        // Kill any ongoing tweens to prevent overlap
        outcomeText.DOKill(true);
        outcomeText.rectTransform.DOKill(true);

        // Ensure starting state
        var c = outcomeText.color; c.a = 0f; outcomeText.color = c;
        outcomeText.rectTransform.localScale = Vector3.one;

        // Punch animation: quick fade-in + punch scale, then fade-out
        Sequence seq = DOTween.Sequence();
        seq.Append(outcomeText.DOFade(1f, 1f));
        seq.Join(outcomeText.rectTransform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.5f, 8, 0.8f));
        seq.AppendInterval(0.5f);
        seq.Append(outcomeText.DOFade(0f, 0.4f));
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
        CardsPoolManager.Instance.RedrawHand();
        UpdateRedrawButton();
    }

    void UpdateRedrawButton()
    {
        if (redrawButton == null) return;

        bool canRedraw = CardsPoolManager.Instance.CanRedraw();
        redrawButton.interactable = canRedraw;

        // Update button text if available
        if (redrawButtonText != null)
        {
            int remaining = CardsPoolManager.Instance.GetRedrawsRemaining();
            redrawButtonText.text = $"Redraw ({remaining})";

            // Optional: Change color based on availability
            redrawButtonText.color = canRedraw ? Color.white : Color.gray;
        }
    }

    void Start()
    {
        totalWicketsText.text = "/ " + wickets.ToString();
        UpdateScore(0); // Initialize score display
        UpdateBallsAndOvers(0); // Initialize balls and overs display
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
}
