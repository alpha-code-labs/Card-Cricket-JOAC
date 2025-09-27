using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalanderSystem : MonoBehaviour
{
    public static CalanderSystem instance;
    void Awake()
    {
        instance = this;
    }
    [SerializeField] internal CalanderRecord calanderRecord;
    Dictionary<string, DateRecord> dateRecords = new Dictionary<string, DateRecord>();
    public void Start()
    {
        LoadDateRecordsDict();
    }
    void LoadDateRecordsDict()
    {
        foreach (var date in calanderRecord.dates)
        {
            dateRecords.Add(date.date, date);
        }
    }
    public DateRecord GetDateRecordFromDate(string date)
    {
        if (dateRecords.TryGetValue(date, out DateRecord record))
        {
            return record;
        }
        else
        {
            UnityEngine.Debug.LogError($"No date record found for date: {date}");
            return null;
        }
    }
    public string GetNextDate(string currentDate)
    {
        int indexOfCurrentDate = calanderRecord.dates.FindIndex(d => d.date == currentDate);
        indexOfCurrentDate++;
        DateRecord nextDateRecord = calanderRecord.dates[indexOfCurrentDate];
        string nextDateString = nextDateRecord.date;
        Debug.Log($"Next date is {nextDateString}");
        return nextDateString;
    }
    public DateTime GetPreviousDateTime(string currentDate)
    {
        int indexOfCurrentDate = calanderRecord.dates.FindIndex(d => d.date == currentDate);
        if (indexOfCurrentDate > 0)
        {
            indexOfCurrentDate--;
            DateRecord previousDateRecord = calanderRecord.dates[indexOfCurrentDate];
            DateTime previousDateTime = DateTime.Parse(previousDateRecord.date);
            Debug.Log($"Previous date is {previousDateTime}");
            return previousDateTime;
        }
        else
        {
            Debug.LogError("No previous date available for: " + currentDate);
            return DateTime.Parse(currentDate);
        }
    }
}