using UnityEngine;
using System.Collections.Generic;

public static class CalendarEventLoader
{
    public static string currentYarnNode;
    public static string nextGameplayName;
    
    [SerializeField] private static CalanderRecord calendar; // Add this at top

    public static void LoadEventByName(string eventName, CalanderRecord calendarRef)
    {
        if (calendarRef == null)
        {
            Debug.LogError("CalanderRecord not assigned!");
            return;
        }

        // Find the event
        EventRecord targetEvent = null;
        DateRecord eventDate = null;

        foreach (var dateRecord in calendarRef.dates)
        {
            foreach (var eventRecord in dateRecord.events)
            {
                if (eventRecord.eventName == eventName)
                {
                    targetEvent = eventRecord;
                    eventDate = dateRecord;
                    break;
                }
            }
            if (targetEvent != null) break;
        }

        if (targetEvent == null)
        {
            Debug.LogError($"Event '{eventName}' not found in calendar!");
            return;
        }

        // Store the yarn node name
        currentYarnNode = eventName;
        Debug.Log($"📌 Loading Event: {eventName}");

        // Find next GamePlay event
        bool foundTarget = false;
        foreach (var dateRecord in calendarRef.dates)
        {
            foreach (var eventRecord in dateRecord.events)
            {
                if (foundTarget && (eventRecord.eventType == TypeOfEvent.GamePlay || eventRecord.eventType == TypeOfEvent.QuizGamePlay))
                {
                    nextGameplayName = eventRecord.eventName;
                    Debug.Log($"⚙️ Next Gameplay: {nextGameplayName}");
                    break;
                }

                if (eventRecord.eventName == eventName)
                {
                    foundTarget = true;
                }
            }
            if (nextGameplayName != null) break;
        }

        DialogueScriptCommandHandler.currentNode = currentYarnNode;
       TransitionScreenManager.instance.LoadScene(SceneNames.CutsceneScene);
    }
}