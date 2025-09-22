using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
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
}