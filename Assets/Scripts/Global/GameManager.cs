using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public SaveData currentSaveData;
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        NewDayManager.currentEventIndex = 0;//Day starts from 0 if game is starting from main menu
        SaveSystem.LoadData();
    }
}
