using UnityEngine;

/// <summary>
/// Holds gameplay data that should be loaded after visual novel ends
/// Static data persists between scenes
/// </summary>
public static class GameplayDataHolder
{
    public static GameplayType gameplayType = GameplayType.None;
    public static string gameplayScene = "";
    public static int gameplayNumber = 0;

    public static void SetGameplayData(GameplayType type, string scene, int number)
    {
        gameplayType = type;
        gameplayScene = scene;
        gameplayNumber = number;
        Debug.Log($"📦 Gameplay Data Set: Type={type}, Scene={scene}, Number={number}");
    }

    public static void ClearGameplayData()
    {
        gameplayType = GameplayType.None;
        gameplayScene = "";
        gameplayNumber = 0;
    }
}