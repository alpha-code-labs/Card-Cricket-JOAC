using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Loading;
using UnityEngine;
using UnityEngine.UI;

public class BasicBlackFadeTransition : MonoBehaviour
{
    [ContextMenu("Test Animation")]
    public void TestAnimation()
    {
        SetupTransition(() => { Debug.Log("Intro Complete"); }, () => { Debug.Log("Outro Complete"); });
    }
    [SerializeField] Image image;
    Action OnOutroCompleteFin;
    public void SetupTransition(Action OnIntroComplete, Action OnOutroComplete)
    {
        OnOutroCompleteFin = OnOutroComplete;
        image.color = new Color(0, 0, 0, 0);
        image.enabled = true;
        IntroTransition();
        void IntroTransition()
        {
            // Debug.Log("Intro Transition");
            image.DOFade(1, 1f).OnComplete(() =>
            {
                OnIntroComplete?.Invoke();
            });
        }
    }
    void OutroTransition()
    {
        // Debug.Log("Outro Transition");
        image.color = new Color(0, 0, 0, 1);

        image.DOFade(0, 1f).OnComplete(() =>
        {
            OnOutroCompleteFin?.Invoke();
            gameObject.SetActive(false);
        });
    }
    void OnEnable()
    {
        TransitionScreenManager.instance.LoadingFinsihed += OutroTransition;
    }
    void OnDisable()
    {
        TransitionScreenManager.instance.LoadingFinsihed -= OutroTransition;
    }
}
