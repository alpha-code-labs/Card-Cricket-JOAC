using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewDayManager : MonoBehaviour
{
    public static NewDayManager instance;
    TextMeshProUGUI dateText;
    public static int currentEventIndex = 0;//Dont Modify This Directly you are probably making a mistake if you want to
    DateRecord currentDateRecord;
    void Start()
    {
        dateText = GetComponentInChildren<TextMeshProUGUI>();
        dateText.text = GameManager.instance.currentSaveData.currentDate;
        currentDateRecord = CalanderSystem.instance.GetDateRecordFromDate(GameManager.instance.currentSaveData.currentDate);
        StartEvent(currentDateRecord.events[currentEventIndex]);
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

                //Load cutscene
                break;
            case TypeOfEvent.FreeTime:
                //Load FreeTime
                break;
            case TypeOfEvent.Speical:
                //Load Special Event
                break;
            case TypeOfEvent.SkipDayOrEvening:
                //Skip to next day or evening
                break;
            case TypeOfEvent.GamePlay:
                ScoreManager.Instance.SetTargetFromEventName(events.eventName);
                //Load GamePlay                
                break;
            default:
                UnityEngine.Debug.LogError("No event type found");
                break;
        }
    }

}
