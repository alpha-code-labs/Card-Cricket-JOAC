using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewDayManager : MonoBehaviour
{
    TextMeshProUGUI dateText;
    public static DateRecord currentDateRecord;
    public static int currentEventIndex = 0;//Dont Modify This Directly you are probably making a mistake if you want to
    public static bool isEvening = false;
    void Start()
    {
        dateText = GetComponentInChildren<TextMeshProUGUI>();
        string prettyDate = GetPrettyDateString(GameManager.instance.currentSaveData.currentDate);
        dateText.text = prettyDate;
        currentDateRecord = CalanderSystem.instance.GetDateRecordFromDate(GameManager.instance.currentSaveData.currentDate);
        if (currentEventIndex >= currentDateRecord.events.Count)
        {
            EndDay();
        }
        StartCoroutine(StartEventAfterDelay(currentDateRecord.events[currentEventIndex]));
    }
    string GetPrettyDateString(string date)
    {
        if (DateTime.TryParse(date, out DateTime dt))
        {
            int day = dt.Day;
            string suffix = "th";
            if (day % 10 == 1 && day != 11) suffix = "st";
            else if (day % 10 == 2 && day != 12) suffix = "nd";
            else if (day % 10 == 3 && day != 13) suffix = "rd";
            return $"{day}{suffix} {dt:MMMM yyyy}";
        }
        return date;
    }
    IEnumerator StartEventAfterDelay(EventRecord events)
    {
        Debug.Log($"Starting Event {currentDateRecord.date} {events.eventName} after delay");
        yield return new WaitForSeconds(3f);
        StartEvent(events);
    }
    public string GetCurrentEventName()
    {
        return currentDateRecord.events[currentEventIndex].eventName;
    }
    public void StartEvent(EventRecord events)
    {
        Debug.Log($"Starting Event: {events.eventName} of type {events.eventType}");
        switch (events.eventType)
        {
            case TypeOfEvent.ForcedCutscene:
                // LoadDialoageSystem(events.eventName);  // dialogueRunner.StartDialogue("FirstTimeIntro");
                DialogueScriptCommandHandler.currentNode = events.eventName;
                TransitionScreenManager.instance.LoadScene(SceneNames.CutsceneScene);
                // TransitionScreenManager.instance.LoadScene("yarn-test");
                break;
            case TypeOfEvent.FreeTime:
                TransitionScreenManager.instance.LoadScene(SceneNames.WorldNav);
                break;
            case TypeOfEvent.Speical:
                //Load Special Event
                break;
            case TypeOfEvent.SkipDayOrEvening:
                if (isEvening)
                    EndDay();
                isEvening = true;
                currentEventIndex++;
                TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
                //Skip to next day or evening
                break;
            case TypeOfEvent.GamePlay:
                ScoreManager.Instance.SetTargetFromEventName(events.eventName);
                TransitionScreenManager.instance.LoadScene(SceneNames.CardGameScene);
                //Load GamePlay                
                break;
            default:
                Debug.LogError("No event type found");
                break;
        }
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
