using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class YarnDialogSystemSingleTonMaker : MonoBehaviour
{
    public static YarnDialogSystemSingleTonMaker instance;
    [SerializeField] Image UIBlocker;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
        dialogueRunner.LoadStateFromPersistentStorage("yarnSaveData.json");

        dialogueRunner.onDialogueComplete.AddListener(HandleDialogueComplete);
        dialogueRunner.onDialogueStart.AddListener(HandleDialogueStart);
    }
    public DialogueRunner dialogueRunner;
    [YarnCommand("AutoAdvance")]
    public static void AutoAdvance(bool isAuto)
    {
        instance.dialogueRunner.GetComponentInChildren<LinePresenter>().autoAdvance = isAuto;
    }
    [YarnCommand("GoToMainMenu")]
    public static void GoToMainMenu()
    {
        TransitionScreenManager.instance.LoadScene(SceneNames.MainMenu);
    }
    void HandleDialogueComplete()
    {
        UIBlocker.raycastTarget = false;
    }
    void HandleDialogueStart()
    {
        UIBlocker.raycastTarget = true;
    }
    [YarnFunction("GetAdvanceButtonTip")]
    public static string GetAdvanceButtonTip()
    {
        if (Application.isEditor)
        {
            return "Press Space to advance (Editor)";
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.LinuxPlayer)
        {
            return "Press Space to advance";
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return "Touch here to continue";
        }
        else
        {
            return "Press here to advance";
        }
    }

}
enum Reward
{
    Courage,
    Foresight,
    Humility,
    Resourcefulness,
}
