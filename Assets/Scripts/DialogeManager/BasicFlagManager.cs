using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class BasicFlagManager : MonoBehaviour
{
    public static BasicFlagManager Instance;
    [SerializeField] List<Sprite> sprites;
    [SerializeField] Image currentCharSprite;
    [SerializeField] Image currentBGSprite;
    void Awake()
    {
        Instance = this;
    }
    public int currentDay;
    [SerializeField] public AryanPath aryanPath;
    [YarnFunction("GetAryanPath")]
    public static string GetAryanPath()
    {
        return Instance.aryanPath.ToString();
    }
    [YarnCommand("SetCharSprite")]
    public static void SetCharSprite(int index)
    {
        Instance.currentCharSprite.sprite = Instance.sprites[index];
    }
    [YarnCommand("SetBGSprite")]
    public static void SetBGSprite(int index)
    {
        Instance.currentBGSprite.sprite = Instance.sprites[index];
    }
}

public enum AryanPath
{
    Serious,
    Angry,
    Quizical
}