using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CurrencyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] string tooltipText;
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowCurrencyTooltip();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        HideCurrencyTooltip();
    }
    void ShowCurrencyTooltip()
    {
        CurrencyToolTip.instance.ShowToolTip(tooltipText);
    }
    void HideCurrencyTooltip()
    {
        CurrencyToolTip.instance.ShowToolTip("");
    }
}
