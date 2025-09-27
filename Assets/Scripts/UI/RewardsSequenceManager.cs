using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class RewardsSequenceManager : MonoBehaviour
{
    public static RewardsSequenceManager instance;
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
    }
    void Start()
    {
        ProcessSprites();
        ResetSequence();
    }
    [SerializeField] GameObject RewardPanel;//Contains all the below elements
    [SerializeField] Image RewardImage;
    [SerializeField] Image Bar;
    [SerializeField] Image BarFillImage;//0,0.2,0.4,0.6,0.8,1
    [SerializeField] List<Sprite> sprites;
    [SerializeField] TextMeshProUGUI RankUpText;
    Dictionary<Reward, Sprite> rewardSpriteDict;
    void ProcessSprites()
    {
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
    [ContextMenu("TestShowReward")]
    public void TestShowReward()
    {
        ShowRewardImage("Courage");
    }
    [YarnCommand("ShowRewardImage")]
    public static void ShowRewardImage(string rewardType)//this gives and shows rewards
    {
        //Parsing
        Reward reward;
        System.Enum.TryParse<Reward>(rewardType, out reward);

        int currantStat = 0;
        switch (reward)
        {
            case Reward.Courage:
                GameManager.instance.currentSaveData.courage = Mathf.Min(GameManager.instance.currentSaveData.courage + 1, 5);
                currantStat = GameManager.instance.currentSaveData.courage;
                break;
            case Reward.Foresight:
                GameManager.instance.currentSaveData.foresight = Mathf.Min(GameManager.instance.currentSaveData.foresight + 1, 5);
                currantStat = GameManager.instance.currentSaveData.foresight;
                break;
            case Reward.Humility:
                GameManager.instance.currentSaveData.humility = Mathf.Min(GameManager.instance.currentSaveData.humility + 1, 5);
                currantStat = GameManager.instance.currentSaveData.humility;
                break;
            case Reward.Resourcefulness:
                GameManager.instance.currentSaveData.resourcefulness = Mathf.Min(GameManager.instance.currentSaveData.resourcefulness + 1, 5);
                currantStat = GameManager.instance.currentSaveData.resourcefulness;
                break;
        }
        //Animations
        Sprite sprite = instance.rewardSpriteDict[reward];
        if (sprite != null)
        {
            instance.RewardPanel.SetActive(true);
            instance.RewardImage.sprite = sprite;
            instance.RewardImage.transform.localScale = Vector3.zero;
            instance.RewardImage.transform.rotation = Quaternion.identity;
            instance.RewardImage.color = new Color(1, 1, 1, 0);
            instance.BarFillImage.fillAmount = 0;
            instance.RewardImage.gameObject.SetActive(true);
            Vector3 originalPos = instance.RewardImage.transform.localPosition;
            instance.RewardImage.transform.localPosition = originalPos + new Vector3(0, 300, 0);


            float targetFill = ((int)currantStat) * 0.2f;

            Sequence seq = DOTween.Sequence();
            seq.Append(instance.RewardImage.transform.DOLocalMove(originalPos, 0.6f).SetEase(Ease.OutBounce));
            seq.Join(instance.RewardImage.transform.DOScale(Vector3.one * 1.2f, 0.6f).SetEase(Ease.OutBack));
            seq.Join(instance.RewardImage.transform.DORotate(new Vector3(0, 0, 720), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));
            seq.Join(instance.RewardImage.DOFade(1, 0.6f));
            seq.Append(instance.RewardImage.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBounce));
            seq.Append(instance.Bar.DOFade(1, 0.5f));
            seq.Join(instance.BarFillImage.DOFade(1, 0.5f));
            seq.Append(instance.BarFillImage.DOFillAmount(targetFill, 2f).SetEase(Ease.OutQuad));
            seq.AppendCallback(() =>
            {
                instance.RankUpText.text = "Rank Up!";
                instance.RankUpText.color = new Color(1, 1, 1, 0);
                instance.RankUpText.gameObject.SetActive(true);
            });
            seq.Append(instance.RankUpText.DOFade(1, 0.5f));
            seq.SetId("ShowRewardSequence");
        }
        else
        {
            Debug.LogWarning($"Sprite with name {rewardType} not found in Resources/Sprites/Rewards/");
        }
    }
    [YarnCommand("HideRewardImage")]
    public static void HideRewardImage()
    {
        DOTween.Kill("ShowRewardSequence", true);
        Sequence hideSeq = DOTween.Sequence();
        hideSeq.Append(instance.RewardImage.DOFade(0, 0.5f));
        hideSeq.Join(instance.Bar.DOFade(0, 0.5f));
        hideSeq.Join(instance.BarFillImage.DOFade(0, 0.5f));
        hideSeq.Join(instance.RankUpText.DOFade(0, 0.5f));
        hideSeq.SetDelay(.1f).OnComplete(() =>
        {
            instance.ResetSequence();
        });
        hideSeq.SetId("HideRewardSequence").OnKill(() => instance.ResetSequence());
    }
    void ResetSequence()
    {
        instance.RewardImage.transform.localScale = Vector3.zero;
        instance.RewardImage.transform.rotation = Quaternion.identity;
        instance.RewardImage.color = new Color(1, 1, 1, 0);
        instance.Bar.color = new Color(1, 1, 1, 0);
        instance.BarFillImage.color = new Color(1, 1, 1, 0);
        instance.BarFillImage.fillAmount = 0;
        instance.RankUpText.color = new Color(1, 1, 1, 0);
        instance.RewardImage.gameObject.SetActive(false);
        instance.RankUpText.gameObject.SetActive(false);
        instance.RewardPanel.SetActive(false);
    }
}
