using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{


    public static Timer Instance;
    public int maxTimeToChooseStrategy = 5; // seconds
    private bool isPaused = false;
    private float pausedTimeRemaining = 0;
    private Coroutine currentTimerCoroutine;

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] TextMeshProUGUI timerText;
    // Start is called before the first frame update
    void Start()
    {
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
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
        isPaused = false;
        pausedTimeRemaining = maxTimeToChooseStrategy;
        currentTimerCoroutine = StartCoroutine(TimerCoroutine(maxTimeToChooseStrategy));
    }

    public void EndTurnTimer()
    {
        StopAllCoroutines();
        timerText.text = "" + maxTimeToChooseStrategy.ToString() + "s";
    }

    IEnumerator TimerCoroutine(int duration)
    {
        float timeLeft = duration;
        pausedTimeRemaining = timeLeft;
        
        while (timeLeft > 0)
        {
            pausedTimeRemaining = timeLeft;
            timerText.text = "" + Mathf.CeilToInt(timeLeft).ToString() + "s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }
        
        timerText.text = "Time's Up!";
        CardsPoolManager.Instance.EndTurn(true);
    }
}
