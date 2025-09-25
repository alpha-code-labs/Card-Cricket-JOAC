using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer_Tutorial : MonoBehaviour
{


    public static Timer_Tutorial Instance;
    public int maxTimeToChooseStrategy = 5; // seconds
    private bool isPaused = false;
    private float pausedTimeRemaining = 0;
    private Coroutine currentTimerCoroutine;

    void Awake()
    {
        Instance = this;
    }

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject TimerParent;
    // Start is called before the first frame update
    void Start()
    {
        TimerParent.SetActive(false);
        //timerText.text = maxTimeToChooseStrategy.ToString() + "s";
    }

    public void ShowTimingPanel()
    {
        TimerParent.SetActive(true);
        timerText.text = maxTimeToChooseStrategy.ToString() + "s";
        UIHighlightManager.Instance.HighlightObject(TimerParent);
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
        isPaused = false;
        if (currentTimerCoroutine != null)
        {
            StopCoroutine(currentTimerCoroutine);
            currentTimerCoroutine = null;
        }
        timerText.text = "" + maxTimeToChooseStrategy.ToString() + "s";
    }

    IEnumerator TimerCoroutine(int duration)
    {
        float timeLeft = duration;

        while (timeLeft > 0)
        {
            pausedTimeRemaining = timeLeft;
            timerText.text = "" + Mathf.CeilToInt(timeLeft).ToString() + "s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        currentTimerCoroutine = null;

        timerText.text = "Time's Up!";
        CardsPoolManager.Instance.EndTurn(true);
        yield return new WaitForSeconds(3f);
        CardsPoolManager.Instance.StartTurn(true);
    }
}
