using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class RewardsSequenceManager : MonoBehaviour
{
    public static RewardsSequenceManager instance;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {

        ProcessSprites();
    }
    [SerializeField] Image RewardImage;
    [SerializeField] Image BarFillImage;//0,0.2,0.4,0.6,0.8,1
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
            instance.RewardImage.DOFade(1, 0.5f);
        }
        else
        {
            Debug.LogWarning($"Sprite with name {rewardType} not found in Resources/Sprites/Rewards/");
        }
        int currantStat = 0;
        switch (reward)
        {
            case Reward.Courage:
                currantStat = GameManager.instance.currentSaveData.courage;
                break;
            case Reward.Foresight:
                currantStat = GameManager.instance.currentSaveData.foresight;
                break;
            case Reward.Humility:
                currantStat = GameManager.instance.currentSaveData.humility;
                break;
            case Reward.Resourcefulness:
                currantStat = GameManager.instance.currentSaveData.resourcefulness;
                break;
        }
        float targetFill = ((int)currantStat + 1) * 0.2f;
        instance.BarFillImage.DOFillAmount(targetFill, 0.5f);
    }
    [YarnCommand("HideRewardImage")]
    public static void HideRewardImage()
    {
        instance.RewardImage.DOFade(0, 0.5f).SetDelay(2f).OnComplete(() =>
                {
                    instance.RewardImage.gameObject.SetActive(false);
                });
    }
}
