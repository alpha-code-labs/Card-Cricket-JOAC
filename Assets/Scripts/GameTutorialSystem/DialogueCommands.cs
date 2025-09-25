using UnityEngine;
using Yarn.Unity;

public class DialogueCommands : MonoBehaviour
{

    [YarnCommand("moveDialogue")]
    public static void MoveDialogue(string anchor)
    {
        // e.g. <<moveDialogue top>> in Yarn
        Vector2 pos = anchor switch
        {
            "top" => new Vector2(0f, 453f),
            "bottom" => new Vector2(200f, 80f),
            _ => new Vector2(0, 0)
        };
        DialoguePositioner.Instance.SetPosition(pos);
    }

    [YarnCommand("showCharacterImage")]
    public static void ShowCharacterImage(string relativePos)
    {
        //relative pos can be right and top
        Vector2 pos = relativePos switch
        {
            "top" => new Vector2(649f, -308f),
            "center" => new Vector2(674f, 69f),
            _ => new Vector2(649f, -308f)
        };

        float width, height;
        switch (relativePos)
        {
            case "top": { width = 220f; height = 304f; break; }
            default: { width = 220f; height = 304f; break; }
        }

        DialoguePositioner.Instance.ShowCharacterImage(pos, width, height);
    }

    [YarnCommand("hideCharacterImage")]
    public static void HideCharacterImage()
    {
        DialoguePositioner.Instance.HideCharacterImage();
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

    [YarnCommand("showTimingPanel")]
    public static void showTimingPanel()
    {
        Timer_Tutorial.Instance.ShowTimingPanel();
    }

    [YarnCommand("showFlipButton")]
    public static void showFlipButton()
    {
        ScoreManager_Tutorial.Instance.ShowFlipButton();
    }

    [YarnCommand("highlightShotPanel")]
    public static void highlightShotPanel()
    {
        CardsPoolManager_Tutorial.Instance.HighlightShotPanel();
    }


    ///Balls 
    [YarnCommand("ballFirstBall")]
    public static void ballFirstBall()
    {
        CardsPoolManager_Tutorial.Instance.BallFirstBall();
    }
}
