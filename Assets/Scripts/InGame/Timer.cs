using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using System;

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
    [SerializeField] GameObject overInfoPanel; // Panel to show over info
    [SerializeField] TextMeshProUGUI overInfoText; // Text for over information
    public static Action onFirstOverAnimationComplete;
    
    void Awake()
    {
        //courage, foresight, humility, resourcefulness
        if (GameManager.instance != null)
            maxTimeToChooseStrategy = baseTimeToChooseStrategy + GameManager.instance.currentSaveData.foresight*5;
        else 
            maxTimeToChooseStrategy = baseTimeToChooseStrategy;
        Instance = this;
    }

    void Start()
    {
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
        
        // Hide over info panel initially
        if (overInfoPanel != null)
            overInfoPanel.SetActive(false);
    }

    public void PauseTimer()
    {
        if (isPaused) return;
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
        if (currentBall > 5 && isFirstBallOfOver && overInfoPanel != null && overInfoText != null)
        {
            int overNumber = (currentBall / 6) + 1;
            overInfoText.text = $"OVER {overNumber}";
            overInfoPanel.SetActive(true);

            // Animate over info (optional)
            yield return AnimateOverInfo();
            if( onFirstOverAnimationComplete != null && currentBall < 6)
            onFirstOverAnimationComplete?.Invoke();
            overInfoPanel.SetActive(false);
        }

        // Re-enable card interactions
        CardsPoolManager.Instance.SetCardsInteractable(true);

        // Start the main timer
        isPaused = false;
        pausedTimeRemaining = maxTimeToChooseStrategy;
        currentTimerCoroutine = StartCoroutine(TimerCoroutine(maxTimeToChooseStrategy));

        countdownCoroutine = null;
    }
    
    private IEnumerator AnimateOverInfo()
    {
        if (overInfoText == null) yield break;
        yield return new WaitForSeconds(0f); // Initial delay
        // Simple scale animation for over info
        float duration = 1f;
        float elapsed = 0;
        Vector3 originalScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Lerp(0.7f, 1f, t);
            overInfoText.transform.localScale = originalScale * scale;

            yield return null;
        }
        overInfoText.transform.localScale = originalScale;
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
            ScoreManager.Instance.UpdateBallsAndOvers(CardsPoolManager.Instance.CurrntTurn+1);

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
            CardsPoolManager.Instance.EndTurn(ScoreManager.Instance.MaxBalls, true);
            yield return new WaitForSeconds(1f);
            CardsPoolManager.Instance.StartTurn(true);
        }
        // CardsPoolManager.Instance.EndTurn(true);
        // yield return new WaitForSeconds(3f);
        // CardsPoolManager.Instance.StartTurn(true);
    }

    internal class Instannce
    {
    }
}