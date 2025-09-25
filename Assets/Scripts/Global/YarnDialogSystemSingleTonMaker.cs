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
    }
    public DialogueRunner dialogueRunner;
    [YarnCommand("AutoAdvance")]
    public static void AutoAdvance(bool isAuto)
    {
        instance.dialogueRunner.GetComponentInChildren<LinePresenter>().autoAdvance = isAuto;
    }   

}
enum Reward
{
    Courage,
    Foresight,
    Humility,
    Resourcefulness,
}
