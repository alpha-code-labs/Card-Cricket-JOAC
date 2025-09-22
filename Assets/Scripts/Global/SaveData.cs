using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string currentDate = "1998-07-18"; // B-Day Scene
    public int humility;
    public int courage;
    public int resourcefulness;
    public int foresight;

    // Constructor with default values
    public SaveData()
    {
        currentDate = "1998-07-18";
        humility = 0;
        courage = 0;
        resourcefulness = 0;
        foresight = 0;
    }
}

public static class SaveSystem
{
    private static SaveData _currentSave;
    private static readonly string SaveFileName = "gamedata.json";

    // Property to access current save data
    public static SaveData Current
    {
        get
        {
            if (_currentSave == null)
            {
                LoadData();
            }
            return _currentSave;
        }
    }

    // Get the full path for the save file
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    // Load data from file or create new if none exists
    public static void LoadData()
    {
        string savePath = GetSavePath();

        if (File.Exists(savePath))
        {
            try
            {
                string jsonData = File.ReadAllText(savePath);
                _currentSave = JsonUtility.FromJson<SaveData>(jsonData);
                Debug.Log("Save data loaded successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                _currentSave = new SaveData(); // Create new if loading fails
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new save data.");
            _currentSave = new SaveData();
        }
    }

    // Save data to file
    public static void SaveDataToFile()
    {
        if (_currentSave == null)
        {
            Debug.LogWarning("No save data to write!");
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(_currentSave, true);
            string savePath = GetSavePath();
            File.WriteAllText(savePath, jsonData);
            Debug.Log($"Save data written to: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

    // Utility methods for easy access to stats
    public static void ModifyHumility(int amount)
    {
        Current.humility += amount;
    }

    public static void ModifyCourage(int amount)
    {
        Current.courage += amount;
    }

    public static void ModifyResourcefulness(int amount)
    {
        Current.resourcefulness += amount;
    }

    public static void ModifyForesight(int amount)
    {
        Current.foresight += amount;
    }

    // Reset save data
    public static void ResetData()
    {
        _currentSave = new SaveData();
        SaveDataToFile();
    }
}