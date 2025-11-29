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
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // ✅ LOAD DATA HERE - Runs before any Start() methods
            SaveSystem.LoadData();
            Debug.Log($"✅ Data loaded in Awake - hasCampaignStarted: {currentSaveData.hasCampaignStarted}");
        }
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        NewDayManager.currentEventIndex = 0;//Day starts from 0 if game is starting from main menu
        NewDayManager.isEvening = false;
        // SaveSystem.LoadData();//Load data at the start of the game
        SaveSystem.SaveUsernameOnly();

         FirestoreManager firestoreManager = FirestoreManager.GetInstance();
        if (firestoreManager != null)
        {
            firestoreManager.LoadUsernameFromFirestore();
            
            // ✅ Also save to Firestore
            Invoke(nameof(SaveUsernameToFirestore), 1f);
        }
        
        Debug.Log("✅ GameManager started and save data loaded.");

        
    }
    private void SaveUsernameToFirestore()
    {
        FirestoreManager firestoreManager = FirestoreManager.GetInstance();
        if (firestoreManager != null)
        {
            firestoreManager.SaveUsernameToFirestore(currentSaveData.userName);
        }
    }


    public void SaveStatsToFirestore(float strikeRate, float battingAverage)
    {
        FirestoreManager firestoreManager = FirestoreManager.GetInstance();
        if (firestoreManager != null)
        {
            firestoreManager.SaveStatsToFirestore(strikeRate, battingAverage);
            Debug.Log($"✅ GameManager: Saved stats to Firestore - SR: {strikeRate:F1}, BA: {battingAverage:F1}");
        }
    }

    // ✅ NEW - Load stats from Firestore (optional - for future use)
    public void LoadStatsFromFirestore()
    {
        FirestoreManager firestoreManager = FirestoreManager.GetInstance();
        if (firestoreManager != null)
        {
            Debug.Log("✅ GameManager: Loading stats from Firestore...");
            // You can add load methods if needed later
        }
    }
}
