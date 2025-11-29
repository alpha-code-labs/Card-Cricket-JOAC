using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayButtonData", menuName = "Game/GameplayButtonData")]
public class GameplayButtonData : ScriptableObject
{
    [SerializeField] public List<ButtonDayRecord> days;
}

[Serializable]
public class ButtonDayRecord
{
    public int dayNumber;              // 1-15
    public string sceneName;           // "CutsceneScene"
    public string yarnNodeName;        // "scene_7"
    public GameplayType gameplayType;  // CardGameplay, QuizGameplay, None
    public string gameplayScene;       // "CardGameScene"
    public int gameplayNumber;         // 1, 2, 3...
}

[Serializable]
public enum GameplayType
{
    None = 0,
    CardGameplay = 1,
    QuizGameplay = 2,
    CardGameplayTutorial = 3
}