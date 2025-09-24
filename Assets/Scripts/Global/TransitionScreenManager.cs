using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionScreenManager : MonoBehaviour
{
    public static TransitionScreenManager instance;
    [SerializeField] BasicBlackFadeTransition basicBlackFadeTransition;

    void Awake()
    {
        instance = this;
    }
    public static void DevLoadScene(SceneNames SceneName)
    {
        // AndroidHelper.ShowToast("Loading Dev Scene");
        SceneManager.LoadScene(SceneName.ToString());
    }
    public void LoadScene(string SceneName)
    {
        LoadScene((SceneNames)Enum.Parse(typeof(SceneNames), SceneName));
    }
    public Action LoadingFinsihed;
    public void LoadScene(SceneNames SceneName)
    {
        // Debug.Log($"Loading Scene: {SceneName}");
        // FirebaseManager.OnSceneTransistion(SceneName);
        Action SceneTransitionFinishedAction = () => { SceneTransitionFinished(SceneName); };//This What Needs to Happen After Transition is Finished
        Action LoadSceneAction = () => { StartCoroutine(LoadSceneCoroutine(SceneName.ToString())); };//This is What Needs to Happen After Transition has Started or During Loading

        // GetComponent<MochiBounceSceneTransition>().AnimateLoadingScreen(LoadSceneAction);//This is the Intro Animation
        basicBlackFadeTransition.gameObject.SetActive(true);

        basicBlackFadeTransition.SetupTransition(LoadSceneAction, SceneTransitionFinishedAction);

        IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Optional: Show loading screen or progress bar here
            while (!asyncLoad.isDone)
            {
                // Optionally check progress: asyncLoad.progress (0.0 to 0.9)
                if (asyncLoad.progress >= 0.9f)
                {
                    // Scene is ready, now activate
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
        float waitTime = 5f;//this is max time to wait
        waitTime = 0;//Setting this to 0 for now as we dont have tips yet
        // bool touched = false;

        // Wait for 3 seconds or until touch/mouse down
        while (timer < waitTime && !Input.GetMouseButtonDown(0) && Input.touchCount == 0)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // If interrupted by touch/mouse, wait for release
        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            // Wait until all touches/mouse are released
            while (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                yield return null;
            }
        }
    }
    void SceneTransitionFinished(SceneNames processThisSceneTransitionFinish)
    {
        Time.timeScale = 1;
    }

}


public enum SceneNames
{
    CardGameScene,
    NewDayScene,
    WorldNav,
    CutsceneScene
}
public enum TransitionTypes
{
    None,
    FadeBlack,
    FilmGrainWithDate,
    DayEvening,
    OutBounce//Mochi the Blob Style
}