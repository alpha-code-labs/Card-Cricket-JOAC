using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn;
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
        linePresenter = dialogueRunner.GetComponentInChildren<LinePresenter>();
        dialogueRunner.LoadStateFromPersistentStorage("yarnSaveData.json");

        dialogueRunner.onDialogueComplete.AddListener(HandleDialogueComplete);
        dialogueRunner.onDialogueStart.AddListener(HandleDialogueStart);
    }
    public DialogueRunner dialogueRunner;
    LinePresenter linePresenter;
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
    [YarnCommand("SetTypewriterSpeed")]
    public static void SetTypewriterSpeed(int speed)
    {
        instance.linePresenter.typewriterEffectSpeed = speed;
    }
    [YarnCommand("ResetTypewriterSpeed")]
    public static void ResetTypewriterSpeed()
    {
        instance.linePresenter.typewriterEffectSpeed = 60;//Default Speed
    }
    void HandleDialogueComplete()
    {
        UIBlocker.raycastTarget = false;
    }
    void HandleDialogueStart()
    {
        UIBlocker.raycastTarget = true;
    }

}
enum Reward
{
    Courage,
    Foresight,
    Humility,
    Resourcefulness,
}
