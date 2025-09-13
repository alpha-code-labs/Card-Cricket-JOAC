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
        energyCostText.text = cardData.EnergyCost.ToString();
    }
    string GetTitle()
    {
        string title = cardData.excelBattinStrategy.ToString().Replace("Shot", " Shot").Replace("Drive", " Drive").Replace("Glance", " Glance").Replace("Defense", " Defense");

        Sprite selectedSprite = cardData.cardSprite;
        if (selectedSprite != null)
            cardImage.sprite = selectedSprite;

        return title;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }
    void OnClick()
    {
        if (EnergyManager.Instance.GetEnergy() < cardData.EnergyCost)
        {
            Debug.LogWarning("Not enough energy to play this card.");
            return;
        }
        else
        {
            EnergyManager.Instance.HandelEnergyChange(cardData.EnergyCost);
        }
        ScoreManager.Instance.PlayExcelBattingStrategy(cardData.excelBattinStrategy);
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
