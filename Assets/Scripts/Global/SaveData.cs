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
        // currentDate = "1988/07/18";//B-Day Scene 
        currentDate = "1990/03/02";//First FreeTime
        humility = 0;
        courage = 0;
        resourcefulness = 0;
        foresight = 0;
    }
}
