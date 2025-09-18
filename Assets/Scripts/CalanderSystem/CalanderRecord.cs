
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using UnityEngine;
// Allows creation from Unity context menu
[CreateAssetMenu(fileName = "CalanderRecord", menuName = "Calendar/CalanderRecord")]
public class CalanderRecord : ScriptableObject
{
    [SerializeField] public List<DateRecord> dates;//Only 30
}
[Serializable]
public class DateRecord
{
    public string date;//used for Dictionary key and Reffe
    public List<EventRecord> events;
}
[Serializable]
public class EventRecord
{
    public string eventName;
    public string eventDescription;
    public TypeOfEvent eventType;
}
[Serializable]
public enum TypeOfEvent
{
    ForcedCutscene,
    FreeTime,
    Speical,
    SkipDayOrEvening,
    GamePlay
}