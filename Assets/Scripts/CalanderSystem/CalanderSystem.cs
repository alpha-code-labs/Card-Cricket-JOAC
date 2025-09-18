using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalanderSystem : MonoBehaviour
{
}
// Allows creation from Unity context menu
[CreateAssetMenu(fileName = "CalanderRecord", menuName = "Calendar/CalanderRecord")]
public class CalanderRecord : ScriptableObject
{
    [SerializeField] List<DateRecord> dates;//Only 30
}
[Serializable]
public class DateRecord
{
    public string date;
    public List<EventRecord> events;//Only 2
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