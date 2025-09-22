using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn;
using Yarn.Unity;

public class WorldIntractionDialougeManager : MonoBehaviour
{
    public static WorldIntractionDialougeManager instance;
    [SerializeField] internal DialogueRunner dialogueRunner;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // dialogueRunner.AddFunction<string>("get_next_location", GetNextLocation);
    }
    public bool IsDialogueCurrentlyRunning()
    {
        return dialogueRunner.IsDialogueRunning;
    }
    internal Locations location;
    public void StartGoToDialogue(Locations locations)
    {
        location = locations;
        dialogueRunner.StartDialogue("ConfirmLocation");
    }
    [YarnCommand("confirm_location")]
    public static void ConfirmLocation(bool userConfirmed)
    {
        if (userConfirmed)
        {
            LocationSwitcher.instance.SwitchLocation(instance.location);
        }
        else
        {
            Debug.Log("User cancelled location switch.");
        }
    }
    [YarnFunction("get_next_location")]
    public static string GetNextLocation()
    {
        return instance.location.ToString();
    }

}
