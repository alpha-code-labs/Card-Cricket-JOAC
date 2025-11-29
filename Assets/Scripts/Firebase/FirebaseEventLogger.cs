using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
public class FirebaseEventLogger : MonoBehaviour
{
  
    public static void LogStartButtonClick()
    {
        Debug.Log("Logging Start Button Click Event to Firebase Analytics");
        FirebaseAnalytics.LogEvent("start_button_click");
    }
    public static void LogCutsceneEnd(string currentNode)
{
    string sceneString = $"scene: {currentNode}";
    Debug.Log($"🔍 LogCutsceneEnd called with: {sceneString}");
    FirebaseAnalytics.LogEvent(
        "cutscene_end",
        new Parameter[] {
            new Parameter("scene_name", sceneString),
        }
    );
}

    public static void LogTutorialEnd()
    {
        FirebaseAnalytics.LogEvent("tutorial_end");
    }

    public static void LogSkipButtonOnMap()
    {
        FirebaseAnalytics.LogEvent("skip_button_on_map");
    }

public static void LogGameplayEnd(string gameplayName)
{
    string gameplayString = $"gameplay: {gameplayName}";
    Debug.Log($"🔍 LogGameplayEnd called with: {gameplayString}");
    FirebaseAnalytics.LogEvent(
        "gameplay_end",
        new Parameter[] {
            new Parameter("gameplay_name", gameplayString),
        }
    );
}

   public static void LogSideStoryDayEnd(string dayName)
{
    string dayString = $"side_story: {dayName}";
    Debug.Log($"🔍 LogSideStoryDayEnd called with: {dayString}");
    FirebaseAnalytics.LogEvent(
        "side_story_day_end",
        new Parameter[] {
            new Parameter("day_name", dayString),
        }
    );
}
}