using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    public static SceneManager instance;
    [SerializeField] TextMeshProUGUI tip;
    void Awake()
    {
        instance = this;
    }
    public static void DevLoadScene(SceneNames SceneName)
    {
        // AndroidHelper.ShowToast("Loading Dev Scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName.ToString());
    }

    public void LoadScene(SceneNames SceneName)
    {
        // FirebaseManager.OnSceneTransistion(SceneName);
        Action action = (() =>
        {
            StartCoroutine(LoadSceneCoroutine(SceneName.ToString()));
        });

        AnimateLoadingScreen(action);//this is the intro animation

        IEnumerator LoadSceneCoroutine(string sceneName)
        {
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
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
            StartCoroutine(AnimateLoadingScreenFinished(SceneName));
        }
    }
    [SerializeField] RectTransform LoadingScreenImage;
    void AnimateLoadingScreen(Action doAfterLoadingScreenHasStarted)
    {
        //Show Tip
        tip.text = GetRandomTip();
        // Ensure DOTween is imported: using DG.Tweening;
        LoadingScreenImage.anchoredPosition = new Vector2(0, Screen.height);
        LoadingScreenImage.gameObject.SetActive(true);
        LoadingScreenImage.DOAnchorPosY(0, 0.8f)
            .SetEase(Ease.OutBounce)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Time.timeScale = 0;
                doAfterLoadingScreenHasStarted?.Invoke();
                DOTween.KillAll();
                // Debug.Log("Loading screen animation complete.");
                // StartCoroutine(WaitAndAnimateLoadingScreenFinished());
            });
    }

    IEnumerator AnimateLoadingScreenFinished(SceneNames processThisSceneTransitionFinish)
    {
        yield return WaitTipReading();
        // Animate the loading screen out (slide up and hide)
        LoadingScreenImage.DOAnchorPosY(Screen.height, 0.6f)
            .SetEase(Ease.InBack)
            .SetUpdate(true) // Ensures tween runs even if timeScale is 0
            .OnComplete(() =>
            {
                LoadingScreenImage.gameObject.SetActive(false);
                SceneTransitionFinished(processThisSceneTransitionFinish);
            });
    }
    IEnumerator WaitTipReading()
    {
        float timer = 0f;
        float waitTime = 5f;
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
    private List<string> basicTips = new List<string>
    {
        "You can modify the setting mid-game from the pause menu, but remember some settings only apply after a level restart.",
        "You can drag less to do a lower power jump it can be handy sometimes.",
        "A precise 89-degree high-power jump can get you up some super tall platforms.",
        "A simple tap after a jump will try to repeat your last jump.",
        "You can click on hints to confirm which paths are easier, but they tend to be pretty slow.",
        "Use low-power jumps to micro-adjust your character.",
        "Click on 'Local' to see the record of your own scores and track how you’ve improved.",
        "You can choose to pull or drag the arrow from the settings menu.",
        "You don’t have to immediately swipe you can hold your jumps and adjust until ready.",
        "You don’t have to click on the character you can start pulling or dragging from anywhere on screen.",
        "You are a champ, but can your friends clear the levels you can?",
        "You can change your profile name from the profile icon on the main menu.",
        "Pink slime is waiting for your rescue never give up!",
        "The witch wants to make a beauty potion out of pink slime rescue her before that happens!"
    };
    private List<string> badTips = new List<string>
    {
        "Git gud.",
        "Skill issue.",
        "***"
    };
    int tipCount = 0;
    public string GetRandomTip()
    {
        tipCount++;
        List<string> tipsPool = new List<string>();
        tipsPool.AddRange(basicTips);
        if (tipCount > 30)
            tipsPool.AddRange(badTips);



        int randomIndex = UnityEngine.Random.Range(0, tipsPool.Count);
        return tipsPool[randomIndex];
    }
}


public enum SceneNames
{
    BasicLevel,
    Cutscene,
    UI,
    FinalCutscene
}
public enum TransitionTypes
{
    None,
    FadeBlack,
    FilmGrainWithDate,
    DayEvening,
    OutBounce//Mochi the Blob Style
}