using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public static class SaveSystem
{
    private static readonly string SaveFileName = "gamedata.json";
    private static readonly string UsernameFileName = "username.json";

    // Get the full path for the save file
    private static string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    // ✅ NEW - Get username file path
    private static string GetUsernamePath()
    {
        return Path.Combine(Application.persistentDataPath, UsernameFileName);
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Open Save Folder")]//This is prorably create compile time erros in build (it indeed did so wrappted it for editor only execution)
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
#endif
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

        // Load username from separate file
        LoadUsername();
    }

    //  Load ONLY username
    private static void LoadUsername()
    {
        string usernamePath = GetUsernamePath();

        if (File.Exists(usernamePath))
        {
            try
            {
                string jsonData = File.ReadAllText(usernamePath);
                UsernameData usernameData = JsonUtility.FromJson<UsernameData>(jsonData);
                GameManager.instance.currentSaveData.userName = usernameData.userName;
                Debug.Log($"✅ Username loaded: {GameManager.instance.currentSaveData.userName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load username: {e.Message}");
                GenerateNewUsername();
            }
        }
        else
        {
            Debug.Log("No username file found. Generating new username...");
            GenerateNewUsername();
        }
    }

    //  Generate new username if doesn't exist
    private static void GenerateNewUsername()
    {
        GameManager.instance.currentSaveData.userName = "Champ" + Random.Range(1, 100).ToString("D2");
        Debug.Log($"✅ Generated username: {GameManager.instance.currentSaveData.userName}");
    }

    // Save ONLY username to separate file
public static void SaveUsernameOnly()
{
    if (GameManager.instance.currentSaveData == null)
    {
        Debug.LogWarning("No save data to write!");
        return;
    }

    try
    {
        UsernameData usernameData = new UsernameData();
        usernameData.userName = GameManager.instance.currentSaveData.userName;
        usernameData.deviceInfo = $"{SystemInfo.deviceModel} | {SystemInfo.operatingSystem}";  // ✅ ADD THIS

        string jsonData = JsonUtility.ToJson(usernameData, true);
        string usernamePath = GetUsernamePath();
        File.WriteAllText(usernamePath, jsonData);
        Debug.Log($"✅ Username & DeviceInfo saved: {usernameData.userName} | {usernameData.deviceInfo}");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to save username: {e.Message}");
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

    // ✅ NEW METHOD - Save stats to JSON
public static void SaveStatsToLocal()
{
    if (GameManager.instance.currentSaveData == null)
    {
        Debug.LogWarning("No save data to write!");
        return;
    }

    try
    {
        string jsonData = JsonUtility.ToJson(GameManager.instance.currentSaveData, true);
        string savePath = GetUsernamePath();
        File.WriteAllText(savePath, jsonData);
        
        Debug.Log($"✅ Stats saved to username.json");
        Debug.Log($"   - StrikeRate: {GameManager.instance.currentSaveData.strikeRate:F1}");
        Debug.Log($"   - BattingAverage: {GameManager.instance.currentSaveData.battingAverage:F1}");
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to save stats: {e.Message}");
    }
}
}