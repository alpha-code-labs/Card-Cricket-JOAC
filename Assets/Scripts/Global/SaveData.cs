using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string currentDate;//Always in format YYYY/MM/DD
    public int humility;
    public int courage;
    public int resourcefulness;
    public int foresight;

    // Constructor with default values
    public SaveData()
    {
        currentDate = "1988/07/18";//B-Day Scene 
#if UNITY_EDITOR
        currentDate = "1989/01/31";// Multi Scene Day  
        // currentDate = "1990/03/02";//First FreeTime
        // currentDate = "1990/04/15";//Last Quiz Scene
#endif
        humility = 0;
        courage = 0;
        resourcefulness = 0;
        foresight = 0;
    }
}
