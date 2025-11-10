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
    DateShuffleEffectManager dateShuffelEffectManager;
    void Awake()
    {
        instance = this;
    }
    public static DateRecord currentDateRecord;
    public static int currentEventIndex = 0;//Dont Modify This Directly you are probably making a mistake if you want to
    public static bool isEvening = false;
    void Start()
    {
        dateShuffelEffectManager = GetComponentInChildren<DateShuffleEffectManager>();
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
            yield return dateShuffelEffectManager.DisplayTextThenFade(prettyDate + "\n Day End");
            EndDay();
            yield break;
        }
        EventRecord events = currentDateRecord.events[currentEventIndex];
        if (currentEventIndex == 0)
        {
            DateTime previousDate = CalanderSystem.instance.GetPreviousDateTime(GameManager.instance.currentSaveData.currentDate);
            DateTime currentDate = DateTime.Parse(GameManager.instance.currentSaveData.currentDate);
            if (currentDate == previousDate && currentDate == DateTime.Parse("1988/07/18"))
            {
                //Special Case for first day of game no animation
                yield return dateShuffelEffectManager.DisplayTextThenFade(PrettyStrings.GetPrettyDateString(currentDate.ToString()) + "\nSomewhere in Dharavi, Mumbai");
            }
            else
                yield return dateShuffelEffectManager.AnimateDateProgression(previousDate, currentDate);
        }

        Debug.Log($"Starting Event: {events.eventName} of type {events.eventType}");

        switch (events.eventType)
        {
            case TypeOfEvent.ForcedCutscene:
                if (currentEventIndex != 0)
                    yield return dateShuffelEffectManager.DisplayTextThenFade("");//remove this if you dont want to proprly wait and want transistions to be fast
                DialogueScriptCommandHandler.currentNode = events.eventName;
                TransitionScreenManager.instance.LoadScene(SceneNames.CutsceneScene);
                // TransitionScreenManager.instance.LoadScene("yarn-test");
                break;
            case TypeOfEvent.FreeTime:
                string timeOfDay = isEvening ? "Evening" : "Morning";
                yield return dateShuffelEffectManager.DisplayTextThenFade($"Free Time\n{timeOfDay}");
                TransitionScreenManager.instance.LoadScene(SceneNames.WorldNav);
                break;
            case TypeOfEvent.Speical:
                yield return dateShuffelEffectManager.DisplayTextThenFade("");
                //Load Special Event
                break;
            case TypeOfEvent.CardGamePlayTutorial:
                yield return dateShuffelEffectManager.DisplayTextThenFade("");
                TransitionScreenManager.instance.LoadScene(SceneNames.CardGameTutorialScene);
                break;
            case TypeOfEvent.QuizGamePlay:
                yield return dateShuffelEffectManager.DisplayTextThenFade("");
                TransitionScreenManager.instance.LoadScene(SceneNames.QuizGamePlay);
                break;
            case TypeOfEvent.SkipDayOrEvening:
                isEvening = true;
                currentEventIndex++;
                BeginNewDaySequence();
                //Skip to next day or evening
                break;
            case TypeOfEvent.GamePlay:
                yield return dateShuffelEffectManager.DisplayTextThenFade("");
                //ScoreManager.Instance.SetTargetFromEventName(events.eventName);
                TransitionScreenManager.instance.LoadScene(SceneNames.CardGameScene);
                //Load GamePlay                
                break;
            default:
                Debug.LogError("No event type found");
                break;
        }
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
    [YarnCommand("RetryCurrentEvent")]//special command to retry the current event
    public static void RetryCurrentEvent()
    {
        TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
    }
    [YarnCommand("RetryEvent")]//special command to retry a specific event by name
    public static void RetryEvent(string eventName)//only works for current day
    {
        int resetToIndex = currentDateRecord.events.FindIndex(e => e.eventName == eventName);
        currentEventIndex = resetToIndex;
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
    [YarnFunction("GetCurrentEventName")]
    public static string GetCurrentEventNameStatic(int i)
    {
        if (i < 0 || i >= currentDateRecord.events.Count)
            return "End Day (Index Out of Range)";
        return $"{i}: {currentDateRecord.events[i].eventName} of type {currentDateRecord.events[i].eventType}";
    }
    [YarnFunction("GetCurrentEventIndex")]
    public static int GetCurrentEventIndex()
    {
        return currentEventIndex;
    }
    [YarnCommand("SetEventIndex")]
    public static void SetEventIndex(int index)
    {
        currentEventIndex = index;
    }
    [YarnCommand("SetNextDateWithForcedCutscene")]
    public static void SetNextDateWith()//Sets the next date that has a forced cutscene skips everything else
    {
        TypeOfEvent eventType = TypeOfEvent.ForcedCutscene;
        while (currentDateRecord.events[currentEventIndex].eventType != eventType)
        {
            currentEventIndex++;
            if (currentEventIndex >= currentDateRecord.events.Count)
            {
                GameManager.instance.currentSaveData.currentDate = CalanderSystem.instance.GetNextDate(GameManager.instance.currentSaveData.currentDate);
                currentDateRecord = CalanderSystem.instance.GetDateRecordFromDate(GameManager.instance.currentSaveData.currentDate);
                if (currentDateRecord == null)
                {
                    Debug.LogError("No next date record found, cannot set next date with forced cutscene");
                    return;
                }
                currentEventIndex = 0;
            }
        }
    }
}
