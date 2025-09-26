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
    public override void CheckAvaliability()
    {
        bool isCompleted = false;
        YarnDialogSystemSingleTonMaker.instance.dialogueRunner.VariableStorage.TryGetValue("$" + character.ToString() + "_Complete", out isCompleted);
        Debug.Log(character.ToString() + " is completed: " + isCompleted);
        SetAvaliabilityIndicator(!isCompleted);
    }
    void OnConfirmed()
    {
        DialogueScriptCommandHandler.currentNode = character.ToString();
        TransitionScreenManager.instance.LoadScene(SceneNames.CutsceneScene);
    }
    string GetIntractionMessage()
    {
        return "Talk to " + PrettyStrings.GetPrettyEnumString(character.ToString());
    }
}
public enum Characters
{
    Ramu = 0,
    Kamla = 1,
    ShivPrasad = 2,
    Bed = 3,
    Amit = 4,
    Sumit = 5,
    Pinky = 6,
    RamCharan = 7,
    Priya = 8,
    Naresh = 9,
    CoachSharma = 10,
    SunitaMam = 11,
    Aryan = 12,
    CricketDada = 13,
    Amarjeet = 14,
    Suresh = 15,
    AgarwalUncle = 16,
    Fatima = 17,
    MochiUncle = 18,
    MunnaBhai = 19,
    Vikram = 20,
    Raju = 21
}