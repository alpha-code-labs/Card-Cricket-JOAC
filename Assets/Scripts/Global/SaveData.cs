using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;


[System.Serializable]
 public class UsernameData
{
    public string userName;
    public string deviceInfo;

    public UsernameData()
    {
        userName = "";
         deviceInfo = ""; 
        
    }
}
[System.Serializable]
public class SaveData
{
    public string currentDate;//Always in format YYYY/MM/DD
    public int humility;
    public int courage;
    public int resourcefulness;
    public int foresight;
    public string userName;

     public float strikeRate;  //Strike Rate for Leaderboards
    public float battingAverage; //Batting Average for Leaderboards
   
     public bool hasCampaignStarted;
   public bool hasCompletedChapter1;
    // Constructor with default values
    public SaveData()
    {
        currentDate = "1988/07/18";//B-Day Scene . This is Game Start Date Do not Modify this ever
#if UNITY_EDITOR
        // currentDate = "1988/07/23";// Tutorial Scene
        // currentDate = "1989/01/31";// Multi Scene Day  
        // currentDate = "1990/03/02";//First FreeTime
        // currentDate = "1990/04/15";//Last Quiz Scene
        // currentDate = "1990/04/30";// Last day of Chapter 1
        // currentDate = "1990/03/15";// Cricket Gameplay 4

#endif
        humility = 0;
        courage = 0;
        resourcefulness = 0;
        foresight = 0;
        strikeRate = 0f;
        battingAverage = 0f;
        userName = "";
        hasCompletedChapter1 = false;
        hasCampaignStarted = false;
    }
}
