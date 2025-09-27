using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{


    public static Timer Instance;
    public int baseTimeToChooseStrategy = 15; 
    private int maxTimeToChooseStrategy = 5; // seconds
    private bool isPaused = false;
    private float pausedTimeRemaining = 0;
    private Coroutine currentTimerCoroutine;

    void Awake()
    {
        //courange, foresight, humility, resourcefullness
        maxTimeToChooseStrategy = baseTimeToChooseStrategy + GameManager.instance.currentSaveData.foresight;
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
