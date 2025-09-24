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
            yield return DisplayTextThenFade(prettyDate);
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
