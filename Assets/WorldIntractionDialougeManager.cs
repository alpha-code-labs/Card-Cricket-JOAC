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
    internal Locations location;
    public bool IsDialogueCurrentlyRunning()
    {
        return dialogueRunner.IsDialogueRunning;
    }
}
