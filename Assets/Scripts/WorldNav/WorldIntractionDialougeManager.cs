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
    internal Locations location;
    public bool IsDialogueCurrentlyRunning()
    {
        return dialogueRunner.IsDialogueRunning;
    }
    [YarnFunction("get_next_location")]
    public static string GetNextLocation()
    {
        return instance.location.ToString();
    }
}
