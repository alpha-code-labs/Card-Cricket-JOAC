using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class WorldIntractionDialougeManager : MonoBehaviour
{
    public static WorldIntractionDialougeManager instance;
    void Awake()
    {
        instance = this;
    }
    public bool IsDialogueCurrentlyRunning()
    {
        return YarnDialogSystemSingleTonMaker.instance.dialogueRunner.IsDialogueRunning;
    }
    string YesChoice;
    string NoChoice;
    Action OnDialogueConfirmed;
    public void StartConfirmationDialogue(string YesChoice, string NoChoice, Action onDialogueConfirmed = null)
    {
        OnDialogueConfirmed = onDialogueConfirmed;
        this.YesChoice = YesChoice;
        this.NoChoice = NoChoice;

        YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue("ConfirmationDialogue");
    }
    [YarnCommand("confirm_choice")]
    public static void ConfirmChoice(bool userConfirmed)
    {
        if (userConfirmed)
        {
            instance.OnDialogueConfirmed?.Invoke();
        }
    }
    [YarnFunction("get_yes_choice")]
    public static string GetYesChoice()
    {
        return instance.YesChoice;
    }
    [YarnFunction("get_no_choice")]
    public static string GetNoChoice()
    {
        return instance.NoChoice;
    }

}
