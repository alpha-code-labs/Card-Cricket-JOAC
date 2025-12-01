using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using UnityEngine.SceneManagement;

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

        UIBlocker.raycastTarget = false;
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
        // When we load MainMenu, we want to run OnMainMenuLoaded
        // SceneManager.sceneLoaded += OnMainMenuLoaded;

        // If you are using an enum/string helper, you can use that.
        // To avoid confusion, I'm using the literal scene name "MainMenu".
        //TransitionScreenManager.instance.LoadScene("MainMenu");

         ResetGameProgressOnChapterEnd();

        TransitionScreenManager.instance.LoadScene(SceneNames.MainMenu);
    }


private static void ResetGameProgressOnChapterEnd()
{
    // Keep these values
    string savedUsername = GameManager.instance.currentSaveData.userName;
    bool savedHasCompletedChapter1 = GameManager.instance.currentSaveData.hasCompletedChapter1;
    float savedStrikeRate = GameManager.instance.currentSaveData.strikeRate;
    float savedBattingAverage = GameManager.instance.currentSaveData.battingAverage;
    
    // Reset to defaults
    GameManager.instance.currentSaveData = new SaveData();
    
    // Restore kept values
    GameManager.instance.currentSaveData.userName = savedUsername;
    GameManager.instance.currentSaveData.hasCompletedChapter1 = savedHasCompletedChapter1;
    GameManager.instance.currentSaveData.strikeRate = savedStrikeRate;
    GameManager.instance.currentSaveData.battingAverage = savedBattingAverage;
    
    // Save to file
    SaveSystem.SaveDataToFile();
    
    // Delete yarnSaveData.json
    DeleteYarnSaveData();
    
    // Reset runtime variables
    NewDayManager.currentEventIndex = 0;
    NewDayManager.isEvening = false;
    
    Debug.Log("✅ Game progress reset for new playthrough!");
}

// ✅ ADD THIS METHOD
private static void DeleteYarnSaveData()
{
    string yarnSavePath = System.IO.Path.Combine(Application.persistentDataPath, "yarnSaveData.json");
    
    if (System.IO.File.Exists(yarnSavePath))
    {
        System.IO.File.Delete(yarnSavePath);
        Debug.Log("✅ yarnSaveData.json deleted!");
    }
}

    // private static void OnMainMenuLoaded(Scene scene, LoadSceneMode mode)
    // {
    //     // Make sure this only runs for the MainMenu scene
    //     if (scene.name == "MainMenu")
    //     {
    //         // Unsubscribe so it doesn't run again and again
    //         SceneManager.sceneLoaded -= OnMainMenuLoaded;

    //         // Find MainMenuManager in the loaded scene
    //         MainMenuManager mainMenuManager = FindObjectOfType<MainMenuManager>();
    //         if (mainMenuManager != null)
    //         {
    //             Debug.Log("MainMenu loaded → enabling Continue/PlayMatches button");
    //             mainMenuManager.EnableContinueButton();
    //         }
    //         else
    //         {
    //             Debug.LogError("MainMenuManager not found in MainMenu scene!");
    //         }
    //     }
    // }
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
