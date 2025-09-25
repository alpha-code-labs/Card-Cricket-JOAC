using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CurrencyUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TextMeshProUGUI currencyText;//Amount
    [SerializeField] Reward currencyType;
    void Start()
    {
        switch (currencyType)
        {
            case Reward.Courage:
                currencyText.text = GameManager.instance.currentSaveData.courage.ToString();
                break;
            case Reward.Humility:
                currencyText.text = GameManager.instance.currentSaveData.humility.ToString();
                break;
            case Reward.Foresight:
                currencyText.text = GameManager.instance.currentSaveData.foresight.ToString();
                break;
            case Reward.Resourcefulness:
                currencyText.text = GameManager.instance.currentSaveData.resourcefulness.ToString();
                break;
            default:
                currencyText.text = "0";
                break;
        }
    }
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
