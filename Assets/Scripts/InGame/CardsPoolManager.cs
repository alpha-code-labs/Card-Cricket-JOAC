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
        // InitTextDeck();
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
        // EnergyManager.Instance.IncreaseEnergy(1); // Increment energy at the end of the turn
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
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.BackfootDefense, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.CutShot, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.PullShot, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.ForwardDefense, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.SquareDrive, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.CoverDrive, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.StraightDrive, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.LegDrive, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.LegGlance, EnergyCost = 3 });
        // Deck.Add(new AttackCardData { battingStrategy = BattingStrategy.Block, EnergyCost = 3 });

        BallThrows.Clear();

        // // 1st Over - Fast Bowler (6 balls)
        // // Ball 1: Fast, Off Line, Good Length
        // BallThrows.Add(new BallThrow(BallLength.GoodLength, BallLine.OffLine, BallType.Fast));
        // // Ball 2: In Swing, At the Stumps, Full Length
        // BallThrows.Add(new BallThrow(BallLength.FullLength, BallLine.AtTheStumps, BallType.InSwing));
        // // Ball 3: Fast, Leg Line, Short
        // BallThrows.Add(new BallThrow(BallLength.Short, BallLine.Leg, BallType.Fast));
        // // Ball 4: Out Swing, Off Line, Good Length
        // BallThrows.Add(new BallThrow(BallLength.GoodLength, BallLine.OffLine, BallType.OutSwing));
        // // Ball 5: Fast, At the Stumps, Full Length
        // BallThrows.Add(new BallThrow(BallLength.FullLength, BallLine.AtTheStumps, BallType.Fast));
        // // Ball 6: In Swing, At the Stumps, Good Length
        // BallThrows.Add(new BallThrow(BallLength.GoodLength, BallLine.AtTheStumps, BallType.InSwing));

        // // 2nd Over - Spin Bowler (6 balls)
        // // Ball 1: Off Spin, Off Line, Full Length
        // BallThrows.Add(new BallThrow(BallLength.FullLength, BallLine.OffLine, BallType.OffSpin));
        // // Ball 2: Leg Spin, At the Stumps, Good Length
        // BallThrows.Add(new BallThrow(BallLength.GoodLength, BallLine.AtTheStumps, BallType.LegSpin));
        // // Ball 3: Off Spin, Leg Line, Short
        // BallThrows.Add(new BallThrow(BallLength.Short, BallLine.Leg, BallType.OffSpin));
        // // Ball 4: Leg Spin, Off Line, Good Length
        // BallThrows.Add(new BallThrow(BallLength.GoodLength, BallLine.OffLine, BallType.LegSpin));
        // // Ball 5: Off Spin, At the Stumps, Full Length
        // BallThrows.Add(new BallThrow(BallLength.FullLength, BallLine.AtTheStumps, BallType.OffSpin));
        // // Ball 6: Leg Spin, At the Stumps, Short
        // BallThrows.Add(new BallThrow(BallLength.Short, BallLine.AtTheStumps, BallType.LegSpin));
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
