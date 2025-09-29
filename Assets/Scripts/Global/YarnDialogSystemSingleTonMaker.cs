using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class YarnDialogSystemSingleTonMaker : MonoBehaviour
{
    public static YarnDialogSystemSingleTonMaker instance;
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

}
enum Reward
{
    Courage,
    Foresight,
    Humility,
    Resourcefulness,
}
