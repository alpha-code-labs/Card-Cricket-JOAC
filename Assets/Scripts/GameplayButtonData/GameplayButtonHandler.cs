using UnityEngine;
using UnityEngine.UI;

public class GameplayButtonHandler : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private string eventName = "scene_6";
    [SerializeField] private CalanderRecord calendarRecord;

    void Start()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

void OnButtonClicked()
{
    if (string.IsNullOrEmpty(eventName))
    {
        Debug.LogError("Event name not set!");
        return;
    }

    if (calendarRecord == null)
    {
        Debug.LogError("❌ CalendarRecord NOT assigned!");
        return;
    }

    Debug.Log($"✅ CalendarRecord loaded with {calendarRecord.dates.Count} dates");
    Debug.Log($"🔘 Button clicked with eventName: '{eventName}'");

    // ✅ FIND THE DATE FOR THIS EVENT
    string eventDate = FindDateForEvent(eventName);
    
    if (string.IsNullOrEmpty(eventDate))
    {
        Debug.LogError($"❌ Could not find date for event: {eventName}");
        return;
    }

    // ✅ FIND NEXT GAMEPLAY EVENT
    string nextGameplay = FindNextGameplayEvent(eventName);

    // ✅ SET BUTTON MODE FLAGS
    GameFlowManager.isButtonMode = true;
    GameFlowManager.buttonYarnNode = eventName;
    GameFlowManager.buttonSceneToLoad = "CutsceneScene";
    GameFlowManager.nextGameplayName = nextGameplay;
    GameFlowManager.nextGameplayDate = eventDate; // ✅ USE DYNAMIC DATE
    // GameFlowManager.buttonModeDate = eventDate; 
    
    Debug.Log($"📌 GameFlowManager set:");
    Debug.Log($"   isButtonMode: {GameFlowManager.isButtonMode}");
    Debug.Log($"   buttonYarnNode: {GameFlowManager.buttonYarnNode}");
    Debug.Log($"   nextGameplayName: {GameFlowManager.nextGameplayName}");
    Debug.Log($"   nextGameplayDate: {GameFlowManager.nextGameplayDate}");
    
    // ✅ SET THE DATE
    // GameManager.instance.currentSaveData.currentDate = eventDate;
    GameFlowManager.savedCampaignDate = GameManager.instance.currentSaveData.currentDate;
GameManager.instance.currentSaveData.currentDate = eventDate;
    Debug.Log($"📅 Set current date to: {eventDate}");
    
    Debug.Log($"🔄 About to load scene: NewDayScene");
    
    // ✅ LOAD NewDayScene
    TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
}

// ✅ ADD THIS METHOD - Find the date for an event
string FindDateForEvent(string eventNameToFind)
{
    foreach (var dateRecord in calendarRecord.dates)
    {
        foreach (var eventRecord in dateRecord.events)
        {
            if (eventRecord.eventName == eventNameToFind)
            {
                Debug.Log($"✅ Found date for '{eventNameToFind}': {dateRecord.date}");
                return dateRecord.date;
            }
        }
    }
    
    Debug.LogError($"❌ Event '{eventNameToFind}' not found in calendar!");
    return "";
}

string FindNextGameplayEvent(string eventName)
{
    bool foundTarget = false;
    foreach (var dateRecord in calendarRecord.dates)
    {
        foreach (var eventRecord in dateRecord.events)
        {
            if (foundTarget && (eventRecord.eventType == TypeOfEvent.GamePlay || eventRecord.eventType == TypeOfEvent.QuizGamePlay))
            {
                Debug.Log($"⚙️ Next Gameplay: {eventRecord.eventName}");
                return eventRecord.eventName;
            }

            if (eventRecord.eventName == eventName)
            {
                foundTarget = true;
            }
        }
    }
    
    return "";
}
}