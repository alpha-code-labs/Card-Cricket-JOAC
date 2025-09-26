using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Yarn.Unity;

public class NewDayManager : MonoBehaviour
{
    public static NewDayManager instance;
    void Awake()
    {
        instance = this;
    }
    TextMeshProUGUI dateText;
    public static DateRecord currentDateRecord;
    public static int currentEventIndex = 0;//Dont Modify This Directly you are probably making a mistake if you want to
    public static bool isEvening = false;
    void Start()
    {
        dateText = GetComponentInChildren<TextMeshProUGUI>();
        BeginNewDaySequence();
    }
    public void BeginNewDaySequence()
    {
        Debug.Log("Beginning New Day Sequence");
        currentDateRecord = CalanderSystem.instance.GetDateRecordFromDate(GameManager.instance.currentSaveData.currentDate);
        StartCoroutine(StartEventWithTransition());
    }

    IEnumerator StartEventWithTransition()
    {
        string prettyDate = PrettyStrings.GetPrettyDateString(GameManager.instance.currentSaveData.currentDate);
        if (currentEventIndex >= currentDateRecord.events.Count)
        {
            yield return DisplayTextThenFade(prettyDate + "\n Day End");
            EndDay();
            yield break;
        }
        EventRecord events = currentDateRecord.events[currentEventIndex];
        if (currentEventIndex == 0)
        {
            SetFilmGrain(true);
            DateTime previousDate = CalanderSystem.instance.GetPreviousDateTime(GameManager.instance.currentSaveData.currentDate);
            DateTime currentDate = DateTime.Parse(GameManager.instance.currentSaveData.currentDate);
            yield return AnimateDateProgression(previousDate, currentDate);
        }
        else
        {
            SetFilmGrain(false);
            dateText.text = "";
        }

        Debug.Log($"Starting Event: {events.eventName} of type {events.eventType}");

        switch (events.eventType)
        {
            case TypeOfEvent.ForcedCutscene:
                if (currentEventIndex != 0)
                    yield return DisplayTextThenFade("");//remove this if you dont want to proprly wait and want transistions to be fast
                DialogueScriptCommandHandler.currentNode = events.eventName;
                TransitionScreenManager.instance.LoadScene(SceneNames.CutsceneScene);
                // TransitionScreenManager.instance.LoadScene("yarn-test");
                break;
            case TypeOfEvent.FreeTime:
                string timeOfDay = isEvening ? "Evening" : "Day";
                yield return DisplayTextThenFade($"Free Time\n{timeOfDay}");
                TransitionScreenManager.instance.LoadScene(SceneNames.WorldNav);
                break;
            case TypeOfEvent.Speical:
                yield return DisplayTextThenFade("");
                //Load Special Event
                break;
            case TypeOfEvent.SkipDayOrEvening:
                if (isEvening)
                    EndDay();
                isEvening = true;
                currentEventIndex++;
                BeginNewDaySequence();
                //Skip to next day or evening
                break;
            case TypeOfEvent.GamePlay:
                yield return DisplayTextThenFade("");
                ScoreManager.Instance.SetTargetFromEventName(events.eventName);
                TransitionScreenManager.instance.LoadScene(SceneNames.CardGameScene);
                //Load GamePlay                
                break;
            default:
                Debug.LogError("No event type found");
                break;
        }
    }
    IEnumerator DisplayTextThenFade(string textToDisplay, float displayDuration = 2f, float fadeDuration = 1f)
    {
        dateText.text = textToDisplay;
        if (dateText.alpha != 1)
        {
            yield return new WaitForSeconds(displayDuration);
            yield return dateText.DOFade(1f, fadeDuration).WaitForCompletion();
        }
        yield return new WaitForSeconds(displayDuration);
        yield return dateText.DOFade(0f, fadeDuration).WaitForCompletion();
    }

    IEnumerator AnimateDateProgression(DateTime startDate, DateTime endDate)
    {
        // Calculate the total number of days between dates
        int totalDays = (endDate - startDate).Days;

        // Start with the previous date
        DateTime currentAnimatedDate = startDate;
        string currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");
        dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);

        // Make sure the text is visible
        if (dateText.alpha != 1f)
        {
            yield return dateText.DOFade(1f, 0.3f).WaitForCompletion();
        }

        // Hold at the starting date for 1 second
        yield return new WaitForSeconds(1f);

        // Animate through each day with variable speed
        for (int i = 0; i < totalDays; i++)
        {
            currentAnimatedDate = startDate.AddDays(i + 1);
            currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");

            // Calculate speed based on position in the sequence using exponential acceleration
            float timeForThisDay;

            if (totalDays <= 2)
            {
                // For short sequences, use moderate speed
                timeForThisDay = 0.3f;
            }
            else
            {
                // Use exponential curve for dramatic acceleration/deceleration
                // Map i to a range from 0 to 1
                float normalizedPosition = (float)i / (totalDays - 1); // 0 to 1

                // Create exponential curve that goes from slow -> instant -> slow
                // Using distance from center (0.5) to create symmetrical exponential decay
                float distanceFromCenter = Mathf.Abs(normalizedPosition - 0.5f) * 2f; // 0 to 1 (0 at center, 1 at edges)

                // Exponential function: e^(4x) where x goes from 0 (center) to 1 (edges)
                // This creates dramatic exponential acceleration towards the center
                float exponentialValue = Mathf.Exp(4f * distanceFromCenter);

                // Map exponential value to time range [0, 0.6]
                // At center: distanceFromCenter = 0, exp(0) = 1, timeForThisDay ≈ 0
                // At edges: distanceFromCenter = 1, exp(4) ≈ 54.6, timeForThisDay = 0.6
                float maxTime = 0.6f;  // Slow at edges
                timeForThisDay = (exponentialValue - 1f) / (Mathf.Exp(5f) - 1f) * maxTime;

                // Clamp to ensure we don't go negative and have a tiny minimum
                timeForThisDay = Mathf.Max(timeForThisDay, 0.001f);
            }

            // Create a smooth transition effect with variable timing
            float fadeTime = timeForThisDay * 0.3f;
            yield return dateText.DOFade(0.7f, fadeTime).WaitForCompletion();
            dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);
            yield return dateText.DOFade(1f, fadeTime).WaitForCompletion();

            // Wait for the remaining time for this day
            float remainingTime = timeForThisDay - (fadeTime * 2);
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }
        }

        // Final format showing "from -> to" 
        yield return new WaitForSeconds(1f);
        yield return dateText.DOFade(0.5f, 1f).WaitForCompletion();
    }
    VideoPlayer videoPlayer;
    void SetFilmGrain(bool enable)
    {
        videoPlayer = Camera.main.GetComponent<VideoPlayer>();
        videoPlayer.enabled = enable;
    }
    public string GetCurrentEventName()
    {
        return currentDateRecord.events[currentEventIndex].eventName;
    }

    [YarnCommand("EndEvent")]
    public static void EndEvent(bool FreeTimeConsumed = false)
    {
        if (FreeTimeConsumed)
            isEvening = true;
        currentEventIndex++;
        TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
    }
    public void EndDay()
    {
        currentEventIndex = 0;
        isEvening = false;
        GameManager.instance.currentSaveData.currentDate = CalanderSystem.instance.GetNextDate(GameManager.instance.currentSaveData.currentDate);
        SaveSystem.SaveDataToFile();
        YarnDialogSystemSingleTonMaker.instance.dialogueRunner.SaveStateToPersistentStorage("yarnSaveData.json");
        TransitionScreenManager.instance.LoadScene("NewDayScene");
    }

}
