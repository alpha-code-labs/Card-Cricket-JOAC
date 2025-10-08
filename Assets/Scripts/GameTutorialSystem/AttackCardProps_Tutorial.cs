using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class AttackCardProps_Tutorial : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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

    private string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
            return camelCase;

        // First, handle any existing spaces by splitting the string
        string[] words = camelCase.Split(' ');
        
        for (int i = 0; i < words.Length; i++)
        {
            // For each word, insert spaces before capital letters (except the first one)
            words[i] = Regex.Replace(words[i], "([a-z])([A-Z])", "$1 $2");
            
            // Also handle cases where there are consecutive capitals followed by lowercase
            // e.g., "XMLParser" -> "XML Parser"
            words[i] = Regex.Replace(words[i], "([A-Z]+)([A-Z][a-z])", "$1 $2");
            
            // Capitalize the first letter of each word
            if (words[i].Length > 0)
            {
                words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
            }
        }
        
        // Join all words back together with spaces
        return string.Join(" ", words);
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

        return CamelCaseToTitleCase(title);
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
        CardsPoolManager_Tutorial.Instance.SetCardsInteractable(false);
        // Deduct energy
        // EnergyManager.Instance.HandelEnergyChange(cardData.EnergyCost);
        
        // Get sprite and pass card object
        Sprite sprite = GetCardSprite();
        // CardsPoolManager.Instance.DestroyCurrentBallCard();
        ScoreManager_Tutorial.Instance.PlayExcelBattingStrategy(cardData.excelBattinStrategy, gameObject, sprite);
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