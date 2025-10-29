using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CurrencyToolTip : MonoBehaviour
{
    public static CurrencyToolTip instance;
    [SerializeField] Image Background;
    [SerializeField] TextMeshProUGUI currencyToolTip;
    private const string FADE_TWEEN_ID = "CurrencyToolTipFade";
    void Awake()
    {
        instance = this;
    }

    public void ShowToolTip(string tooltip)
    {
        // Kill any existing fade tween when setting a non-blank tooltip
        if (!string.IsNullOrEmpty(tooltip) && !string.IsNullOrWhiteSpace(tooltip))
        {
            DOTween.Kill(FADE_TWEEN_ID);

            // Ensure UI elements are visible when showing tooltip
            if (Background != null)
                Background.color = new Color(Background.color.r, Background.color.g, Background.color.b, 1f);
            if (currencyToolTip != null)
                currencyToolTip.color = new Color(currencyToolTip.color.r, currencyToolTip.color.g, currencyToolTip.color.b, 1f);
            currencyToolTip.text = tooltip;
        }
        // Start fade out tween if tooltip is empty
        if (string.IsNullOrEmpty(tooltip) || string.IsNullOrWhiteSpace(tooltip))
        {
            StartFadeOut();
        }
    }

    private void StartFadeOut()
    {
        Sequence fadeSequence = DOTween.Sequence();
        fadeSequence.SetId(FADE_TWEEN_ID);

        if (Background != null)
        {
            fadeSequence.Join(Background.DOFade(0f, 0.5f));
        }

        if (currencyToolTip != null)
        {
            fadeSequence.Join(currencyToolTip.DOFade(0f, 0.5f));
        }
    }

    private void OnDestroy()
    {
        // Clean up tween when object is destroyed
        DOTween.Kill(FADE_TWEEN_ID);
    }
}
