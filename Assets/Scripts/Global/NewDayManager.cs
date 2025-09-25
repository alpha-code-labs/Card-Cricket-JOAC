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
            yield return StartCoroutine(AnimateDateProgression(previousDate, currentDate));
        }
        else
        {
            SetFilmGrain(false);
            dateText.text = "";
        }

        // Debug.Log($"Starting Event: {events.eventName} of type {events.eventType}");

        switch (events.eventType)
        {
            case TypeOfEvent.ForcedCutscene:
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
                yield return DisplayTextThenFade("");
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
        float animationDuration = 2f; // Total duration for the animation
        float timePerDay = animationDuration / Math.Max(totalDays, 1);
        
        // Start with the previous date
        DateTime currentAnimatedDate = startDate;
        string currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");
        dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);
        
        // Make sure the text is visible
        if (dateText.alpha != 1f)
        {
            yield return dateText.DOFade(1f, 0.3f).WaitForCompletion();
        }
        
        // Wait a moment to show the starting date
        yield return new WaitForSeconds(0.5f);
        
        // Animate through each day
        for (int i = 0; i < totalDays; i++)
        {
            currentAnimatedDate = startDate.AddDays(i + 1);
            currentDateString = currentAnimatedDate.ToString("yyyy/MM/dd");
            
            // Create a smooth transition effect
            yield return dateText.DOFade(0.7f, timePerDay * 0.3f).WaitForCompletion();
            dateText.text = PrettyStrings.GetPrettyDateString(currentDateString);
            yield return dateText.DOFade(1f, timePerDay * 0.3f).WaitForCompletion();
            
            // Wait for the remaining time for this day
            if (timePerDay > 0.6f)
            {
                yield return new WaitForSeconds(timePerDay - 0.6f);
            }
        }
        
        // Final format showing "from -> to" 
        yield return new WaitForSeconds(0.5f);
        yield return dateText.DOFade(0.5f, 0.3f).WaitForCompletion();
        string startDateString = startDate.ToString("yyyy/MM/dd");
        string endDateString = endDate.ToString("yyyy/MM/dd");
        dateText.text = PrettyStrings.GetPrettyDateString(startDateString) + "\n to \n" + PrettyStrings.GetPrettyDateString(endDateString);
        yield return dateText.DOFade(1f, 0.5f).WaitForCompletion();
        
        // Hold the final result for a moment
        yield return new WaitForSeconds(1f);
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
        TransitionScreenManager.instance.LoadScene("NewDayScene");
    }

}
