using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DateHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI dateText;
    void Start()
    {
        if (NewDayManager.currentDateRecord == null)
        {
            Debug.LogError("This scene is not supposed to be loaded directly, it needs NewDayManager to set the date");
            dateText.text = "Null Date";
            return;
        }
        dateText.text = NewDayManager.currentDateRecord.date + "\n" + (NewDayManager.isEvening ? " Evening" : " Day");
    }
}
