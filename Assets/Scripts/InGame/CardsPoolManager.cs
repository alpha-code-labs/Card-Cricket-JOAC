using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CardsPoolManager : MonoBehaviour
{
    [Header("Card Decks")]
    [SerializeField] List<AttackCardData> Deck;
    [Header("Piles")]

    [SerializeField] List<AttackCardProps> DrawPile;
    [SerializeField] List<AttackCardProps> DiscardPile;
    [SerializeField] List<AttackCardProps> HandCards;
    [Header("Game State Vars")]
    public int CurrntTurn = 0; // Current turn number
    public List<BallThrow> BallThrows; // List to hold BallThrow instances
    [Header("Difficulty Settings")]
    [SerializeField] public int maxHandSize = 5;
    public int maxRedraws = 1; // Maximum redraws per game
    private int redraws = 0; // Track number of redraws used
    private bool cardsInteractable = true;

    [Header("Reffrences")]
    [SerializeField] Transform hand; // Transform to parent drawn cards
    [SerializeField] Transform ballerCardTransform;
    [SerializeField] TextMeshProUGUI BallThrowText; // Text to display current BallThrow
    public static CardsPoolManager Instance;
    public static event System.Action OnTurnStarted;
    public static event System.Action<int, int> OnHandRedrawn; // Event to notify when hand is redrawn
    public GameObject cardPrefab; // Assign in inspector

    public GameObject ballerCard;

    public GameObject ballerCardPrefab;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitTextDeck();// Initialize the deck with random cards for batiing and bowling disable to keep deck in scene
        InstantiateCards();
        StartTurn();
        // EnergyManager.Instance.RefreshEnergyText();
    }
    [ContextMenu("Start Turn")]
    void StartTurn(bool incrementBalls = true)
    {
        if (CurrntTurn >= ScoreManager.Instance.MaxBalls || ScoreManager.Instance.wickets < 1)
        {
            //Game ended 
            
            return;    
        }

        Timer.Instance.StartTurnTimer();
        if (incrementBalls)
            ScoreManager.Instance.UpdateBallsAndOvers(CurrntTurn);
        if (ballerCard != null)
            Destroy(ballerCard);
        ballerCard = InstantiateBallerCard(CurrentBallThrow);
        BallThrowText.text = CurrentBallThrow.ToString();
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
        OnTurnStarted?.Invoke();
    }
    [ContextMenu("End Turn")]
    public void EndTurn(bool incrementBalls = true)
    {
        Timer.Instance.EndTurnTimer();
        // Logic to end a turn, e.g., moving cards from HandCards to DiscardPile
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card);
            card.gameObject.SetActive(false); // Optionally deactivate the card
        }
        
        SetCardsInteractable(true);
        HandCards.Clear();
        // EnergyManager.Instance.IncreaseEnergy(2); // Increment energy at the end of the turn
        CurrntTurn++; // Increment the turn number
        StartTurn(incrementBalls); // Start the next turn
    }
    [ContextMenu("Draw Card")]
    void DrawCard()
    {
        if (DrawPile.Count <= 0)
        {
            DrawPile = new List<AttackCardProps>(DiscardPile);
            DiscardPile.Clear();
        }
        AttackCardProps card = DrawPile[0];
        DrawPile.RemoveAt(0);
        HandCards.Add(card);
        card.gameObject.SetActive(true); // Activate the card when drawn 
        SimpleHandArcManager arcManager = hand.GetComponent<SimpleHandArcManager>();
            if (arcManager != null)
                arcManager.RefreshCardArrangement();
    }

    public void RedrawHand()
    {
        if (redraws >= maxRedraws)
        {
            Debug.LogWarning($"Cannot redraw: Maximum redraws ({maxRedraws}) already used!");
            return;
        }
        
        if (HandCards.Count == 0)
        {
            Debug.LogWarning("No cards in hand to redraw!");
            return;
        }
        
        // Move current hand cards to discard pile
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card);
            card.gameObject.SetActive(false);
        }
        HandCards.Clear();
        
        // Draw new cards
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
        
        redraws++;
        Debug.Log($"Hand redrawn! Redraws used: {redraws}/{maxRedraws}");
        
        // Optional: Trigger an event for UI updates
        OnHandRedrawn?.Invoke(redraws, maxRedraws);
    }

    void InstantiateCards()
    {
        DrawPile.Clear();
        foreach (var cardData in Deck)
        {
            AttackCardProps card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps>();
            card.cardData = cardData; // Set the card data
            DrawPile.Add(card);
            card.gameObject.SetActive(false); // Deactivate the card initially
        }
    }


    public void DestroyCurrentBallCard()
    {
        if (ballerCard != null)
            ballerCard.SetActive(false);
            Destroy(ballerCard);
    }

    GameObject InstantiateBallerCard(BallThrow ballThrow)
    {
        GameObject ballerCard = Instantiate(ballerCardPrefab, ballerCardTransform);
        BallerCardProps cardProps = ballerCard.GetComponent<BallerCardProps>();
        cardProps.assignBallerProps(ballThrow);
        return ballerCard;
    }

    

    [ContextMenu("Init Text Deck")]
    void InitTextDeck(PitchCondition pitchCondition = PitchCondition.Friendly)
    {
        Deck.Clear();
        foreach (BattingStrategy strategy in System.Enum.GetValues(typeof(BattingStrategy)))
        {
            Deck.Add(new AttackCardData(strategy));
        }
        RandomizeDeck();
        BallThrows.Clear();

        //Over - Fast Bowler Right Arm (6 balls)
        // Initialize bowler variables outside the loop
        TypeOfBowler bowlerType = TypeOfBowler.Fast;
        Side bowlerSide = Side.RightArm;
        
        for (int i = 0; i < ScoreManager.Instance.MaxBalls; i++)
        {
            // Randomize bowler type and side every 6 balls (start of each over)
            if (i % 6 == 0)
            {
                bowlerType = (TypeOfBowler)Random.Range(0, System.Enum.GetValues(typeof(TypeOfBowler)).Length);
                bowlerSide = (Side)Random.Range(0, System.Enum.GetValues(typeof(Side)).Length);
            }
            BallThrows.Add(ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition));
        }
    }

    /// <summary>
    /// Randomizes the order of cards in the deck using Fisher-Yates shuffle algorithm
    /// </summary>
    [ContextMenu("Randomize Deck")]
    void RandomizeDeck()
    {
        for (int i = Deck.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AttackCardData temp = Deck[i];
            Deck[i] = Deck[randomIndex];
            Deck[randomIndex] = temp;
        }
        Debug.Log("Deck has been randomized!");
    }

     public bool CanRedraw()
    {
        return redraws < maxRedraws && HandCards.Count > 0;
    }

    public int GetRedrawsRemaining()
    {
        return Mathf.Max(0, maxRedraws - redraws);
    }

    public void ResetRedraws()
    {
        redraws = 0;
    }

    public void SetCardsInteractable(bool interactable)
    {
        cardsInteractable = interactable;
        
        // Disable/enable all card interactions
        foreach (var card in HandCards)
        {
            if (card != null && card.gameObject != null)
            {
                var canvasGroup = card.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = card.gameObject.AddComponent<CanvasGroup>();
                
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
                
                // Optional: visual feedback
                canvasGroup.alpha = interactable ? 1f : 1f;
            }
        }
    }

    public bool AreCardsInteractable()
    {
        return cardsInteractable;
    }

    public BallThrow CurrentBallThrow
    {
        get
        {
            if (BallThrows.Count > 0)
            {
                return BallThrows[CurrntTurn % BallThrows.Count]; // Cycle through BallThrows based on current turn
            }
            return null; // No BallThrow available
        }
    }
}
