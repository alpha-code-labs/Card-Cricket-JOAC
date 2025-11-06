using System;
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
    public ParticleSystem fireworkEffect;
    public ParticleSystem fireworkEffect2;
    private bool canUpdateChaseDisplay = false;
    private bool hattrickTriggered = false;
    public GameObject gamePuaseInstructionText;
    
    [Header("Hat-trick Animation")]
    private int consecutiveBoundaries = 0;
    [SerializeField] Image hatTrickImage; // Assign the hat-trick image in inspector
    [SerializeField] float flipDuration = 0.3f; // Duration for each flip (3 flips total)
    [SerializeField] float punchStrength = 0.5f; // Strength of the punch effect
    [SerializeField] AudioClip hatTrickSound; // Optional: Special sound for hat-trick


    void Awake()
    {
        disableRaycasterOnMainDialogueSystem();
        if (GameManager.instance != null)
            wickets = baseWickets + GameManager.instance.currentSaveData.humility;
        else
            wickets = baseWickets;

        initialWickets = wickets;
        Instance = this;

        // Initially hide chase display
        if (chaseDisplayText != null && chaseDisplayContainer != null)
        {
            chaseDisplayContainer.SetActive(false);
            chaseDisplayText.gameObject.SetActive(false);
        }

        // Initially hide pause intruction
        gamePuaseInstructionText.SetActive(false);
    }
    public void TriggerFirework()
    {
        if (fireworkEffect != null)
        {
            // musicIntensity.SetExcitement(.65f);
            fireworkEffect.Play();
            fireworkEffect2.Play();
        }
    }
    
    // Simple hat-trick animation with image
    private void TriggerHatTrickAnimation()
    {
        
        hattrickTriggered = true;
        Debug.Log("HAT-TRICK! Player hit 3 boundaries in a row!");
        
        // Start the simple image animation
        if (hatTrickImage != null)
        {
            StartCoroutine(PlayHatTrickImageAnimation());
        }
        
        // Trigger fireworks for extra celebration
        TriggerFirework();
    }
    
    private IEnumerator PlayHatTrickImageAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Slight delay before starting
        CardPlayAnimationController.Instance.StopCommentary();
        // Play special hat-trick sound if available
        if (hatTrickSound != null && gameAudioSource != null)
        {
            gameAudioSource.PlayOneShot(hatTrickSound, cheeringVolume * 1.2f);
        }
        // Make sure image is active and starts at scale 0
        hatTrickImage.gameObject.SetActive(true);
        hatTrickImage.transform.localScale = Vector3.zero;
        
        // Create animation sequence
        Sequence hatTrickSequence = DOTween.Sequence();
        
        // Scale up from 0 to 1
        hatTrickSequence.Append(hatTrickImage.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        
        // Do 3 quick flips (rotate 360 degrees 3 times)
        for (int i = 0; i < 1; i++)
        {
            hatTrickSequence.Append(hatTrickImage.transform.DORotate(new Vector3(0, 0, 360f * (i + 1)), flipDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));
        }
        
        // Punch effect
        hatTrickSequence.Append(hatTrickImage.transform.DOPunchScale(Vector3.one * punchStrength, 0.5f, 10, 1f));
        
        // Wait for the sequence to complete
        yield return hatTrickSequence.WaitForCompletion();
        
        // Stay visible for 3 seconds
        yield return new WaitForSeconds(3f);
        
        // Scale down to 0
        yield return hatTrickImage.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).WaitForCompletion();
        
        // Hide the image
        hatTrickImage.gameObject.SetActive(false);
        
        // Reset rotation for next time
        hatTrickImage.transform.rotation = Quaternion.identity;
                
        // Reset music intensity to normal celebratory level
        // musicIntensity.SetExcitement(0.65f);
    }
    
    public int currentRuns = 0;
    public int TargetScore = 40;
    public int MaxBalls = 24;
    public int baseWickets = 2;
    private int wickets;

    [SerializeField] TextMeshProUGUI outcomeCommentaryText;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI currentRunsText;
    [SerializeField] TextMeshProUGUI totalRunsNeededText;
    [SerializeField] TextMeshProUGUI remainingBallsText;
    [SerializeField] TextMeshProUGUI remainingWicketsText;
    [SerializeField] TextMeshProUGUI totalWicketsText;
    [SerializeField] TextMeshProUGUI ballsAndOversText;
    [SerializeField] Button redrawButton;
    [SerializeField] TextMeshProUGUI redrawButtonText;

    [Header("Chase Mode Display")]
    [SerializeField] TextMeshProUGUI chaseDisplayText; // Single text for "X runs needed in Y balls"
    [SerializeField] GameObject chaseDisplayContainer;
    [SerializeField] float chasePulseDuration = 0.3f;
    [SerializeField] float chasePulseScale = 1.06f;
    [SerializeField] Color comfortableChaseColor = new Color(0.4f, 1f, 0.4f); // Green
    [SerializeField] Color normalChaseColor = Color.white;
    [SerializeField] Color tightChaseColor = new Color(1f, 1f, 0.4f); // Yellow
    [SerializeField] Color difficultChaseColor = new Color(1f, 0.6f, 0.2f); // Orange
    [SerializeField] Color criticalChaseColor = new Color(1f, 0.3f, 0.3f); // Red

    [Header("Audio")]
    [SerializeField] AudioClip cheeringSound; // Assign your cheering audio file here
    [SerializeField] AudioClip battingSound; // Assign your batting audio file here
    [SerializeField] AudioClip cheeringSound_gameWon;
    [SerializeField] AudioClip groaningSound;
    [SerializeField] float cheeringVolume = 1f;
    [SerializeField] AudioSource cheeringAudioSource; // Optional: separate audio source for cheering
    [SerializeField] MusicIntensity musicIntensity;
    
    
    private Sequence chaseTextSequence;
    private int lastRunsNeeded = -1;
    private int lastBallsRemaining = -1;
    
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

    [Header("UI Animation Settings")]
    [SerializeField] float scoreAnimDuration = 0.5f;
    [SerializeField] float wicketAnimDuration = 0.6f;
    [SerializeField] float ballAnimDuration = 0.4f;
    [SerializeField] AnimationCurve scoreAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] Color wicketLossColor = Color.red;
    [SerializeField] Color scoreIncreaseColor = new Color(0.4f, 1f, 0.4f); // Green
    [SerializeField] Color ballDecreaseColor = new Color(1f, 0.8f, 0.2f); // Yellow-orange

    public static event System.Action<int> OnWideBall;

    private bool gameEnded = false;
    private int previousRuns;
    private int previousWickets;
    private int previousBallsRemaining;
    
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
        // Animate balls remaining decrease
        if (previousBallsRemaining != -1 && ballsRemain < previousBallsRemaining)
        {
            AnimateBallsDecrease(ballsRemain);
        }
        else
        {
            remainingBallsText.text = ballsRemain.ToString();
        }
        previousBallsRemaining = ballsRemain;

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

    
    private void UpdateChaseDisplay(int runsNeeded, int ballsRemaining)
    {
        Debug.Log("Updating chase display text");
        if (chaseDisplayText == null || gameEnded)
            return;
            
        // Check if values have changed
        bool valuesChanged = (runsNeeded != lastRunsNeeded || ballsRemaining != lastBallsRemaining);
        
        if (valuesChanged)
        {
            // Kill any existing animation
            if (chaseTextSequence != null && chaseTextSequence.IsActive())
            {
                chaseTextSequence.Kill();
                chaseDisplayText.transform.localScale = Vector3.one;
            }
            
            // Update text with formatted display
            if (runsNeeded <= 0)
            {
                chaseDisplayText.text = "Target Achieved!";
            }
            else if (ballsRemaining == 1)
            {
                chaseDisplayText.text = $"{runsNeeded} {(runsNeeded == 1 ? "run" : "runs")} needed in {ballsRemaining} ball";
            }
            else
            {
                chaseDisplayText.text = $"{runsNeeded} {(runsNeeded == 1 ? "run" : "runs")} needed in {ballsRemaining} balls";
            }
            
            // Determine color based on chase difficulty
            Color targetColor = GetChaseColor(runsNeeded, ballsRemaining);
            
            // Create animation sequence
            chaseTextSequence = DOTween.Sequence();
            
            // Subtle pulse effect
            chaseTextSequence.Append(chaseDisplayText.transform.DOScale(chasePulseScale, chasePulseDuration * 0.5f)
                .SetEase(Ease.OutQuad));
            chaseTextSequence.Append(chaseDisplayText.transform.DOScale(1f, chasePulseDuration * 0.5f)
                .SetEase(Ease.InQuad));
                
            // Color transition
            chaseTextSequence.Join(chaseDisplayText.DOColor(targetColor, chasePulseDuration));
            
            // Optional: Add emphasis for critical situations
            if (ballsRemaining <= 6 && runsNeeded > ballsRemaining * 1.5f) // Last over and difficult
            {
                chaseTextSequence.Append(chaseDisplayText.transform.DOShakePosition(0.2f, 2f, 8, 90, false, true));
            }
            
            lastRunsNeeded = runsNeeded;
            lastBallsRemaining = ballsRemaining;
        }
    }

    private Color GetChaseColor(int runsNeeded, int ballsRemaining)
    {
        if (runsNeeded <= 0) return comfortableChaseColor; // Target achieved
        if (ballsRemaining <= 0) return criticalChaseColor;

        float requiredRate = (float)runsNeeded / ballsRemaining;

        // Determine difficulty based on required run rate per ball
        if (requiredRate <= 0.8f) // Comfortable
            return comfortableChaseColor;
        else if (requiredRate <= 1.2f) // Normal
            return normalChaseColor;
        else if (requiredRate <= 1.5f) // Tight
            return tightChaseColor;
        else if (requiredRate <= 2f) // Difficult
            return difficultChaseColor;
        else // Critical
            return criticalChaseColor;
    }

    private void PlayCheeringSound()
    {
        if (cheeringSound == null)
        {
            Debug.LogWarning("Cheering sound not assigned!");
            return;
        }

        // Use dedicated audio source if available, otherwise use the main game audio source
        AudioSource audioSource = cheeringAudioSource != null ? cheeringAudioSource : gameAudioSource;

        if (audioSource != null)
        {
            // If using the same audio source as game sounds, we might want to stop current sound
            if (audioSource == gameAudioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(cheeringSound, cheeringVolume);
        }
        else
        {
            // Fallback: Play at the camera position if no audio source is set
            AudioSource.PlayClipAtPoint(cheeringSound, Camera.main.transform.position, cheeringVolume);
        }

        Debug.Log($"Cheering sound played for boundary!");
    }

        private void PlayBattingSound()
    {
        if (battingSound == null)
        {
            Debug.LogWarning("Batting sound not assigned!");
            return;
        }

        // Use dedicated audio source if available, otherwise use the main game audio source
        AudioSource audioSource = cheeringAudioSource != null ? cheeringAudioSource : gameAudioSource;

        if (audioSource != null)
        {
            // If using the same audio source as game sounds, we might want to stop current sound
            if (audioSource == gameAudioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(battingSound, cheeringVolume);
        }
        else
        {
            // Fallback: Play at the camera position if no audio source is set
            AudioSource.PlayClipAtPoint(battingSound, Camera.main.transform.position, cheeringVolume);
        }

        Debug.Log($"Batting sound played!");
    }

    private void PlayGroaningSound()
    {
        if (groaningSound == null)
        {
            Debug.LogWarning("Groaning sound not assigned!");
            return;
        }

        // Use dedicated audio source if available, otherwise use the main game audio source
        AudioSource audioSource = cheeringAudioSource != null ? cheeringAudioSource : gameAudioSource;

        if (audioSource != null)
        {
            // If using the same audio source as game sounds, we might want to stop current sound
            if (audioSource == gameAudioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(groaningSound, cheeringVolume);
        }
        else
        {
            // Fallback: Play at the camera position if no audio source is set
            AudioSource.PlayClipAtPoint(groaningSound, Camera.main.transform.position, cheeringVolume);
        }

        Debug.Log($"Groaning sound played on loosing wicket!");
    }

    private void PlayGameWonCheeringSound()
    {
        if (cheeringSound_gameWon == null)
        {
            Debug.LogWarning("Cheering sound not assigned!");
            return;
        }

        // Use dedicated audio source if available, otherwise use the main game audio source
        AudioSource audioSource = cheeringAudioSource != null ? cheeringAudioSource : gameAudioSource;

        if (audioSource != null)
        {
            // If using the same audio source as game sounds, we might want to stop current sound
            if (audioSource == gameAudioSource && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.PlayOneShot(cheeringSound_gameWon, cheeringVolume);
        }
        else
        {
            // Fallback: Play at the camera position if no audio source is set
            AudioSource.PlayClipAtPoint(cheeringSound_gameWon, Camera.main.transform.position, cheeringVolume);
        }

        Debug.Log($"Cheering sound played for boundary!");
    }
    
    bool targetReached = false;
    private Vector3 batterOriginalPosition;
    private Tween currentBatterTween;
    
    public void UpdateScore(int runs)
    {
        if (gameEnded) return;
        // Track consecutive boundaries
        if (runs == 4 || runs == 6)
        {
            consecutiveBoundaries++;
            Debug.Log($"Boundary hit! Consecutive boundaries: {consecutiveBoundaries}");
            PlayCheeringSound();

            // Check for hat-trick (3 boundaries in a row)
            if (consecutiveBoundaries >= 3)
            {
                TriggerHatTrickAnimation();
                consecutiveBoundaries = 0; // Reset after triggering
            }
        }
        else
        {
            // Reset streak if no boundary
            consecutiveBoundaries = 0;
        }

        if (runs > 0)
            currentRuns += runs;
        else if (runs == -3)
            currentRuns += 1;

        if (previousRuns < currentRuns)
        {
            AnimateScoreIncrease(previousRuns, currentRuns);
            previousRuns = currentRuns;
        }

        int _currentTurn = runs == -3 ? CardsPoolManager.Instance.CurrntTurn : CardsPoolManager.Instance.CurrntTurn + 1;
        if(runs != -3)
            UpdateBallsAndOvers(_currentTurn);
            
         if (!isBattingFirst && EncouragementSystem.Instance != null)
        {
            int _ballsRemaining = runs == -3 ? MaxBalls - CardsPoolManager.Instance.CurrntTurn : MaxBalls - CardsPoolManager.Instance.CurrntTurn - 1;
            EncouragementSystem.Instance.CheckMilestones(currentRuns-currentGameplayConfig.initialScore, TargetScore-currentGameplayConfig.initialScore);
        }

        if (isBattingFirst)
        {
            Debug.Log("updating score display text");
            totalRunsNeededText.text = "";
            scoreText.text = "Score: " + currentRuns.ToString();
        }
        else
        {
            Debug.Log("updating score display text while chasing to " + currentRuns);
            totalRunsNeededText.text = "/ " + TargetScore;
            scoreText.text = "Score: " + currentRuns + " / " + TargetScore.ToString();
            Canvas.ForceUpdateCanvases();


        }

        // Update chase display after scoring
        int runsNeeded = TargetScore - currentRuns;
        int ballsRemain = runs == -3 ? MaxBalls - CardsPoolManager.Instance.CurrntTurn : MaxBalls - CardsPoolManager.Instance.CurrntTurn - 1;
        // dont update on first iteration 
         if (canUpdateChaseDisplay)
        {
            UpdateChaseDisplay(runsNeeded, ballsRemain);
        }
        else
        {
            UpdateChaseDisplay(runsNeeded, MaxBalls);
            canUpdateChaseDisplay = true;
        }
        
        remainingWicketsText.text = wickets.ToString();
        
        // Check if target is reached
        if (currentRuns >= currentGameplayConfig.winScore && !targetReached)
        {
            targetReached = true;
            HandleGameOver("won", "Target reached! You win!");
        }
        
        if (runs == -1)
        {
            LooseWicket();
        }
    }
    
    public void LooseWicket()
    {
        wickets--;
        PlayGroaningSound();
        AnimateWicketLoss(previousWickets, wickets);
        // musicIntensity.SetExcitement(.45f);
        
        // Reset consecutive boundaries on wicket loss
        if (consecutiveBoundaries > 0)
        {
            Debug.Log($"Wicket lost - consecutive boundaries streak of {consecutiveBoundaries} ended");
            consecutiveBoundaries = 0;
        }
        
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
        if (YarnDialogSystemSingleTonMaker.instance == null)
        {
            Debug.LogError("YarnDialogSystemSingleTonMaker.instance is null");
            return;
        }

        var storage = YarnDialogSystemSingleTonMaker.instance.dialogueRunner.VariableStorage;
        if(storage == null)
        {
            Debug.LogError("could not set yarn variable");
            return;
        }
        
        // Set gameplay outcome variables
        storage.SetValue("$gameplayNumber", currentGameplayConfig.gameplayNumber); // Changed from $gameplayDate to $gameplayNumber
        storage.SetValue("$finalScore", currentRuns);
        storage.SetValue("$targetScore", TargetScore);
        storage.SetValue("$wicketsLost", initialWickets - wickets);
        storage.SetValue("$gameResult", result);
        storage.SetValue("$initialScore", currentGameplayConfig.initialScore);
        
        Debug.Log($"Yarn Variables Set - Gameplay Number: {currentGameplayConfig.gameplayNumber}, Score: {currentRuns}, initialScore: {currentGameplayConfig.initialScore}, " +
                  $"Target: {TargetScore}, Wickets Lost: {initialWickets - wickets}, Result: {result}");
    }
    
    private IEnumerator EndGameAfterDelay()
    {
        //check if player won the match
        if(currentGameplayConfig.winScore <= currentRuns)
        {
            PlayGameWonCheeringSound();
            TriggerFirework();
            // musicIntensity.SetExcitement(.5f);
            yield return new WaitForSeconds(5f);
        }
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
        
        
        BallThrow currentBallThrow = CardsPoolManager.Instance.CurrentBallThrow;
        PitchCondition pitchCondition = currentBallThrow.pitchCondition;
        Debug.Log($"Current Ball Throw: \n{currentBallThrow}\n Pitch Condition: {pitchCondition}");
        OutcomeResult outcomeResult = ExcelDataSOManager.Instance.outComeCalculator.CalculateOutcome(
            battingStrategy, currentBallThrow, BattingTiming.Perfect, pitchCondition);

        OutCome outcome = outcomeResult.outcome;
        string commentary = outcomeResult.commentary;
        Debug.Log($"Commentary for outcome: " + commentary);
        //on wide
        if((int)outcome == -3)
        {
            OnWideBall.Invoke(-3);
        }
        //play batting sound
        if((int)outcome != -3 && (int)outcome != -4)
            gameAudioSource.PlayOneShot(battingSound, 1f);
        if((int)outcome >=4)
        {
            TriggerFirework();
        }
        if (CardPlayAnimationController.Instance != null)
        {
            yield return CardPlayAnimationController.Instance.PlayCardSequence(
                cardObject, cardSprite, battingStrategy, outcome, commentary);
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
            
            // Wait extra time if hat-trick animation is triggered
            if (hattrickTriggered)
            {
                Debug.Log("Waiting extra time for hat-trick animation to complete");
                // The animation already pauses the game, so we just need to wait for it to complete
                // Total wait time: 0.5 (scale up) + 0.9 (3 flips) + 0.5 (punch) + 3 (stay) + 0.5 (scale down) = ~5.4 seconds
                yield return new WaitForSeconds(2.5f);
                hattrickTriggered = false;
            }
            
            if ((int)outcome >= 4)
                yield return new WaitForSeconds(2f); //wait for the fireworks animation to complete
            if (!isBattingFirst && EncouragementSystem.Instance != null)
            {
                EncouragementSystem.Instance.TryShowPendingEncouragement();

                // Wait for encouragement to finish if it's showing
                while (EncouragementSystem.Instance != null && EncouragementSystem.Instance.isShowingEncouragement)
                {
                    // Debug.Log("isShowingEncouragment Panel " + EncouragementSystem.Instance.isShowingEncouragement);
                    yield return null;
                }
            }

            Timer.Instance.PauseTimer();
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

        // Get gameplay configuration
        currentGameplayConfig = GameplayConfiguration.Instance.GetCurrentGameplayConfig();
        if (currentGameplayConfig != null)
        {
            TargetScore = currentGameplayConfig.winScore;
            MaxBalls = currentGameplayConfig.balls;
            isBattingFirst = currentGameplayConfig.isBattingFirst;
            currentRuns = currentGameplayConfig.initialScore;

            Debug.Log($"Gameplay {currentGameplayConfig.gameplayNumber} - Date: {currentGameplayConfig.date}");
            Debug.Log($"Batting First: {isBattingFirst}, CurrentRuns: {currentRuns}, Target: {TargetScore}, Balls: {MaxBalls}");
        }
        else
        {
            Debug.Log("Loading gameplay 2 as date is not found");
            currentGameplayConfig = GameplayConfiguration.Instance.GetConfigForDate("1990/04/13");
            TargetScore = currentGameplayConfig.winScore;
            MaxBalls = currentGameplayConfig.balls;
            isBattingFirst = currentGameplayConfig.isBattingFirst;
            currentRuns = currentGameplayConfig.initialScore;
            Debug.Log($"Gameplay {currentGameplayConfig.gameplayNumber} - Date: {currentGameplayConfig.date}");
            Debug.Log($"Batting First: {isBattingFirst}, CurrentRuns: {currentRuns}, Target: {TargetScore}, Balls: {MaxBalls}");

        }

        previousRuns = currentRuns;
        previousWickets = wickets;
        previousBallsRemaining = MaxBalls;

        // Update UI based on game mode
        if (isBattingFirst)
        {
            musicIntensity.SetExcitement(.65f);
            totalRunsNeededText.text = "";
            currentRunsText.text = currentRuns.ToString();
            totalWicketsText.text = "/ " + wickets.ToString();
            remainingWicketsText.text = wickets.ToString();
        }
        else
        {
            musicIntensity.SetExcitement(.55f);
            currentRunsText.text = currentRuns.ToString();
            totalRunsNeededText.text = "/ " + TargetScore.ToString();
            totalWicketsText.text = "/ " + wickets.ToString();
            remainingWicketsText.text = wickets.ToString();
        }

        UpdateScore(0);
        UpdateBallsAndOvers(0);

        if (BatterImage != null)
        {
            batterOriginalPosition = BatterImage.rectTransform.anchoredPosition;
        }

        // Reset encouragement system if chasing
        if (!isBattingFirst && EncouragementSystem.Instance != null)
        {
            EncouragementSystem.Instance.ResetMilestones();
        }

        // Subscribe to the turn started event to show chase display after countdown
        if (chaseDisplayText != null)
        {
            CardsPoolManager.OnTurnStarted += ShowChaseDisplayAfterCountdown;
        }

        if(redrawButton != null)
        {
            redrawButton.onClick.AddListener(OnRedrawButtonClicked);
            //update redraw button state when turn starts
            CardsPoolManager.OnTurnStarted += UpdateRedrawButton;

        }
     }

    private void AnimateScoreIncrease(int fromScore, int toScore)
    {
        if (currentRunsText == null) return;

        // Kill any existing animations on this text
        currentRunsText.DOKill(true);

        // Number counter animation
        DOTween.To(() => fromScore, x =>
        {
            currentRunsText.text = x.ToString();
        }, toScore, scoreAnimDuration).SetEase(Ease.OutQuad);

        // Pulse and color animation
        Sequence seq = DOTween.Sequence();
        seq.Append(currentRunsText.transform.DOScale(1.3f, scoreAnimDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Join(currentRunsText.DOColor(scoreIncreaseColor, scoreAnimDuration * 0.3f));
        seq.Append(currentRunsText.transform.DOScale(1f, scoreAnimDuration * 0.6f).SetEase(Ease.InOutQuad));
        seq.Join(currentRunsText.DOColor(Color.white, scoreAnimDuration * 0.7f));

        // Optional: Add a punch rotation for boundaries
        int runsScored = toScore - fromScore;
        if (runsScored >= 4)
        {
            currentRunsText.transform.DOPunchRotation(new Vector3(0, 0, 15f), scoreAnimDuration, 10, 1f);
        }

        // Update score text as well if chasing
        if (!isBattingFirst && scoreText != null)
        {
            scoreText.text = "Score: " + toScore.ToString() + " / " + TargetScore.ToString();
            scoreText.DOKill(true);
            scoreText.transform.DOPunchScale(Vector3.one * 0.2f, scoreAnimDuration * 0.5f, 5, 0.5f);
        }
    }

    private void AnimateWicketLoss(int fromWickets, int toWickets)
    {
        if (remainingWicketsText == null) return;
        
        // Kill any existing animations
        remainingWicketsText.DOKill(true);
        
        // Update text
        remainingWicketsText.text = toWickets.ToString();
        
        // Dramatic wicket loss animation
        Sequence seq = DOTween.Sequence();
        
        // Flash red
        seq.Append(remainingWicketsText.DOColor(wicketLossColor, wicketAnimDuration * 0.2f));
        
        // Shake effect
        seq.Join(remainingWicketsText.transform.DOShakePosition(wicketAnimDuration * 0.5f, 10f, 20, 90, false, true));
        
        // Scale bounce
        seq.Join(remainingWicketsText.transform.DOScale(1.5f, wicketAnimDuration * 0.3f).SetEase(Ease.OutQuad));
        seq.Append(remainingWicketsText.transform.DOScale(0.8f, wicketAnimDuration * 0.2f));
        seq.Append(remainingWicketsText.transform.DOScale(1f, wicketAnimDuration * 0.2f).SetEase(Ease.OutBounce));
        
        // Return to white color
        seq.Join(remainingWicketsText.DOColor(Color.white, wicketAnimDuration * 0.5f));
        
        // Optional: Flash the background or add a vignette effect if you have one
        Debug.Log($"Wicket lost! Remaining: {toWickets}");
    }
    
    private void AnimateBallsDecrease(int ballsRemaining)
    {
        if (remainingBallsText == null) return;
        
        // Kill any existing animations
        remainingBallsText.DOKill(true);
        
        // Update text
        remainingBallsText.text = ballsRemaining.ToString();
        
        // Subtle animation for ball count decrease
        Sequence seq = DOTween.Sequence();
        
        // Quick scale down then back
        seq.Append(remainingBallsText.transform.DOScale(0.7f, ballAnimDuration * 0.3f).SetEase(Ease.InQuad));
        seq.Append(remainingBallsText.transform.DOScale(1.1f, ballAnimDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(remainingBallsText.transform.DOScale(1f, ballAnimDuration * 0.3f));
        
        // Color flash based on urgency
        Color targetColor = ballDecreaseColor;
        if (ballsRemaining <= 6) // Last over
        {
            targetColor = difficultChaseColor;
            // Add extra shake for last 6 balls
            seq.Join(remainingBallsText.transform.DOShakeRotation(ballAnimDuration, new Vector3(0, 0, 10f), 5, 90));
        }
        else if (ballsRemaining <= 12) // Last 2 overs
        {
            targetColor = tightChaseColor;
        }
        
        seq.Insert(0, remainingBallsText.DOColor(targetColor, ballAnimDuration * 0.4f));
        seq.Insert(ballAnimDuration * 0.6f, remainingBallsText.DOColor(Color.white, ballAnimDuration * 0.4f));
    }
    void ShowChaseDisplayAfterCountdown()
    {
        if (chaseDisplayText != null && chaseDisplayContainer != null)
        {
            chaseDisplayContainer.SetActive(true);
            chaseDisplayText.gameObject.SetActive(true);
        }
        //show pause intruction
        if(gamePuaseInstructionText != null)
            gamePuaseInstructionText.SetActive(true);
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