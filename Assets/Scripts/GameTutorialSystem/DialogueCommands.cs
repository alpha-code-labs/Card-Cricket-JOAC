using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering;

public class DialogueCommands : MonoBehaviour
{

    private static GraphicRaycaster persistentDialogueRaycaster;
    private static GameObject persistentDialogueSystem;
    public static LinePresenter linePresenter;

    [YarnCommand("enableAutoAdvance")]
    public static void EnableAutoAdvance()
    {

        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System")
            .ToArray();

        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
            if (dialogueSystem.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                // Disable the entire Dialogue System GameObject
                linePresenter = dialogueSystem.GetComponentInChildren<LinePresenter>();
                Debug.Log("Found LinePresenter in current scene Dialogue System");
            }
        }
        if (linePresenter != null)
        {
            linePresenter.autoAdvance = true;
            Debug.Log("Enabled AutoAdvance on LinePresenter");
        }
        else
        {
            Debug.LogWarning("LinePresenter reference is null. Cannot enable AutoAdvance.");
        }
    }
    [YarnCommand("disableAutoAdvance")]
    public static void DisableAutoAdvance()
    {
        if (linePresenter != null)
        {
            linePresenter.autoAdvance = false;
            Debug.Log("Disabled AutoAdvance on LinePresenter");
        }
        else
        {
            Debug.LogWarning("LinePresenter reference is null. Cannot disable AutoAdvance.");
        }
    }
    public void DisablePersistentDialogueRaycaster()
    {
        // Get all GameObjects named "Dialogue System"
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System")
            .ToArray();

        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                // Disable the entire Dialogue System GameObject
                dialogueSystem.SetActive(false);
                persistentDialogueSystem = dialogueSystem; // Store reference for re-enabling
                Debug.Log($"Disabled DontDestroyOnLoad Dialogue System GameObject from scene: {dialogueSystem.scene.name}");
                break;
            }
        }
    }

    public void EnablePersistentDialogueRaycaster()
    {
        // If we have a stored dialogue system reference, use it (faster)
        if (persistentDialogueSystem != null)
        {
            persistentDialogueSystem.SetActive(true);
            Debug.Log("Re-enabled DontDestroyOnLoad Dialogue System GameObject (from reference)");
            return;
        }

        // Otherwise, find it again (though this is unlikely since inactive objects won't be found by FindObjectsOfType)
        // This fallback might not work if the object is inactive
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
            .Where(go => go.name == "Dialogue System")
            .ToArray();

        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                dialogueSystem.SetActive(true);
                persistentDialogueSystem = dialogueSystem; // Store reference for future use
                Debug.Log($"Re-enabled DontDestroyOnLoad Dialogue System GameObject from scene: {dialogueSystem.scene.name}");
                break;
            }
        }
    }

    [YarnCommand("disableRaycasterOnMainDialogueSystem")]
    public static void disableRaycasterOnMainDialogueSystem()
    {
        GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
           .Where(go => go.name == "Dialogue System")
           .ToArray();

        foreach (GameObject dialogueSystem in dialogueSystems)
        {
            // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
            if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
            {
                // Disable the entire Dialogue System GameObject
                dialogueSystem.SetActive(false);
                persistentDialogueSystem = dialogueSystem; // Store reference for re-enabling
                Debug.Log($"Disabled DontDestroyOnLoad Dialogue System GameObject from scene: {dialogueSystem.scene.name}");
                break;
            }
        }
    }

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

    [YarnCommand("unhideBallerCard")]
    public static void UnhideBallerCard()
    {
        Debug.Log("Unhiding baller card");
        DialoguePositioner.Instance.SetPositionToUnhideCard();
        DialoguePositioner.Instance.ShiftCharacterImageToUnhideCard();
    }

    [YarnCommand("resetToOriginalPresets")]
    public static void ResetToOriginalPresets()
    {
        Debug.Log("Unhiding baller card");
        DialoguePositioner.Instance.ResetToOriginalPresets();
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
    public static void ShowScorePanel()
    {
        ScoreManager_Tutorial.Instance.ShowScorePanel();
    }

    [YarnCommand("showBBallsPanel")]
    public static void ShowBBallsPanel()
    {
        ScoreManager_Tutorial.Instance.ShowBallsPanel();
    }

    [YarnCommand("showWicketsPanel")]
    public static void ShowWicketsPanel()
    {
        ScoreManager_Tutorial.Instance.ShowWicketsPanel();
    }

    [YarnCommand("showShotsPanel")]
    public static void ShowShotsPanel()
    {
        CardsPoolManager_Tutorial.Instance.showShotPanel();
    }

    [YarnCommand("showTimingPanel")]
    public static void ShowTimingPanel()
    {
        Timer_Tutorial.Instance.ShowTimingPanel();
    }

    [YarnCommand("showFlipButton")]
    public static void ShowFlipButton()
    {
        ScoreManager_Tutorial.Instance.ShowFlipButton();
    }

    [YarnCommand("highlightShotPanel")]
    public static void HighlightShotPanel()
    {
        CardsPoolManager_Tutorial.Instance.HighlightShotPanel();
    }


    ///Balls 
    [YarnCommand("ballFirstBall")]
    public static void BallFirstBall()
    {
        CardsPoolManager_Tutorial.Instance.BallFirstBall();
    }

    [YarnCommand("ballSecondBall")]
    public static void BallSecondBall()
    {
        CardsPoolManager_Tutorial.Instance.BallSecondBall();
    }
    [YarnCommand("ballThirdBall")]
    public static void BallThirdBall()
    {
        CardsPoolManager_Tutorial.Instance.BallThirdBall();
    }
    [YarnCommand("ballFourthBall")]
    public static void BallFourthBall()
    {
        CardsPoolManager_Tutorial.Instance.BallFourthBall();
    }
    [YarnCommand("ballFifthBall")]
    public static void BallFifthBall()
    {
        Debug.Log("Fifth ball called from yarn");
        CardsPoolManager_Tutorial.Instance.BallFifthBall();
    }

    [YarnCommand("ballSixthBall")]
    public static void BallSixthBall()
    {
        CardsPoolManager_Tutorial.Instance.BallSixthBall();
    }

    [YarnCommand("ballSeventhBall")]
    public static void BallSeventhBall()
    {
        CardsPoolManager_Tutorial.Instance.BallSeventhBall();
    }
    [YarnCommand("startNextScene")]
    public static void StartNextScene()
    {
        Debug.Log("Tutorial complete starting next scene");
        
        // Enable the entire Dialogue System GameObject
        // if (persistentDialogueSystem != null)
        // {
        //     persistentDialogueSystem.SetActive(true);
        //     Debug.Log("Re-enabled DontDestroyOnLoad Dialogue System GameObject (from reference)");
        // }
        // else
        // {
        //     // Fallback: try to find it again (though this won't work if it's inactive)
        //     // This is kept for safety but likely won't find inactive objects
        //     GameObject[] dialogueSystems = FindObjectsOfType<GameObject>()
        //         .Where(go => go.name == "Dialogue System")
        //         .ToArray();

        //     foreach (GameObject dialogueSystem in dialogueSystems)
        //     {
        //         // Check if it's not in the current scene (meaning it's DontDestroyOnLoad)
        //         if (dialogueSystem.scene != UnityEngine.SceneManagement.SceneManager.GetActiveScene())
        //         {
        //             dialogueSystem.SetActive(true);
        //             persistentDialogueSystem = dialogueSystem; // Store reference for future use
        //             Debug.Log($"Re-enabled DontDestroyOnLoad Dialogue System GameObject from scene: {dialogueSystem.scene.name}");
        //             break;
        //         }
        //     }
        // }
        
        //implement...
        Debug.Log("calling end event");
        NewDayManager.EndEvent();
    }

    [YarnCommand("showBatsman")]
    public static void ShowBatsman()
    {
        ScoreManager_Tutorial.Instance.ShowBatsman();
    }

    [YarnCommand("hideBatsman")]
    public static void HideBatsman()
    {
        ScoreManager_Tutorial.Instance.HideBatsman();
    }

    [YarnCommand("showArrow")]
    public static void ShowArrow(string arrowName)
    {
        TutorialArrowManager.Instance.ShowArrow(arrowName);
    }

    [YarnCommand("hideArrow")]
    public static void HideArrow()
    {
        TutorialArrowManager.Instance.HideAllArrows();
    }

    [YarnCommand("ShowAllArrows")]
    public static void ShowAllArrows()
    {
        TutorialArrowManager.Instance.ShowAllArrows();
    }

    [YarnCommand("changeTimerTiming")]
    public static void ChangeTimerTiming(float newTime)
    {
        Timer_Tutorial.Instance.updateMaxTimeToChooseStrategy(Mathf.CeilToInt(newTime));
    }
}