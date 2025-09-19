using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{


    public static Timer Instance;
    public int maxTimeToChooseStrategy = 5; // seconds
    void Awake()
    {
        Instance = this;
    }

    [SerializeField] TextMeshProUGUI timerText;
    // Start is called before the first frame update
    void Start()
    {
        timerText.text = "Time Left: " + maxTimeToChooseStrategy.ToString() + "s";
    }
    public void StartTurnTimer()
    {
        StartCoroutine(TimerCoroutine(maxTimeToChooseStrategy));
    }

    public void EndTurnTimer()
    {
        StopAllCoroutines();
        timerText.text = "Time Left: " + maxTimeToChooseStrategy.ToString() + "s";
    }

    IEnumerator TimerCoroutine(int duration)
    {
        float timeLeft = duration;
        while (timeLeft > 0)
        {
            timerText.text = "Time Left: " + Mathf.CeilToInt(timeLeft).ToString() + "s";
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }
        timerText.text = "Time's Up!";
        // Handle time up scenario, e.g., auto-select a strategy or end turn
        CardsPoolManager.Instance.EndTurn(true);
    }
}
