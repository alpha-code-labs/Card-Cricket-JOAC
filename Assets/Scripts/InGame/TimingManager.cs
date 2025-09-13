using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;
using DG.Tweening;

public class TimingManager : MonoBehaviour
{
    public static TimingManager Instance;
    [SerializeField] BattingStrategy testStrategy;
    [SerializeField] BattingTiming testTiming;
    [SerializeField] BallThrow testBall;
    [ContextMenu("Test Outcome Calculation")]
    void Test()
    {
        OutCome outcome = outcomeCalculator.CalculateOutcome(testStrategy, testBall, testTiming);
        Debug.Log($"Test Outcome: {outcome}");
    }
    Image bg;
    void Start()
    {
        bg = GetComponent<Image>();
        bg.enabled = false;
    }
    void Awake()
    {
        Instance = this;
    }
    [SerializeField] Image BAR;
    [SerializeField] Image MARKER;
    bool isTimingMeterActive = false;

    //
    BattingStrategy battingStrategy;
    public void StartTimingMeter(BattingStrategy strategy)
    {
        battingStrategy = strategy;
        bg.enabled = true;
        float totalHeight = BAR.rectTransform.rect.height;
        MARKER.rectTransform.anchoredPosition = new Vector2(0, -totalHeight / 2);

        // Start the timing meter coroutine
        StartCoroutine("TimingMeterCoroutine");
    }

    IEnumerator TimingMeterCoroutine()
    {
        isTimingMeterActive = true;
        float speed = 2f; // Adjust this value to control speed (higher = faster)
        float direction = 1f; // 1 for going up, -1 for going down
        float halfHeight = BAR.rectTransform.rect.height / 2;

        while (true)
        {
            // Get current Y position
            float currentY = MARKER.rectTransform.anchoredPosition.y;

            // Move the marker
            currentY += direction * speed * halfHeight * Time.deltaTime;

            // Check if we hit the top
            if (currentY >= halfHeight)
            {
                currentY = halfHeight;
                direction = -1f; // Start moving down
            }
            // Check if we hit the bottom
            else if (currentY <= -halfHeight)
            {
                currentY = -halfHeight;
                direction = 1f; // Start moving up
            }

            // Apply the new position
            MARKER.rectTransform.anchoredPosition = new Vector2(0, currentY);

            yield return null; // Wait for next frame
        }
    }
    void StopTimingMeter()
    {
        bg.enabled = false;
        isTimingMeterActive = false;
        StopCoroutine("TimingMeterCoroutine");
        BattingTiming timing = GetTiming();
        SendOutcome(timing);
    }

    [SerializeField] TextMeshProUGUI outcomeText;
    [SerializeField] OutComeCalculator outcomeCalculator;
    [ContextMenu("Rebuild Lookup Dictionary")]
    public void RebuldLookupDictionary()
    {
        outcomeCalculator.LoadFromExcel();
        outcomeCalculator.BuildLookupDictionary();
    }
    void SendOutcome(BattingTiming timing)
    {
        OutCome outcome = outcomeCalculator.CalculateOutcome(battingStrategy, CardsPoolManager.Instance.CurrentBallThrow, timing);

        // Assuming outcome.Runs is the number of runs scored
        int runsScored = (int)outcome;
        if (runsScored < 0)
        {
            Debug.LogWarning("Negative runs scored, You Lost A Wicket.");
            runsScored = 0; // Prevent negative runs
        }
        ScoreManager.Instance.UpdateScore(runsScored);

        // Logic to handle the outcome, e.g., updating the game state
        Debug.Log($"Played card: {battingStrategy}, Runs Scored: {runsScored}, Timing: {timing}, Outcome: {outcome} Against Ball: \n{CardsPoolManager.Instance.CurrentBallThrow}");
        outcomeText.text = $"Played card: {battingStrategy},\n Timing: {timing},\n Outcome: {outcome},\n Runs Scored: {runsScored} ";
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

        //
        CardsPoolManager.Instance.EndTurn(); // End turn after playing the card
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isTimingMeterActive)
        {
            StopTimingMeter();
        }
    }


    [ContextMenu("Test Get Timing")]
    public BattingTiming GetTiming()
    {
        float timingValue = MARKER.rectTransform.anchoredPosition.y / (BAR.rectTransform.rect.height / 2);
        // Debug.Log($"Timing Value: {timingValue:F2}");        
        float a = Mathf.Abs(timingValue);
        if (a <= 0.1f) return BattingTiming.Perfect;
        if (a <= 0.3f) return BattingTiming.VeryGood;
        if (a <= 0.5f) return BattingTiming.Good;
        if (a <= 0.7f) return BattingTiming.Average;
        if (a <= 0.9f) return BattingTiming.Poor;
        if (a <= 1f) return BattingTiming.Poor;// Added this line to handle edge case LITRALY!
        Debug.LogWarning("Timing out of range, defaulting to Perfect.");
        return BattingTiming.Perfect; // TEMP
    }
}
