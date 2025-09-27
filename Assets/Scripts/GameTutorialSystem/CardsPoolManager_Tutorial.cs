using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Yarn.Unity;

public class CardsPoolManager_Tutorial : MonoBehaviour
{
    [Header("Card Decks")]
    [SerializeField] List<AttackCardData> Deck;
    [Header("Piles")]

    [SerializeField] List<AttackCardProps_Tutorial> DrawPile;
    [SerializeField] List<AttackCardProps_Tutorial> DiscardPile;
    [SerializeField] List<AttackCardProps_Tutorial> HandCards;
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
    public static CardsPoolManager_Tutorial Instance;
    public static event System.Action OnTurnStarted;
    public static event System.Action<int, int> OnHandRedrawn; // Event to notify when hand is redrawn
    public GameObject cardPrefab; // Assign in inspector

    private GameObject ballerCard;
    public string currentTutorialBall = "None";

    [Header("Yarn Spinner References")]
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private InMemoryVariableStorage variableStorage;

    public GameObject ballerCardPrefab;
    void Awake()
    {
         if (dialogueRunner == null)
            dialogueRunner = FindObjectOfType<DialogueRunner>();
        
        if (variableStorage == null)
            variableStorage = dialogueRunner.GetComponent<InMemoryVariableStorage>();
        Instance = this;
    }

    public void showFirstBallingCard()
    {
        BallThrow firstBallThrow = new BallThrow(
                        TypeOfBowler.Fast,
                        Side.RightArm,
                        BallType.Straight,
                        BallLine.OffStump,
                        BallLength.FullLength,
                        PitchCondition.Friendly
                    );
        ballerCard = InstantiateBallerCard(firstBallThrow);
        UIHighlightManager.Instance.HighlightObject(ballerCard);
    }

    public void showShotPanel()
    {
        currentTutorialBall = "None";
        InitTextDeck();
        InstantiateCards();
    }

    public void BallFirstBall()
    {
        currentTutorialBall = "first";
        DestroyAllPiles();
        InitTextDeck(PitchCondition.Friendly, 1);
        InstantiateCards();
        StartTurn();
    }

    public void BallSecondBall()
    {
        currentTutorialBall = "second";
        DestroyAllPiles();
        InitTextDeck(PitchCondition.Friendly, 2);
        InstantiateCards();
        StartTurn();
    }

    public void BallThirdBall()
    {
        currentTutorialBall = "third";
        DestroyAllPiles();
        InitTextDeck(PitchCondition.Friendly, 3);
        InstantiateCards();
        StartTurn();
    }

    public void BallFourthBall()
    {
        currentTutorialBall = "fourth";
        DestroyAllPiles();
        InitTextDeck(PitchCondition.Friendly, 4);
        InstantiateCardsWithoutActivation();
        StartTurnWithoutTimer();
    }


    public void HighlightShotPanel()
    {
        UIHighlightManager.Instance.HighlightObject(hand.gameObject);
    }

    [ContextMenu("Start Turn")]
    public void StartTurn(bool incrementBalls = true)
    {
        if (CurrntTurn >= ScoreManager_Tutorial.Instance.MaxBalls || ScoreManager_Tutorial.Instance.wickets < 1)
        {
            //Game ended 
            return;
        }
        if (incrementBalls)
            ScoreManager_Tutorial.Instance.UpdateBallsAndOvers(CurrntTurn);
        if (ballerCard != null)
            Destroy(ballerCard);

        if (CurrentBallThrow != null)
            ballerCard = InstantiateBallerCard(CurrentBallThrow);
        else
        {
            Debug.LogError("Current Ball throw is null");
        }
        //BallThrowText.text = CurrentBallThrow.ToString();

        HandCards.Clear();
        // This is where we should start animating in the cards
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }

        Timer_Tutorial.Instance.StartTurnTimer();
        OnTurnStarted?.Invoke();
    }

    public void StartTurnWithoutTimer(bool incrementBalls = true)
    {
        if (CurrntTurn >= ScoreManager_Tutorial.Instance.MaxBalls || ScoreManager_Tutorial.Instance.wickets < 1)
        {
            //Game ended 
            return;
        }
        if (incrementBalls)
            ScoreManager_Tutorial.Instance.UpdateBallsAndOvers(CurrntTurn);
        if (ballerCard != null)
            Destroy(ballerCard);

        if (CurrentBallThrow != null)
            ballerCard = InstantiateBallerCard(CurrentBallThrow);
        else
        {
            Debug.LogError("Current Ball throw is null");
        }
        //BallThrowText.text = CurrentBallThrow.ToString();

        HandCards.Clear();
        // This is where we should start animating in the cards
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }

        // Timer_Tutorial.Instance.StartTurnTimer();
        OnTurnStarted?.Invoke();
    }

    [ContextMenu("End Turn")]
    public void EndTurn(bool incrementBalls = true)
    {
        // Timer.Instance.EndTurnTimer();
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
        //StartTurn(incrementBalls); // Start the next turn
    }
    [ContextMenu("Draw Card")]
    void DrawCard()
    {
        if (DrawPile.Count <= 0)
        {
            DrawPile = new List<AttackCardProps_Tutorial>(DiscardPile);
            DiscardPile.Clear();
        }
        AttackCardProps_Tutorial card = DrawPile[0];
        DrawPile.RemoveAt(0);
        HandCards.Add(card);
        card.gameObject.SetActive(true); // Activate the card when drawn 
        SimpleHandArcManager arcManager = hand.GetComponent<SimpleHandArcManager>();
            if (arcManager != null)
                arcManager.RefreshCardArrangement();
    }

    public void RedrawHand()
    {
        if (currentTutorialBall != "fourth") return;
        currentTutorialBall = "None";
        variableStorage.SetValue("$flipButtonClicked", true);
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
        dialogueRunner.StartDialogue("FlipCardSelection");
    }

    void InstantiateCards()
    {
        DrawPile.Clear();
        foreach (var cardData in Deck)
        {
            AttackCardProps_Tutorial card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps_Tutorial>();
            card.cardData = cardData; // Set the card data
            DrawPile.Add(card);
            card.gameObject.SetActive(true); // Deactivate the card initially
        }
    }
    
    void InstantiateCardsWithoutActivation()
    {
        DrawPile.Clear();
        foreach (var cardData in Deck)
        {
            AttackCardProps_Tutorial card = Instantiate(cardPrefab, hand).GetComponent<AttackCardProps_Tutorial>();
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


    void DestroyAllPiles()
    {
        // if(DrawPile.Count > 0)
        // foreach (AttackCardProps_Tutorial card in DrawPile)
        // {
        //     if(card.gameObject != null)
        //         Destroy(card.gameObject);
        // }

        // if(DiscardPile.Count > 0)
        // foreach (AttackCardProps_Tutorial card in DiscardPile)
        // {
        //     if(card.gameObject != null)
        //         Destroy(card.gameObject);
        // }

        // if(HandCards.Count > 0)
        // foreach (AttackCardProps_Tutorial card in HandCards)
        // {
        //     if(card.gameObject != null)
        //         Destroy(card.gameObject);
        // }

        for (int i = hand.childCount - 1; i >= 0; i--)
        {
            GameObject child = hand.GetChild(i).gameObject;
            Destroy(child);
        }
    }
    

    [ContextMenu("Init Text Deck")]
void InitTextDeck(PitchCondition pitchCondition = PitchCondition.Friendly, int tutorialBallNumber = 0)
{
        Deck.Clear();

        // Instead of always looping through all strategies,
        // pick batting strategies based on tutorialBallNumber
        switch (tutorialBallNumber)
        {
            case 0: // Ball 1 batting strategy set
                    // Create a list of the specific strategies you want for tutorial 0
                BattingStrategy[] tutorial0Strategies = new BattingStrategy[]
                {
                BattingStrategy.CutShotPush,
                BattingStrategy.StraightDriveNormal,
                BattingStrategy.OnDriveAggressive,
                BattingStrategy.PullShotLofted
                };

                foreach (BattingStrategy strategy in tutorial0Strategies)
                {
                    Deck.Add(new AttackCardData(strategy));
                }
                break;

            case 1: // Ball 2 batting strategy set
                BattingStrategy[] tutorial1Strategies = new BattingStrategy[]
                {
                BattingStrategy.Leave,
                BattingStrategy.StraightDriveLofted,
                BattingStrategy.CutShotPush,
                BattingStrategy.SweepNormal
                };

                foreach (BattingStrategy strategy in tutorial1Strategies)
                {
                    Deck.Add(new AttackCardData(strategy));
                }
                break;

            case 2: // Ball 3 batting strategy set
                BattingStrategy[] tutorial2Strategies = new BattingStrategy[]
                {
                BattingStrategy.CutShotPush,
                BattingStrategy.CutShotAggressive,
                BattingStrategy.CutShotNormal,
                BattingStrategy.CutShotLofted
                };

                foreach (BattingStrategy strategy in tutorial2Strategies)
                {
                    Deck.Add(new AttackCardData(strategy));
                }
                break;

            case 3: // fallback for any other tutorialBallNumber
                BattingStrategy[] tutorial3Strategies = new BattingStrategy[]
                {
                BattingStrategy.Leave,
                BattingStrategy.StraightDrivePush,
                BattingStrategy.CoverDriveNormal,
                BattingStrategy.SquareDriveAggressive
                };
                foreach (BattingStrategy strategy in tutorial3Strategies)
                {
                    Deck.Add(new AttackCardData(strategy));
                }
                break;
            
            case 4: // fallback for any other tutorialBallNumber
                BattingStrategy[] tutorial4Strategies = new BattingStrategy[]
                {
                BattingStrategy.Leave,
                BattingStrategy.SweepNormal,
                BattingStrategy.CutShotAggressive,
                BattingStrategy.SweepNormal,
                BattingStrategy.StraightDrivePush,
                BattingStrategy.StraightDriveNormal,
                BattingStrategy.OnDriveAggressive,
                BattingStrategy.SquareDriveAggressive
                };
                foreach (BattingStrategy strategy in tutorial4Strategies)
                {
                    Deck.Add(new AttackCardData(strategy));
                }
                break;
            
}

    // RandomizeDeck();
    BallThrows.Clear();

    // default bowler values
    TypeOfBowler bowlerType = TypeOfBowler.Fast;
    Side bowlerSide = Side.RightArm;

    // create balls based on tutorialBallNumber
    for (int i = 0; i < ScoreManager_Tutorial.Instance.MaxBalls; i++)
    {
        BallThrow ballToAdd;

        switch (tutorialBallNumber)
        {
            case 1: // Ball 1
                ballToAdd = new BallThrow(
                    TypeOfBowler.Fast,
                    Side.RightArm,
                    BallType.Straight,
                    BallLine.MiddleStump,
                    BallLength.FullLength,
                    pitchCondition
                );
                break;

            case 2: // Ball 2
                ballToAdd = new BallThrow(
                    TypeOfBowler.Fast,
                    Side.RightArm,
                    BallType.Straight,
                    BallLine.OutsideOff,
                    BallLength.Short,
                    pitchCondition
                );
                break;

            case 3: // Ball 3
                ballToAdd = new BallThrow(
                    TypeOfBowler.Fast,
                    Side.LeftArm,
                    BallType.OutSwinger,
                    BallLine.OffStump,
                    BallLength.GoodLength,
                    pitchCondition
                );
                break;

            case 4: // Ball 3
                ballToAdd = new BallThrow(
                    TypeOfBowler.Fast,
                    Side.LeftArm,
                    BallType.InSwinger,
                    BallLine.OffStump,
                    BallLength.FullLength,
                    pitchCondition
                );
                break;

            default:
                ballToAdd = ExcelDataSOManager.Instance.outComeCalculator
                    .GetRandomBallThrow(bowlerType, bowlerSide, pitchCondition);
                break;
        }

        BallThrows.Add(ballToAdd);
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
        return redraws < maxRedraws;
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
