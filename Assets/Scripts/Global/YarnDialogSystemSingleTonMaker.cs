using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class YarnDialogSystemSingleTonMaker : MonoBehaviour
{
    public static YarnDialogSystemSingleTonMaker instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        dialogueRunner = GetComponent<DialogueRunner>();
        ProcessSprites();
    }
    public DialogueRunner dialogueRunner;
    [YarnCommand("AutoAdvance")]
    public static void AutoAdvance(bool isAuto)
    {
        instance.dialogueRunner.GetComponentInChildren<LinePresenter>().autoAdvance = isAuto;
    }
    [SerializeField] Image RewardImage;
    [SerializeField] List<Sprite> sprites;
    Dictionary<Reward, Sprite> rewardSpriteDict;
    void ProcessSprites()
    {
        RewardImage.gameObject.SetActive(false);
        rewardSpriteDict = new Dictionary<Reward, Sprite>();
        foreach (var sprite in sprites)
        {
            Reward reward;
            string spriteName = sprite.name.Split('-')[0]; // Remove '-' and everything after
            if (System.Enum.TryParse<Reward>(spriteName, out reward))
            {
                rewardSpriteDict[reward] = sprite;
            }
            else
            {
                Debug.LogWarning($"Sprite with name {sprite.name} does not match any Reward enum value.");
            }
        }
    }
    [YarnCommand("ShowRewardImage")]
    [ContextMenu("ShowRewardImage")]
    public static void ShowRewardImage(string rewardType)
    {
        Reward reward;
        System.Enum.TryParse<Reward>(rewardType, out reward);
        Sprite sprite = instance.rewardSpriteDict[reward];
        if (sprite != null)
        {
            instance.RewardImage.sprite = sprite;
            instance.RewardImage.color = new Color(1, 1, 1, 0);
            instance.RewardImage.gameObject.SetActive(true);
            instance.RewardImage.DOFade(1, 0.5f).OnComplete(() =>
            {
                instance.RewardImage.DOFade(0, 0.5f).SetDelay(2f).OnComplete(() =>
                {
                    instance.RewardImage.gameObject.SetActive(false);
                });
            });
        }
        else
        {
            Debug.LogWarning($"Sprite with name {rewardType} not found in Resources/Sprites/Rewards/");
        }
    }
}
enum Reward
{
    Courage,
    Foresight,
    Humility,
    Resourcefulness,
}
