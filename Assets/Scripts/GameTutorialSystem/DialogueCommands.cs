using UnityEngine;
using Yarn.Unity;

public class DialogueCommands : MonoBehaviour
{
    public DialoguePositioner positioner;

    [YarnCommand("moveDialogue")]
    public void MoveDialogue(string anchor)
    {
        // e.g. <<moveDialogue top>> in Yarn
        Vector2 pos = anchor switch
        {
            "top" => new Vector2(200f, 580f),
            "bottom" => new Vector2(200f,80f),
            "" => new Vector2(200f, 200f),
        };
        positioner.MoveToScreenAnchor(pos);
    }

    [YarnCommand("showBallingCard")]
    public static void ShowBallingCard()
    {
        CardsPoolManager_Tutorial.Instance.showFirstBallingCard();
    }

    [YarnCommand("clearHighlights")]
    public static void ClearAllHighlights()
    {
        UIHighlightManager.Instance.ClearHighlight();
    }

    [YarnCommand("showScorePanel")]
    public static void showScorePanel()
    {
        ScoreManager_Tutorial.Instance.ShowScorePanel();
    }

    [YarnCommand("showBBallsPanel")]
    public static void showBBallsPanel()
    {
        ScoreManager_Tutorial.Instance.ShowBallsPanel();
    }

    [YarnCommand("showWicketsPanel")]
    public static void showWicketsPanel()
    {
        ScoreManager_Tutorial.Instance.ShowWicketsPanel();
    }

    [YarnCommand("showShotsPanel")]
    public static void showShotsPanel()
    {
        CardsPoolManager_Tutorial.Instance.showShotPanel();
    }

}
