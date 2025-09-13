using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
[Serializable]
public class AttackCardProps : MonoBehaviour, IPointerClickHandler
{
    public AttackCardData cardData;
    //
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI energyCostText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Image cardImage;
    [Header("Sprite Reffs")]
    [SerializeField] List<Sprite> sprites;
    // Start is called before the first frame update
    void Start()
    {
        string title = GetTitle();
        titleText.text = title;
        UpdateDescription();
    }
    [ContextMenu("Update Description")]
    void UpdateDescription()
    {
        descriptionText.text = "";
        energyCostText.text = "";
    }
    string GetTitle()
    {
        string title = cardData.battingStrategy.ToString().Replace("Shot", " Shot").Replace("Drive", " Drive").Replace("Glance", " Glance").Replace("Defense", " Defense");
        int index = (int)cardData.battingStrategy;
        cardImage.sprite = sprites[index];
        return title;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }
    void OnClick()
    {
        ScoreManager.Instance.PlayCard(cardData.battingStrategy);
    }

    void OnEnable()
    {
        CardsPoolManager.OnTurnStarted += UpdateDescription;
    }
    void OnDisable()
    {
        CardsPoolManager.OnTurnStarted -= UpdateDescription;
    }

}
[Serializable]
public class AttackCardData
{
    [SerializeField] internal BattingStrategy battingStrategy;
    [SerializeField] internal int EnergyCost = 3;
}

public enum BattingStrategy
{
    BackfootDefence,
    CoverDrive,
    Cut,
    ForwardDefence,
    // LegDrive,
    LegGlance,
    Leave,
    OnDrive,
    Pull,
    SquareDrive,
    StraightDrive,
    Sweep
}

public enum OutCome
{
    NoRunWideBall = -2,
    Out = -1,
    NoRun = 0,
    OneRuns = 1,
    TwoRuns = 2,
    FourRuns = 4,
    SixRuns = 6
}
