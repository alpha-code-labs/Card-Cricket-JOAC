using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// public class TransitionScreenManager : MonoBehaviour
// {
//     public static TransitionScreenManager instance;
//     [SerializeField] BasicBlackFadeTransition basicBlackFadeTransition;

//     void Awake()
//     {
//         instance = this;
//     }
//     public static void DevLoadScene(SceneNames SceneName)
//     {
//         // AndroidHelper.ShowToast("Loading Dev Scene");
//         SceneManager.LoadScene(SceneName.ToString());
//     }
//     public void LoadScene(string SceneName)
//     {
//         LoadScene((SceneNames)Enum.Parse(typeof(SceneNames), SceneName));
//     }
//     public Action LoadingFinsihed;
//     public void LoadScene(SceneNames SceneName)
//     {
//         Debug.Log($"Loading Scene: {SceneName}");
//         // FirebaseManager.OnSceneTransistion(SceneName);
//         Action SceneTransitionFinishedAction = () => { SceneTransitionFinished(SceneName); };//This What Needs to Happen After Transition is Finished
//         Action LoadSceneAction = () => { StartCoroutine(LoadSceneCoroutine(SceneName.ToString())); };//This is What Needs to Happen After Transition has Started or During Loading

//         // GetComponent<MochiBounceSceneTransition>().AnimateLoadingScreen(LoadSceneAction);//This is the Intro Animation
//         basicBlackFadeTransition.gameObject.SetActive(true);

//         basicBlackFadeTransition.SetupTransition(LoadSceneAction, SceneTransitionFinishedAction);

//         IEnumerator LoadSceneCoroutine(string sceneName)
//         {
//             AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
//             asyncLoad.allowSceneActivation = false;

//             // Optional: Show loading screen or progress bar here
//             while (!asyncLoad.isDone)
//             {
//                 // Optionally check progress: asyncLoad.progress (0.0 to 0.9)
//                 if (asyncLoad.progress >= 0.9f)
//                 {
//                     // Scene is ready, now activate
//                     asyncLoad.allowSceneActivation = true;
//                 }
//                 yield return null;
//             }
//             yield return WaitTipReading();
//             LoadingFinsihed?.Invoke();
//         }
//     }

//     IEnumerator WaitTipReading()
//     {
//         float timer = 0f;
//         float waitTime = 5f;//this is max time to wait
//         waitTime = 0;//Setting this to 0 for now as we dont have tips yet
//         // bool touched = false;

//         // Wait for 3 seconds or until touch/mouse down
//         while (timer < waitTime && !Input.GetMouseButtonDown(0) && Input.touchCount == 0)
//         {
//             timer += Time.unscaledDeltaTime;
//             yield return null;
//         }

//         // If interrupted by touch/mouse, wait for release
//         if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
//         {
//             // Wait until all touches/mouse are released
//             while (Input.GetMouseButton(0) || Input.touchCount > 0)
//             {
//                 yield return null;
//             }
//         }
//     }
//     void SceneTransitionFinished(SceneNames processThisSceneTransitionFinish)
//     {
//         Time.timeScale = 1;
//         switch (processThisSceneTransitionFinish)
//         {
//             case SceneNames.NewDayScene:
//                 NewDayManager.instance.BeginNewDaySequence();
//                 break;
//         }
//     }
// }
public enum SceneNames
{
    CardGameScene,
    CardGameTutorialScene,
    NewDayScene,
    WorldNav,
    CutsceneScene,
    QuizGamePlay,
    MainMenu
}
public enum TransitionTypes
{
    None,
    FadeBlack,
    FilmGrainWithDate,
    DayEvening,
    OutBounce//Mochi the Blob Style
}




public class TransitionScreenManager : MonoBehaviour
{
    public Action LoadingFinsihed;
    public static TransitionScreenManager instance;
    // ❌ REMOVE: [SerializeField] BasicBlackFadeTransition basicBlackFadeTransition;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(SceneNames SceneName)
    {
        Debug.Log($"🔄 Loading Scene: {SceneName}");
        
        // ✅ USE INSTANCE INSTEAD OF SERIALIZED FIELD
        if (BasicBlackFadeTransition.instance == null)
        {
            Debug.LogError("❌ BasicBlackFadeTransition.instance is null!");
            SceneManager.LoadScene(SceneName.ToString());
            return;
        }
        
        Action SceneTransitionFinishedAction = () => { SceneTransitionFinished(SceneName); };
        Action LoadSceneAction = () => { StartCoroutine(LoadSceneCoroutine(SceneName.ToString())); };

        BasicBlackFadeTransition.instance.gameObject.SetActive(true);
        BasicBlackFadeTransition.instance.SetupTransition(LoadSceneAction, SceneTransitionFinishedAction);

        IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            while (!asyncLoad.isDone)
            {
                if (asyncLoad.progress >= 0.9f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
                yield return null;
            }
            yield return WaitTipReading();
            LoadingFinsihed?.Invoke();
        }
    }

    IEnumerator WaitTipReading()
    {
        float timer = 0f;
        float waitTime = 0;

        while (timer < waitTime && !Input.GetMouseButtonDown(0) && Input.touchCount == 0)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            while (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                yield return null;
            }
        }
    }

void SceneTransitionFinished(SceneNames processThisSceneTransitionFinish)
{
    Debug.Log($"✅ Scene Transition Finished: {processThisSceneTransitionFinish}");
    Time.timeScale = 1;
    switch (processThisSceneTransitionFinish)
    {
        case SceneNames.NewDayScene:
            // ✅ CHECK IF BUTTON MODE FIRST
            if (GameFlowManager.isButtonMode)//false
            {
                Debug.Log("🎮 Button Mode - Loading CutsceneScene");
                
                // Set the dialogue node from button
                DialogueScriptCommandHandler.currentNode = GameFlowManager.buttonYarnNode;
                
                // Load CutsceneScene (skip NewDayManager)
                LoadScene(SceneNames.CutsceneScene);
            }
            else
            {
                // Normal campaign flow
                if (NewDayManager.instance != null)
                Debug.LogError("🎮 Normal Campaign Mode - Starting New Day Sequence");
                    NewDayManager.instance.BeginNewDaySequence();
            }
            break;
            
        // case SceneNames.CutsceneScene:
        //     // StartCoroutine(StartDialogueWhenReady());
        //     Debug.LogError("❌ Starting dialogue directly without coroutine");
        //     //  YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(DialogueScriptCommandHandler.currentNode);
        //     break;
    }
}


// IEnumerator StartDialogueWhenReady()
// {
//     // Wait for YarnDialogSystemSingleTonMaker to initialize
//     float timeout = 5f;
//     float elapsed = 0f;
    
//     while (YarnDialogSystemSingleTonMaker.instance == null && elapsed < timeout)
//     {
//         elapsed += Time.deltaTime;
//         yield return null;
//     }

//     if (YarnDialogSystemSingleTonMaker.instance != null && 
//         !string.IsNullOrEmpty(DialogueScriptCommandHandler.currentNode))
//     {
//         Debug.Log($"✅ Starting dialogue: {DialogueScriptCommandHandler.currentNode}");
//         YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(DialogueScriptCommandHandler.currentNode);
//     }
//     else
//     {
//         Debug.LogError("❌ YarnDialogSystemSingleTonMaker not ready or currentNode is empty");
//     }
// }

    // IEnumerator StartDialogueWhenReady()
    // {
    //     // Wait for YarnDialogSystemSingleTonMaker to initialize
    //     float timeout = 5f;
    //     float elapsed = 0f;
        
    //     while (YarnDialogSystemSingleTonMaker.instance == null && elapsed < timeout)
    //     {
    //         elapsed += Time.deltaTime;
    //         yield return null;
    //     }

    //     if (YarnDialogSystemSingleTonMaker.instance != null && 
    //         !string.IsNullOrEmpty(DialogueScriptCommandHandler.currentNode))
    //     {
    //         Debug.Log($"✅ Starting dialogue: {DialogueScriptCommandHandler.currentNode}");
    //         YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(DialogueScriptCommandHandler.currentNode);
    //     }
    //     else
    //     {
    //         Debug.LogError("❌ YarnDialogSystemSingleTonMaker not ready or currentNode is empty");
    //     }
    // }


}