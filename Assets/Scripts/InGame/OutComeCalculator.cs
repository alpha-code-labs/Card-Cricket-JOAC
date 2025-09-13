using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using OfficeOpenXml; // You'll need EPPlus package for Excel reading
#endif

[CreateAssetMenu(fileName = "OutComeCalculator", menuName = "Cricket/OutCome Calculator")]
public class OutComeCalculator : ScriptableObject
{
    [Header("Excel Source")]
    [Tooltip("Path to Excel file relative to Assets folder")]
    public string excelFilePath = "Data/CricketOutcomes.xlsx";

    [Header("Parsed Data")]
    [SerializeField, HideInInspector] private List<OutcomeEntry> outcomeEntries = new List<OutcomeEntry>();

    // Runtime lookup dictionary
    private Dictionary<OutcomeKey, OutCome> outcomeLookup;

    public void BuildLookupDictionary()
    {
        Debug.Log("Building outcome lookup dictionary...");
        outcomeLookup = new Dictionary<OutcomeKey, OutCome>();
        foreach (var entry in outcomeEntries)
        {
            var key = new OutcomeKey(entry);
            if (!outcomeLookup.ContainsKey(key))
            {
                outcomeLookup[key] = entry.outcome;
            }
        }
    }
    // Overloaded method with bowler details
    public OutCome CalculateOutcome(BattingStrategy battingStrategy, BallThrow ballThrow,
                                    BattingTiming timing)
    {
        if (outcomeLookup == null || outcomeLookup.Count == 0)
        {
            BuildLookupDictionary();
        }

        var key = new OutcomeKey
        {
            typeOfBowler = ballThrow.bowlerType,
            side = ballThrow.bowlerSide,
            typeOfBall = ballThrow.ballType,
            lineOfBall = ballThrow.ballLine,
            lengthOfBall = ballThrow.ballLength,
            timing = timing,
            shotSelected = battingStrategy
        };
        string debugKey = $"{key.typeOfBowler}, {key.side}, {key.typeOfBall}, {key.lineOfBall}, {key.lengthOfBall}, {key.shotSelected}";
        if (outcomeLookup.TryGetValue(key, out OutCome outcome))
        {
            Debug.Log($"Outcome found: {outcome} for {battingStrategy} with keys {debugKey}");
            return outcome;
        }
        Debug.LogWarning($"No outcome found for: {battingStrategy} with {timing} timing vs Key: {debugKey}");

        return OutCome.NoRun; // Default fallback
    }

    public BallThrow GetRandomBallThrow(TypeOfBowler bowlerType, Side bowlerSide)
    {
        if (outcomeLookup == null || outcomeLookup.Count == 0)
        {
            BuildLookupDictionary();
        }

        // Get all keys that match the specified bowler type and side
        var matchingKeys = outcomeLookup.Keys.Where(key =>
            key.typeOfBowler == bowlerType &&
            key.side == bowlerSide).ToList();

        if (matchingKeys.Count == 0)
        {
            Debug.LogWarning($"No matching keys found for bowler type: {bowlerType}, side: {bowlerSide}. Using random generation.");
            // Fallback to random generation
            var random = new System.Random();
            return new BallThrow
            {
                bowlerType = bowlerType,
                bowlerSide = bowlerSide,
                ballType = (BallType)random.Next(Enum.GetValues(typeof(BallType)).Length),
                ballLine = (BallLine)random.Next(Enum.GetValues(typeof(BallLine)).Length),
                ballLength = (BallLength)random.Next(Enum.GetValues(typeof(BallLength)).Length)
            };
        }

        // Select a random key from the matching keys
        var random2 = new System.Random();
        var selectedKey = matchingKeys[random2.Next(matchingKeys.Count)];

        // Create BallThrow from the selected key
        var ballThrow = new BallThrow
        {
            bowlerType = selectedKey.typeOfBowler,
            bowlerSide = selectedKey.side,
            ballType = selectedKey.typeOfBall,
            ballLine = selectedKey.lineOfBall,
            ballLength = selectedKey.lengthOfBall
        };

        return ballThrow;
    }

#if UNITY_EDITOR
    List<string> exceptions;
    [ContextMenu("Load From Excel")]
    public void LoadFromExcel()
    {
        Debug.Log("Loading outcome data from Excel...");
        exceptions = new List<string>();
        string fullPath = Path.Combine(Application.dataPath, excelFilePath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Excel file not found at: {fullPath}");
            return;
        }

        try
        {
            outcomeEntries.Clear();

            // Set up EPPlus license context for non-commercial use
            ExcelPackage.License.SetNonCommercialPersonal("Unity Game Developer");

            using (var package = new ExcelPackage(new FileInfo(fullPath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    Debug.LogError("Excel file appears to be empty or has no data rows");
                    return;
                }

                // Parse each row (starting from row 2 to skip headers)
                for (int row = 2; row <= rowCount; row++)
                {
                    var entry = ParseRow(worksheet, row);
                    if (entry != null)
                    {
                        outcomeEntries.Add(entry);
                    }
                }
            }
            string exceptionsText = "Exceptions:\n:";
            foreach (var ex in exceptions)
            {
                exceptionsText += ex + "\n";
            }

            Debug.Log($"Successfully loaded {outcomeEntries.Count} outcome entries from Excel with {exceptions.Count} Exceptions!\n" + exceptionsText);
            CheckForExtraEnums();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading Excel file: {e.Message}\n{e.StackTrace}");
        }
    }
    private void CheckForExtraEnums()
    {
        // Track which enum values were actually used
        var usedBowlerTypes = new HashSet<TypeOfBowler>();
        var usedSides = new HashSet<Side>();
        var usedBallTypes = new HashSet<BallType>();
        var usedBallLines = new HashSet<BallLine>();
        var usedBallLengths = new HashSet<BallLength>();
        var usedTimings = new HashSet<BattingTiming>();
        var usedStrategies = new HashSet<BattingStrategy>();
        var usedOutcomes = new HashSet<OutCome>();

        // Collect all used enum values from parsed entries
        foreach (var entry in outcomeEntries)
        {
            usedBowlerTypes.Add(entry.typeOfBowler);
            usedSides.Add(entry.side);
            usedBallTypes.Add(entry.typeOfBall);
            usedBallLines.Add(entry.lineOfBall);
            usedBallLengths.Add(entry.lengthOfBall);
            usedTimings.Add(entry.timing);
            usedStrategies.Add(entry.shotSelected);
            usedOutcomes.Add(entry.outcome);
        }

        // Check for unused enum values and log them
        CheckUnusedEnumValues<TypeOfBowler>(usedBowlerTypes, "Type of Bowler");
        CheckUnusedEnumValues<Side>(usedSides, "Side");
        CheckUnusedEnumValues<BallType>(usedBallTypes, "Ball Type");
        CheckUnusedEnumValues<BallLine>(usedBallLines, "Ball Line");
        CheckUnusedEnumValues<BallLength>(usedBallLengths, "Ball Length");
        CheckUnusedEnumValues<BattingTiming>(usedTimings, "Batting Timing");
        CheckUnusedEnumValues<BattingStrategy>(usedStrategies, "Batting Strategy");
        CheckUnusedEnumValues<OutCome>(usedOutcomes, "Outcome");
    }

    private void CheckUnusedEnumValues<T>(HashSet<T> usedValues, string enumName) where T : struct, Enum
    {
        var allValues = (T[])Enum.GetValues(typeof(T));
        var unusedValues = new List<T>();

        foreach (T value in allValues)
        {
            if (!usedValues.Contains(value))
            {
                unusedValues.Add(value);
            }
        }

        if (unusedValues.Count > 0)
        {
            string unusedList = string.Join(", ", unusedValues);
            Debug.LogWarning($"Unused {enumName} enum values: {unusedList}");
        }
        // else Debug.Log($"All {enumName} enum values are used in the data.");

    }

    private OutcomeEntry ParseRow(ExcelWorksheet worksheet, int row)
    {
        try
        {
            // Column mapping based on your Excel structure:
            // 1: Type of Bowler, 2: Side, 3: Type of Ball, 4: Line of Ball, 
            // 5: Length of Ball, 6: Timing, 7: Shot Selected, 8: Outcome, 
            // 9: Special Outcome, 10: Comments

            var entry = new OutcomeEntry
            {
                typeOfBowler = ParseEnum<TypeOfBowler>(worksheet.Cells[row, 1].Value?.ToString()),
                side = ParseEnum<Side>(worksheet.Cells[row, 2].Value?.ToString()),
                typeOfBall = ParseEnum<BallType>(worksheet.Cells[row, 3].Value?.ToString()),
                lineOfBall = ParseEnum<BallLine>(worksheet.Cells[row, 4].Value?.ToString()),
                lengthOfBall = ParseEnum<BallLength>(worksheet.Cells[row, 5].Value?.ToString()),
                // timing = ParseEnum<BattingTiming>(worksheet.Cells[row, 6].Value?.ToString()),
                shotSelected = ParseEnum<BattingStrategy>(worksheet.Cells[row, 6].Value?.ToString()),
            };

            // Handle outcome with special cases
            string outcomeStr = worksheet.Cells[row, 7].Value?.ToString();
            string specialOutcome = worksheet.Cells[row, 8].Value?.ToString();

            // Check for Wide Ball special case
            if (outcomeStr == "No Run" && specialOutcome == "Wide Ball")
            {
                entry.outcome = OutCome.NoRunWideBall;
            }
            else
            {
                entry.outcome = ParseEnum<OutCome>(outcomeStr);
            }

            return entry;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to parse row {row}: {e.Message}");
            if (exceptions.Contains(e.Message) == false)
                exceptions.Add(e.Message);
            return null;
        }
    }

    private T ParseEnum<T>(string value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"Empty value for enum {typeof(T).Name}");
        }

        // Special handling for OutCome enum
        if (typeof(T) == typeof(OutCome))
        {
            string trimmedValue = value.Trim();

            // Handle specific OutCome cases
            switch (trimmedValue)
            {
                case "1 Run":
                    return (T)(object)OutCome.OneRuns;
                case "2 Runs":
                    return (T)(object)OutCome.TwoRuns;
                case "3 Runs":
                    return (T)(object)OutCome.ThreeRuns;
                case "4 Runs":
                    return (T)(object)OutCome.FourRuns;
                case "6 Runs":
                    return (T)(object)OutCome.SixRuns;
                case "No Run":
                    return (T)(object)OutCome.NoRun;
                case "Out":
                    return (T)(object)OutCome.Out;
            }
        }

        // Clean the string for enum parsing (existing logic)
        string cleanValue = value.Replace(" ", "").Replace("_", "").Replace("-", "");

        if (Enum.TryParse<T>(cleanValue, true, out T result))
        {
            return result;
        }

        throw new ArgumentException($"Failed to parse '{value}' as {typeof(T).Name}");
    }

    [ContextMenu("Clear Data")]
    public void ClearData()
    {
        outcomeEntries.Clear();
        outcomeLookup?.Clear();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("Cleared all outcome data");
    }

    [ContextMenu("Debug: Show Entry Count")]
    public void ShowEntryCount()
    {
        Debug.Log($"Total entries stored: {outcomeEntries.Count}");
        if (outcomeEntries.Count > 0)
        {
            Debug.Log($"First entry: {outcomeEntries[0].shotSelected} vs {outcomeEntries[0].typeOfBall} = {outcomeEntries[0].outcome}");
        }
    }
#endif
}

// ===== SUPPORTING CLASSES AND ENUMS =====

[Serializable]
public class OutcomeEntry
{
    public TypeOfBowler typeOfBowler;
    public Side side;
    public BallType typeOfBall;
    public BallLine lineOfBall;
    public BallLength lengthOfBall;
    public BattingTiming timing;
    public BattingStrategy shotSelected;
    public OutCome outcome;
}

[Serializable]
public class OutcomeKey : IEquatable<OutcomeKey>
{
    public TypeOfBowler typeOfBowler;
    public Side side;
    public BallType typeOfBall;
    public BallLine lineOfBall;
    public BallLength lengthOfBall;
    public BattingTiming timing;
    public BattingStrategy shotSelected;

    public OutcomeKey() { }

    public OutcomeKey(OutcomeEntry entry)
    {
        typeOfBowler = entry.typeOfBowler;
        side = entry.side;
        typeOfBall = entry.typeOfBall;
        lineOfBall = entry.lineOfBall;
        lengthOfBall = entry.lengthOfBall;
        timing = entry.timing;
        shotSelected = entry.shotSelected;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + typeOfBowler.GetHashCode();
            hash = hash * 23 + side.GetHashCode();
            hash = hash * 23 + typeOfBall.GetHashCode();
            hash = hash * 23 + lineOfBall.GetHashCode();
            hash = hash * 23 + lengthOfBall.GetHashCode();
            hash = hash * 23 + timing.GetHashCode();
            hash = hash * 23 + shotSelected.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as OutcomeKey);
    }

    public bool Equals(OutcomeKey other)
    {
        if (other == null) return false;

        return typeOfBowler == other.typeOfBowler &&
               side == other.side &&
               typeOfBall == other.typeOfBall &&
               lineOfBall == other.lineOfBall &&
               lengthOfBall == other.lengthOfBall &&
               timing == other.timing &&
               shotSelected == other.shotSelected;
    }
}
