using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkToCharacterIntractionHandler : ClickAbleObjectHandler
{
    public Characters character;
    public override void OnClick()
    {
        WorldIntractionDialougeManager.instance.StartConfirmationDialogue("Yes, " + GetIntractionMessage(), "No, don't talk", OnConfirmed);
    }
    void OnConfirmed()
    {
        NewDayManager.isEvening = true;
        NewDayManager.currentEventIndex++;
        TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);
    }
    string GetIntractionMessage()
    {
        return "Talk to " + character.ToString();
    }
}
public enum Characters
{
    Ramu,
    Kamla,
    ShivPrasad,
    Bed,
    Amit,
    Sumit,
    Pinky,
    RamCharan,
    Priya,
    Naresh,
    CoachSharma,
    SunitaMam,
    Aryan,
    CricketDada,
    Amarjeet,
    Suresh,
    AgarwalUncle,
    Fatima,
    MochiUncle,
    MunnaBhai,
    Vikram
}