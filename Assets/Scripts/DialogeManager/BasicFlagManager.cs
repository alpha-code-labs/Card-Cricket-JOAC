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

    [YarnCommand("SetAryanPath")]
    public static void SetAryanPath(string path)
    {
        switch (path)
        {
            case "Serious":
                Instance.aryanPath = AryanPath.Serious;
                break;
            case "Angry":
                Instance.aryanPath = AryanPath.Angry;
                break;
            case "Quizical":
                Instance.aryanPath = AryanPath.Quizical;
                break;
            default:
                Debug.LogError("Invalid path string passed to SetAryanPath");
                break;
        }
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