
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class SaveSystem
{
    private static readonly string SaveFileName = "gamedata.json";

    // Get the full path for the save file
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }
    [MenuItem("Tools/Open Save Folder")]//This is prorably create compile time erros in build
    public static void OpenSaveFolder()
    {
        string savePath = GetSavePath();
        string folderPath = Path.GetDirectoryName(savePath);

        // Create the directory if it doesn't exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Open the folder in the system's file explorer
        OpenInFileExplorer(folderPath);

    }
    private static void OpenInFileExplorer(string path)
    {
#if UNITY_EDITOR_WIN
        Process.Start("explorer.exe", $"\"{path}\"");
#elif UNITY_EDITOR_OSX
        Process.Start("open", $"\"{path}\"");
#elif UNITY_EDITOR_LINUX
        Process.Start("xdg-open", $"\"{path}\"");
#endif
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
                GameManager.instance.currentSaveData = JsonUtility.FromJson<SaveData>(jsonData);
                Debug.Log("Save data loaded successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                GameManager.instance.currentSaveData = new SaveData(); // Create new if loading fails
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new save data.");
            GameManager.instance.currentSaveData = new SaveData();
        }
    }

    // Save data to file
    public static void SaveDataToFile()
    {
        if (GameManager.instance.currentSaveData == null)
        {
            Debug.LogWarning("No save data to write!");
            return;
        }

        try
        {
            string jsonData = JsonUtility.ToJson(GameManager.instance.currentSaveData, true);
            string savePath = GetSavePath();
            File.WriteAllText(savePath, jsonData);
            Debug.Log($"Save data written to: {savePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save data: {e.Message}");
        }
    }

}