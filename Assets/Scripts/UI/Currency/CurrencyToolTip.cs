using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyToolTip : MonoBehaviour
{
    public static CurrencyToolTip instance;
    void Awake()
    {
        instance = this;
    }
    [SerializeField] TextMeshProUGUI currencyToolTip;
    public void ShowToolTip(string tooltip)
    {
        currencyToolTip.text = tooltip;
    }
}
