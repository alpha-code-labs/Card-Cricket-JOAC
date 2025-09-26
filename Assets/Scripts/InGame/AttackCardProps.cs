using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class AttackCardProps : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public AttackCardData cardData;

    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI energyCostText;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Image cardImage;

    [Header("Sprite Reffs")]
    [SerializeField] List<Sprite> sprites;

    // Reference to the arc manager
    private SimpleHandArcManager arcManager;

    [SerializeField] Image BatterImage;


    void Start()
    {
        string title = GetTitle();
        titleText.text = title;
        UpdateDescription();

        // Find the arc manager in parent
        arcManager = GetComponentInParent<SimpleHandArcManager>();
    }

    [ContextMenu("Update Description")]
    void UpdateDescription()
    {
        descriptionText.text = "";
        energyCostText.text = cardData.EnergyCost.ToString();
    }

    string GetTitle()
    {
        string title = cardData.excelBattinStrategy.ToString()
            .Replace("Shot", " Shot")
            .Replace("Drive", " Drive")
            .Replace("Glance", " Glance")
            .Replace("Defense", " Defense");

        Sprite selectedSprite = cardData.cardSprite;
        if (selectedSprite != null)
            cardImage.sprite = selectedSprite;

        return title;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }

    // Add IPointerEnterHandler implementation
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (arcManager != null)
        {
            arcManager.OnCardHoverEnterDirect(gameObject);
        }
    }

    // Add IPointerExitHandler implementation
    public void OnPointerExit(PointerEventData eventData)
    {
        if (arcManager != null)
        {
            arcManager.OnCardHoverExitDirect(gameObject);
        }
    }

    void OnClick()
    {
        if (CardPlayAnimationController.Instance != null && CardPlayAnimationController.Instance.IsAnimating)
            return;

        // if (EnergyManager.Instance.GetEnergy() < cardData.EnergyCost)
        // {
        //     Debug.LogWarning("Not enough energy to play this card.");
        //     return;
        // }
        CardsPoolManager.Instance.SetCardsInteractable(false);
        // Deduct energy
        // EnergyManager.Instance.HandelEnergyChange(cardData.EnergyCost);
        
        // Get sprite and pass card object
        Sprite sprite = GetCardSprite();
        // CardsPoolManager.Instance.DestroyCurrentBallCard();
        ScoreManager.Instance.PlayExcelBattingStrategy(cardData.excelBattinStrategy, gameObject, sprite);
    }

    public Sprite GetCardSprite()
    {
        return cardImage != null ? cardImage.sprite : null;
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