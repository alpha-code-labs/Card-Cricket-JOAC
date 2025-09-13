using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR;

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
    [SerializeField] int maxHandSize = 5;
    [Header("Reffrences")]
    [SerializeField] Transform hand; // Transform to parent drawn cards
    [SerializeField] TextMeshProUGUI BallThrowText; // Text to display current BallThrow
    public static CardsPoolManager Instance;
    public static event System.Action OnTurnStarted;
    public GameObject cardPrefab; // Assign in inspector
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
    void StartTurn()
    {
        ScoreManager.Instance.UpdateBallsAndOvers(CurrntTurn);
        BallThrowText.text = CurrentBallThrow.ToString();
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
        OnTurnStarted?.Invoke();
    }
    [ContextMenu("End Turn")]
    public void EndTurn()
    {
        // Logic to end a turn, e.g., moving cards from HandCards to DiscardPile
        foreach (var card in HandCards)
        {
            DiscardPile.Add(card);
            card.gameObject.SetActive(false); // Optionally deactivate the card
        }
        HandCards.Clear();
        // EnergyManager.Instance.IncreaseEnergy(2); // Increment energy at the end of the turn
        CurrntTurn++; // Increment the turn number
        StartTurn(); // Start the next turn
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

    [ContextMenu("Init Text Deck")]
    void InitTextDeck()
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
            BallThrows.Add(ExcelDataSOManager.Instance.outComeCalculator.GetRandomBallThrow(bowlerType, bowlerSide));
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
