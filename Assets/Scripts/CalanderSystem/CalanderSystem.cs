using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using UnityEngine;

public class CalanderSystem : MonoBehaviour
{
    string currentDate;
    string currentDateRecord;
    int currentEventIndex;
    [SerializeField] internal CalanderRecord calanderRecord;
    Dictionary<string, DateRecord> dateRecords = new Dictionary<string, DateRecord>();
    public void Start()
    {
        LoadDateRecordsDict();
        currentEventIndex = 0;
        //Set current date from SaveData
        //Set currentDateRecord from current date
        DisplayGrainEffect(currentDate);
        EventRecord currentEvent = dateRecords[currentDate].events[currentEventIndex];
        StartEvent(currentEvent);
    }
    void LoadDateRecordsDict()
    {
        foreach (var date in calanderRecord.dates)
        {
            dateRecords.Add(date.date, date);
        }
    }
    public void DisplayGrainEffect(string currentDate)
    {

    }
    public void StartEvent(EventRecord events)
    {
        switch (events.eventType)
        {
            case TypeOfEvent.ForcedCutscene:
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