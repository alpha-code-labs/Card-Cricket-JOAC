using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardSelectionLogic
{
    // Define shot categories for easier management
    private static readonly List<BattingStrategy> CutShotsPushNormal = new List<BattingStrategy>
    {
        BattingStrategy.CutShotPush,
        BattingStrategy.CutShotNormal
    };
    
    private static readonly List<BattingStrategy> PullShotsPushNormal = new List<BattingStrategy>
    {
        BattingStrategy.PullShotPush,
        BattingStrategy.PullShotNormal
    };

    private static readonly List<BattingStrategy> CutShots = new List<BattingStrategy>
    {
        BattingStrategy.CutShotPush,
        BattingStrategy.CutShotNormal,
        BattingStrategy.CutShotAggressive,
        BattingStrategy.CutShotLofted
    };

    private static readonly List<BattingStrategy> PullShots = new List<BattingStrategy>
    {
        BattingStrategy.PullShotPush,
        BattingStrategy.PullShotNormal,
        BattingStrategy.PullShotAggressive,
        BattingStrategy.PullShotLofted
    };

    private static readonly List<BattingStrategy> CoverDrivePushNormal = new List<BattingStrategy>
    {
        BattingStrategy.CoverDrivePush,
        BattingStrategy.CoverDriveNormal
    };

    private static readonly List<BattingStrategy> SquareDrivePushNormal = new List<BattingStrategy>
    {
        BattingStrategy.SquareDrivePush,
        BattingStrategy.SquareDriveNormal
    };

    private static readonly List<BattingStrategy> StraightDrivePushNormal = new List<BattingStrategy>
    {
        BattingStrategy.StraightDrivePush,
        BattingStrategy.StraightDriveNormal
    };

    private static readonly List<BattingStrategy> LegGlancePushNormal = new List<BattingStrategy>
    {
        BattingStrategy.LegGlancePush,
        BattingStrategy.LegGlanceNormal
    };

    private static readonly List<BattingStrategy> OnDrivePushNormal = new List<BattingStrategy>
    {
        BattingStrategy.OnDrivePush,
        BattingStrategy.OnDriveNormal
    };

    private static readonly List<BattingStrategy> AllShots = System.Enum.GetValues(typeof(BattingStrategy))
        .Cast<BattingStrategy>()
        .ToList();

    public static List<AttackCardData> GetSmartCardSelection(BallThrow ballThrow, int deckSize, float runRate, int currentTurn, int maxBalls, int currentRuns)
    {
        List<AttackCardData> selectedCards = new List<AttackCardData>();

        // Calculate actual run rate
        float actualRunRate = runRate;
        
        Debug.Log("====== SMART CARD SELECTION START ======");
        Debug.Log($"Current Runs: {currentRuns}, Current Turn: {currentTurn}, Calculated Run Rate: {actualRunRate:F2}");
        Debug.Log($"Ball Length: {ballThrow.ballLength}");
        Debug.Log($"Ball Line: {ballThrow.ballLine}");
        Debug.Log($"Deck Size: {deckSize}");

        // Determine which logic to use based on run rate
        if (actualRunRate >= 1.8f)
        {
            Debug.Log($">>> Using HIGH RUN RATE logic (rate: {actualRunRate:F2})");
            selectedCards = GetHighRunRateCards(ballThrow, deckSize);
        }
        else if (actualRunRate >= 1.4f && actualRunRate < 1.8f)
        {
            Debug.Log($">>> Using MEDIUM-HIGH RUN RATE logic (rate: {actualRunRate:F2})");
            selectedCards = GetMediumHighRunRateCards(ballThrow, deckSize);
        }
        else if (actualRunRate >= 1.0f && actualRunRate < 1.4f)
        {
            Debug.Log($">>> Using MEDIUM-LOW RUN RATE logic (rate: {actualRunRate:F2})");
            selectedCards = GetMediumLowRunRateCards(ballThrow, deckSize);
        }
        else if (actualRunRate >= 0.5f && actualRunRate < 1.0f)
        {
            Debug.Log($">>> Using LOW RUN RATE logic (actually HIGH logic, rate: {actualRunRate:F2})");
            selectedCards = GetHighRunRateCards(ballThrow, deckSize);
        }
        else // < 0.5
        {
            Debug.Log($">>> Using VERY LOW RUN RATE logic (actually MEDIUM-HIGH logic, rate: {actualRunRate:F2})");
            selectedCards = GetMediumHighRunRateCards(ballThrow, deckSize);
        }
        
        Debug.Log($"Final Cards Selected: {selectedCards.Count} cards");
        foreach (var card in selectedCards)
        {
            Debug.Log($"  - {card.excelBattinStrategy}");
        }
        Debug.Log("====== SMART CARD SELECTION END ======");
        
        return selectedCards;
    }

    private static List<AttackCardData> GetHighRunRateCards(BallThrow ballThrow, int deckSize)
    {
        List<AttackCardData> cards = new List<AttackCardData>();
        List<BattingStrategy> availableShots = new List<BattingStrategy>(AllShots);
        List<BattingStrategy> usedShots = new List<BattingStrategy>();

        // For high run rate (>= 1.8), serve all shots but ensure key shots for short balls
        if (ballThrow.ballLength == BallLength.Short)
        {
            if (ballThrow.ballLine == BallLine.MiddleStump)
            {
                // Serve at least one cut shot (any kind) or pull shot (any kind)
                var cutAndPullShots = CutShots.Concat(PullShots).ToList();
                AddUniqueRandomFromList(cards, cutAndPullShots, 1, usedShots, availableShots);
            }
            else if (IsOffSideLine(ballThrow.ballLine))
            {
                // Serve at least one cut shot (any kind)
                AddUniqueRandomFromList(cards, CutShots, 1, usedShots, availableShots);
            }
            else if (IsLegSideLine(ballThrow.ballLine))
            {
                // Serve at least one pull shot (any kind)
                AddUniqueRandomFromList(cards, PullShots, 1, usedShots, availableShots);
            }
        }

        // Fill rest with any shots but not repeating the same shot
        FillWithRandomShots(cards, availableShots, deckSize, usedShots);
        
        return cards;
    }

    private static List<AttackCardData> GetMediumHighRunRateCards(BallThrow ballThrow, int deckSize)
    {
        Debug.Log("[MEDIUM-HIGH] Starting GetMediumHighRunRateCards");
        Debug.Log($"[MEDIUM-HIGH] Ball Length: {ballThrow.ballLength}, Ball Line: {ballThrow.ballLine}");
        
        List<AttackCardData> cards = new List<AttackCardData>();
        List<BattingStrategy> availableShots = new List<BattingStrategy>(AllShots);
        List<BattingStrategy> usedShots = new List<BattingStrategy>();

        if (ballThrow.ballLength == BallLength.Short)
        {
            Debug.Log("[MEDIUM-HIGH] Processing SHORT BALL");
            
            List<BattingStrategy> guaranteedPool = new List<BattingStrategy>();
            List<BattingStrategy> loftedToRemove = new List<BattingStrategy>();
            int guaranteedCount = GetGuaranteedCardsCount(deckSize, false);
            Debug.Log($"[MEDIUM-HIGH] Guaranteed cards needed: {guaranteedCount}");

            // Determine pool and exclusions based on ball line
            if (ballThrow.ballLine == BallLine.MiddleStump)
            {
                Debug.Log("[MEDIUM-HIGH] Short ball at MIDDLE STUMP - using Pull + Cut pool");
                // Middle stump: both pull and cut shots (push or normal)
                guaranteedPool.AddRange(PullShotsPushNormal);
                guaranteedPool.AddRange(CutShotsPushNormal);
                
                // Remove both lofted variants
                loftedToRemove.Add(BattingStrategy.PullShotLofted);
                loftedToRemove.Add(BattingStrategy.CutShotLofted);
            }
            else if (IsOffSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-HIGH] Short ball at OFF SIDE - using Cut pool ONLY");
                // Off side: only cut shots (push or normal)
                guaranteedPool.AddRange(CutShotsPushNormal);
                
                // Remove only cut lofted
                loftedToRemove.Add(BattingStrategy.CutShotLofted);
            }
            else if (IsLegSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-HIGH] Short ball at LEG SIDE - using Pull pool ONLY");
                // Leg side: only pull shots (push or normal)
                guaranteedPool.AddRange(PullShotsPushNormal);
                
                // Remove only pull lofted
                loftedToRemove.Add(BattingStrategy.PullShotLofted);
            }
            
            Debug.Log($"[MEDIUM-HIGH] Guaranteed pool size: {guaranteedPool.Count}");
            
            // Remove lofted shots from available
            foreach (var shot in loftedToRemove)
            {
                availableShots.Remove(shot);
            }
            Debug.Log($"[MEDIUM-HIGH] Removed {loftedToRemove.Count} lofted shots");
            
            // Add guaranteed cards without repetition
            AddUniqueRandomFromList(cards, guaranteedPool, guaranteedCount, usedShots, availableShots);
            Debug.Log($"[MEDIUM-HIGH] Cards after guaranteed: {cards.Count}");

            // Fill rest with any shot except the removed lofted shots
            FillWithRandomShots(cards, availableShots, deckSize, usedShots);
            Debug.Log($"[MEDIUM-HIGH] Final card count: {cards.Count}");
        }
        else if (ballThrow.ballLength == BallLength.FullLength || ballThrow.ballLength == BallLength.GoodLength || ballThrow.ballLength == BallLength.Yorker)
        {
            Debug.Log("[MEDIUM-HIGH] Processing FULL/GOOD/YORKER LENGTH");
            
            List<BattingStrategy> guaranteedPool = new List<BattingStrategy>();
            List<BattingStrategy> loftedToRemove = new List<BattingStrategy>();
            int guaranteedCount = GetGuaranteedCardsCount(deckSize, false);
            Debug.Log($"[MEDIUM-HIGH] Guaranteed cards needed: {guaranteedCount}");

            // Check if it's leg side FIRST
            bool isLegSide = IsLegSideLine(ballThrow.ballLine);
            bool isOffSide = IsOffSideLine(ballThrow.ballLine);
            
            Debug.Log($"[MEDIUM-HIGH] Is Leg Side: {isLegSide}, Is Off Side: {isOffSide}");

            if (ballThrow.ballLine == BallLine.MiddleStump)
            {
                Debug.Log("[MEDIUM-HIGH] Ball line is MIDDLE STUMP");
                guaranteedPool.AddRange(CoverDrivePushNormal);
                guaranteedPool.AddRange(SquareDrivePushNormal);
                guaranteedPool.AddRange(StraightDrivePushNormal);
                guaranteedPool.AddRange(LegGlancePushNormal);
                guaranteedPool.AddRange(OnDrivePushNormal);
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.CoverDriveLofted,
                    BattingStrategy.SquareDriveLofted,
                    BattingStrategy.StraightDriveLofted,
                    BattingStrategy.LegGlanceLofted,
                    BattingStrategy.OnDriveLofted
                };
            }
            else if (isOffSide)
            {
                Debug.Log("[MEDIUM-HIGH] Ball line is OFF SIDE");
                guaranteedPool.AddRange(CoverDrivePushNormal);
                guaranteedPool.AddRange(SquareDrivePushNormal);
                guaranteedPool.AddRange(StraightDrivePushNormal);
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.CoverDriveLofted,
                    BattingStrategy.SquareDriveLofted,
                    BattingStrategy.StraightDriveLofted
                };
            }
            else if (isLegSide)
            {
                Debug.Log("[MEDIUM-HIGH] Ball line is LEG SIDE");
                Debug.Log($"[MEDIUM-HIGH] OnDrivePushNormal count: {OnDrivePushNormal.Count}");
                Debug.Log($"[MEDIUM-HIGH] LegGlancePushNormal count: {LegGlancePushNormal.Count}");
                
                guaranteedPool.AddRange(OnDrivePushNormal);
                guaranteedPool.AddRange(LegGlancePushNormal);
                
                Debug.Log($"[MEDIUM-HIGH] Guaranteed pool size after adding: {guaranteedPool.Count}");
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.OnDriveLofted,
                    BattingStrategy.LegGlanceLofted
                };
            }
            else
            {
                Debug.LogWarning($"[MEDIUM-HIGH] WARNING: Ball line {ballThrow.ballLine} did not match any condition!");
            }

            // Remove lofted shots from available
            foreach (var shot in loftedToRemove)
            {
                availableShots.Remove(shot);
            }
            Debug.Log($"[MEDIUM-HIGH] Removed {loftedToRemove.Count} lofted shots");

            // Add guaranteed cards without repetition
            Debug.Log($"[MEDIUM-HIGH] Adding {guaranteedCount} guaranteed cards from pool of {guaranteedPool.Count}");
            AddUniqueRandomFromList(cards, guaranteedPool, guaranteedCount, usedShots, availableShots);
            
            Debug.Log($"[MEDIUM-HIGH] Cards after guaranteed: {cards.Count}");

            // Fill remaining slots
            FillWithRandomShots(cards, availableShots, deckSize, usedShots);
            
            Debug.Log($"[MEDIUM-HIGH] Final card count: {cards.Count}");
        }
        else
        {
            Debug.LogWarning($"[MEDIUM-HIGH] WARNING: Ball length {ballThrow.ballLength} not handled!");
        }

        return cards;
    }

    private static List<AttackCardData> GetMediumLowRunRateCards(BallThrow ballThrow, int deckSize)
    {
        Debug.Log("[MEDIUM-LOW] Starting GetMediumLowRunRateCards");
        Debug.Log($"[MEDIUM-LOW] Ball Length: {ballThrow.ballLength}, Ball Line: {ballThrow.ballLine}");
        
        // Same logic as medium-high but with increased guaranteed card counts
        List<AttackCardData> cards = new List<AttackCardData>();
        List<BattingStrategy> availableShots = new List<BattingStrategy>(AllShots);
        List<BattingStrategy> usedShots = new List<BattingStrategy>();

        if (ballThrow.ballLength == BallLength.Short)
        {
            Debug.Log("[MEDIUM-LOW] Processing SHORT BALL");
            
            List<BattingStrategy> guaranteedPool = new List<BattingStrategy>();
            List<BattingStrategy> loftedToRemove = new List<BattingStrategy>();
            int guaranteedCount = GetGuaranteedCardsCount(deckSize, true);  // true for lower run rate
            Debug.Log($"[MEDIUM-LOW] Guaranteed cards needed: {guaranteedCount}");

            // Determine pool and exclusions based on ball line
            if (ballThrow.ballLine == BallLine.MiddleStump)
            {
                Debug.Log("[MEDIUM-LOW] Short ball at MIDDLE STUMP - using Pull + Cut pool");
                // Middle stump: both pull and cut shots (push or normal)
                guaranteedPool.AddRange(PullShotsPushNormal);
                guaranteedPool.AddRange(CutShotsPushNormal);
                
                // Remove both lofted variants
                loftedToRemove.Add(BattingStrategy.PullShotLofted);
                loftedToRemove.Add(BattingStrategy.CutShotLofted);
            }
            else if (IsOffSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-LOW] Short ball at OFF SIDE - using Cut pool ONLY");
                // Off side: only cut shots (push or normal)
                guaranteedPool.AddRange(CutShotsPushNormal);
                
                // Remove only cut lofted
                loftedToRemove.Add(BattingStrategy.CutShotLofted);
            }
            else if (IsLegSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-LOW] Short ball at LEG SIDE - using Pull pool ONLY");
                // Leg side: only pull shots (push or normal)
                guaranteedPool.AddRange(PullShotsPushNormal);
                
                // Remove only pull lofted
                loftedToRemove.Add(BattingStrategy.PullShotLofted);
            }
            
            Debug.Log($"[MEDIUM-LOW] Guaranteed pool size: {guaranteedPool.Count}");
            
            // Remove lofted shots from available
            foreach (var shot in loftedToRemove)
            {
                availableShots.Remove(shot);
            }
            
            AddUniqueRandomFromList(cards, guaranteedPool, guaranteedCount, usedShots, availableShots);
            FillWithRandomShots(cards, availableShots, deckSize, usedShots);
        }
        else if (ballThrow.ballLength == BallLength.FullLength || ballThrow.ballLength == BallLength.GoodLength || ballThrow.ballLength == BallLength.Yorker)
        {
            Debug.Log("[MEDIUM-LOW] Processing FULL/GOOD/YORKER LENGTH");
            
            List<BattingStrategy> guaranteedPool = new List<BattingStrategy>();
            List<BattingStrategy> loftedToRemove = new List<BattingStrategy>();
            int guaranteedCount = GetGuaranteedCardsCount(deckSize, true);  // true for lower run rate
            Debug.Log($"[MEDIUM-LOW] Guaranteed cards needed: {guaranteedCount}");

            if (ballThrow.ballLine == BallLine.MiddleStump)
            {
                Debug.Log("[MEDIUM-LOW] Ball line is MIDDLE STUMP");
                guaranteedPool.AddRange(CoverDrivePushNormal);
                guaranteedPool.AddRange(SquareDrivePushNormal);
                guaranteedPool.AddRange(StraightDrivePushNormal);
                guaranteedPool.AddRange(LegGlancePushNormal);
                guaranteedPool.AddRange(OnDrivePushNormal);
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.CoverDriveLofted,
                    BattingStrategy.SquareDriveLofted,
                    BattingStrategy.StraightDriveLofted,
                    BattingStrategy.LegGlanceLofted,
                    BattingStrategy.OnDriveLofted
                };
            }
            else if (IsOffSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-LOW] Ball line is OFF SIDE");
                guaranteedPool.AddRange(CoverDrivePushNormal);
                guaranteedPool.AddRange(SquareDrivePushNormal);
                guaranteedPool.AddRange(StraightDrivePushNormal);
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.CoverDriveLofted,
                    BattingStrategy.SquareDriveLofted,
                    BattingStrategy.StraightDriveLofted
                };
            }
            else if (IsLegSideLine(ballThrow.ballLine))
            {
                Debug.Log("[MEDIUM-LOW] Ball line is LEG SIDE");
                guaranteedPool.AddRange(OnDrivePushNormal);
                guaranteedPool.AddRange(LegGlancePushNormal);
                
                loftedToRemove = new List<BattingStrategy>
                {
                    BattingStrategy.OnDriveLofted,
                    BattingStrategy.LegGlanceLofted
                };
            }

            foreach (var shot in loftedToRemove)
            {
                availableShots.Remove(shot);
            }

            AddUniqueRandomFromList(cards, guaranteedPool, guaranteedCount, usedShots, availableShots);
            FillWithRandomShots(cards, availableShots, deckSize, usedShots);
        }

        return cards;
    }

    private static int GetGuaranteedCardsCount(int deckSize, bool isLowerRunRate)
    {
        if (isLowerRunRate)
        {
            // For run rate 1.0-1.4
            return deckSize switch
            {
                4 => 3,
                5 => 3,
                6 => 4,
                _ => 2
            };
        }
        else
        {
            // For run rate 1.4-1.8 (and also < 0.5)
            return deckSize switch
            {
                4 => 2,
                5 => 2,
                6 => 3,
                _ => 2
            };
        }
    }

    private static bool IsOffSideLine(BallLine line)
    {
        bool result = line == BallLine.OffStump || line == BallLine.OutsideOff || line == BallLine.WayOutsideOff;
        Debug.Log($"[IsOffSideLine] Line: {line}, Result: {result}");
        return result;
    }

    private static bool IsLegSideLine(BallLine line)
    {
        bool result = line == BallLine.LegStump || line == BallLine.DowntheLeg || line == BallLine.WayDowntheLeg;
        Debug.Log($"[IsLegSideLine] Line: {line}, Result: {result}");
        return result;
    }

    private static void AddUniqueRandomFromList(List<AttackCardData> cards, List<BattingStrategy> shotList, int count, List<BattingStrategy> usedShots, List<BattingStrategy> availableShots)
    {
        Debug.Log($"[AddUnique] Need to add {count} cards from pool of {shotList.Count}");
        
        var validShots = shotList.Where(s => !usedShots.Contains(s) && availableShots.Contains(s)).ToList();
        Debug.Log($"[AddUnique] Valid shots after filtering: {validShots.Count}");
        
        int addedCount = 0;
        for (int i = 0; i < count && validShots.Count > 0; i++)
        {
            var shot = validShots[Random.Range(0, validShots.Count)];
            Debug.Log($"[AddUnique] Adding: {shot}");
            cards.Add(new AttackCardData(shot));
            usedShots.Add(shot);
            availableShots.Remove(shot);
            validShots.Remove(shot);
            addedCount++;
        }
        
        Debug.Log($"[AddUnique] Added {addedCount} cards");
    }

    private static void FillWithRandomShots(List<AttackCardData> cards, List<BattingStrategy> availableShots, int targetCount, List<BattingStrategy> excludeShots = null)
    {
        Debug.Log($"[FillRandom] Current: {cards.Count}, Target: {targetCount}");
        
        // Clone available shots to avoid modifying the original
        var validShots = availableShots.ToList();
        
        // Remove already used shots
        if (excludeShots != null)
        {
            validShots.RemoveAll(s => excludeShots.Contains(s));
        }

        // Fill remaining slots
        int startCount = cards.Count;
        while (cards.Count < targetCount && validShots.Count > 0)
        {
            var shot = validShots[Random.Range(0, validShots.Count)];
            Debug.Log($"[FillRandom] Adding: {shot}");
            cards.Add(new AttackCardData(shot));
            validShots.Remove(shot);
        }

        // If we still need cards and have exhausted unique shots, add defensive shots
        if (cards.Count < targetCount)
        {
            Debug.LogWarning($"[FillRandom] Exhausted unique shots, adding defensive");
            while (cards.Count < targetCount)
            {
                cards.Add(new AttackCardData(BattingStrategy.ForwardDefense));
            }
        }

        // Shuffle the cards to randomize their order in hand
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
        
        Debug.Log($"[FillRandom] Added {cards.Count - startCount} fill cards");
    }
}