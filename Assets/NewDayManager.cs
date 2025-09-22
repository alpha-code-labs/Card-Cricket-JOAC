using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewDayManager : MonoBehaviour
{
    TextMeshProUGUI dateText;
    void Start()
    {
        dateText = GetComponentInChildren<TextMeshProUGUI>();
        dateText.text = GameManager.instance.currentSaveData.currentDate;
    }
}
