using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public static Timer Instance;
    
    [Header("Timer Settings")]
    public int baseTimeToChooseStrategy = 15; 
    private int maxTimeToChooseStrategy = 5; // seconds
    private bool isPaused = false;
    private float pausedTimeRemaining = 0;
    private Coroutine currentTimerCoroutine;
    private Coroutine countdownCoroutine;
    
    [Header("UI References")]
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject countdownPanel; // Panel to show countdown
    [SerializeField] TextMeshProUGUI countdownText; // Text for 3, 2, 1 countdown
    [SerializeField] GameObject overInfoPanel; // Panel to show over info
    [SerializeField] TextMeshProUGUI overInfoText; // Text for over information
    
    [Header("Countdown Settings")]
    [SerializeField] float countdownDuration = 1f; // Duration each number shows
    [SerializeField] AnimationCurve countdownScaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
    
    void Awake()
    {
        //courage, foresight, humility, resourcefulness
        if (GameManager.instance != null)
            maxTimeToChooseStrategy = baseTimeToChooseStrategy + GameManager.instance.currentSaveData.foresight;
        else 
            maxTimeToChooseStrategy = baseTimeToChooseStrategy;
        Instance = this;
    }

    void Start()
    {
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
        
        // Hide countdown and over info panels initially
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
        if (overInfoPanel != null)
            overInfoPanel.SetActive(false);
    }

    public void PauseTimer()
    {
        isPaused = true;
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
        }
    }

    public void ResumeTimer()
    {
        if (isPaused && pausedTimeRemaining > 0)
        {
            isPaused = false;
            currentTimerCoroutine = StartCoroutine(TimerCoroutine(Mathf.CeilToInt(pausedTimeRemaining)));
        }
    }
    
    public void StartTurnTimer()
    {
        // Stop any existing countdown
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        // Start the countdown sequence
        countdownCoroutine = StartCoroutine(StartTurnWithCountdown());
    }
    
    private IEnumerator StartTurnWithCountdown()
    {
        // Disable card interactions during countdown
        CardsPoolManager.Instance.SetCardsInteractable(false);
        
        // Check if it's the first ball of an over
        int currentBall = CardsPoolManager.Instance.CurrntTurn;
        bool isFirstBallOfOver = (currentBall % 6 == 0);
        
        // Show over information if it's the first ball of an over
        if (isFirstBallOfOver && overInfoPanel != null && overInfoText != null)
        {
            int overNumber = (currentBall / 6) + 1;
            overInfoText.text = $"OVER {overNumber}";
            overInfoPanel.SetActive(true);
            
            // Animate over info (optional)
            yield return AnimateOverInfo();
            
            yield return new WaitForSeconds(1.5f);
            overInfoPanel.SetActive(false);
        }
        
        // Show countdown panel
        if (countdownPanel != null && countdownText != null)
        {
            countdownPanel.SetActive(true);
            
            // Countdown from 3 to 1
            for (int i = 3; i >= 1; i--)
            {
                countdownText.text = i.ToString();
                
                // Animate countdown number
                yield return AnimateCountdownNumber();
            }
            
            // Show "GO!" or "PLAY!"
            countdownText.text = "PLAY!";
            yield return AnimateCountdownNumber();
            
            countdownPanel.SetActive(false);
        }
        else
        {
            // Fallback if UI elements are not set
            Debug.LogWarning("Countdown UI elements not configured. Starting timer directly.");
        }
        
        // Re-enable card interactions
        CardsPoolManager.Instance.SetCardsInteractable(true);
        
        // Start the main timer
        isPaused = false;
        pausedTimeRemaining = maxTimeToChooseStrategy;
        currentTimerCoroutine = StartCoroutine(TimerCoroutine(maxTimeToChooseStrategy));
        
        countdownCoroutine = null;
    }
    
    private IEnumerator AnimateCountdownNumber()
    {
        if (countdownText == null) yield break;
        
        float elapsed = 0;
        Vector3 originalScale = Vector3.one;
        
        while (elapsed < countdownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / countdownDuration;
            
            // Apply scale animation
            float scale = countdownScaleCurve.Evaluate(t);
            countdownText.transform.localScale = originalScale * scale;
            
            // Fade out near the end
            if (t > 0.7f)
            {
                Color color = countdownText.color;
                color.a = 1f - ((t - 0.7f) / 0.3f);
                countdownText.color = color;
            }
            else
            {
                Color color = countdownText.color;
                color.a = 1f;
                countdownText.color = color;
            }
            
            yield return null;
        }
        
        // Reset for next number
        countdownText.transform.localScale = originalScale;
        Color finalColor = countdownText.color;
        finalColor.a = 1f;
        countdownText.color = finalColor;
    }

    private IEnumerator AnimateOverInfo()
    {
        if (overInfoText == null) yield break;
        yield return new WaitForSeconds(0f); // Initial delay
        // Simple scale animation for over info
        float duration = 1.5f;
        float elapsed = 0;
        Vector3 originalScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Lerp(0.8f, 1.1f, t);
            overInfoText.transform.localScale = originalScale * scale;

            yield return null;
        }

        overInfoText.transform.localScale = originalScale;
        yield return new WaitForSeconds(1f);
    }
    
    public void ResetTimerForRedraw()
    {
        // Stop current timer
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }
        
        // Reset and restart the timer
        isPaused = false;
        pausedTimeRemaining = maxTimeToChooseStrategy;
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
        
        // Start a new timer without countdown
        currentTimerCoroutine = StartCoroutine(TimerCoroutine(maxTimeToChooseStrategy));
    }

    public void EndTurnTimer()
    {
        isPaused = false;
        
        // Stop countdown if running
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
        
        // Stop main timer
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }
        
        // Hide countdown panel if visible
        if (countdownPanel != null)
            countdownPanel.SetActive(false);
        if (overInfoPanel != null)
            overInfoPanel.SetActive(false);
            
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
    }

    IEnumerator TimerCoroutine(int duration)
    {
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            pausedTimeRemaining = timeLeft;
            timerText.text = Mathf.CeilToInt(timeLeft).ToString() + "s";
            
            // Visual warning when time is running out
            if (timeLeft <= 5)
            {
                // Make text red and pulse
                timerText.color = Color.Lerp(Color.white, Color.red, (5 - timeLeft) / 5);
            }
            else
            {
                timerText.color = Color.white;
            }
            
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        currentTimerCoroutine = null;
        timerText.color = Color.red;
        timerText.text = "Time's Up!";
        yield return new WaitForSeconds(1f);
        timerText.text = "";
       if (ScoreManager.Instance != null)
        {
            Debug.Log("Time's up! Losing a wicket.");
            ScoreManager.Instance.LooseWicket();

            // Show timeout message (optional)
            if (overInfoPanel != null && overInfoText != null)
            {
                overInfoText.text = "TIMEOUT - WICKET LOST!";
                overInfoPanel.SetActive(true);
                yield return new WaitForSeconds(2f);
                overInfoPanel.SetActive(false);
            }
        }
        if (ScoreManager.Instance.getCurrentWickets() > 0)
        {
            CardsPoolManager.Instance.EndTurn(true);
            yield return new WaitForSeconds(1f);
            CardsPoolManager.Instance.StartTurn(true);
        }
        // CardsPoolManager.Instance.EndTurn(true);
        // yield return new WaitForSeconds(3f);
        // CardsPoolManager.Instance.StartTurn(true);
    }
}