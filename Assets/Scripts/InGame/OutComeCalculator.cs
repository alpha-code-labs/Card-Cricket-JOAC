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
    [Header("Excel Sources")]
    [Tooltip("Path to Friendly pitch Excel file relative to Assets folder")]
    public string friendlyExcelFilePath = "Data/CricketOutcomes_Friendly.xlsx";
    
    [Tooltip("Path to Hostile pitch Excel file relative to Assets folder")]
    public string hostileExcelFilePath = "Data/CricketOutcomes_Hostile.xlsx";

    [Header("Parsed Data")]
    [SerializeField, HideInInspector] private List<OutcomeEntry> friendlyOutcomeEntries = new List<OutcomeEntry>();
    [SerializeField, HideInInspector] private List<OutcomeEntry> hostileOutcomeEntries = new List<OutcomeEntry>();

    // Runtime lookup dictionaries for each pitch condition
    private Dictionary<OutcomeKey, OutCome> friendlyOutcomeLookup;
    private Dictionary<OutcomeKey, OutCome> hostileOutcomeLookup;

    public void BuildLookupDictionary()
    {
        Debug.Log("Building outcome lookup dictionaries...");

        Debug.Log($"Friendly entries count: {friendlyOutcomeEntries.Count}");
        Debug.Log($"Hostile entries count: {hostileOutcomeEntries.Count}");

        // Build friendly lookup
        friendlyOutcomeLookup = new Dictionary<OutcomeKey, OutCome>();
        int friendlyCount = 0;
        int hostileCount = 0;

        foreach (var entry in friendlyOutcomeEntries)
        {
            var key = new OutcomeKey(entry);
            if (!friendlyOutcomeLookup.ContainsKey(key))
            {
                friendlyOutcomeLookup[key] = entry.outcome;
                friendlyCount++;
            }
        }

        // Build hostile lookup
        hostileOutcomeLookup = new Dictionary<OutcomeKey, OutCome>();
        foreach (var entry in hostileOutcomeEntries)
        {
            var key = new OutcomeKey(entry);
            if (!hostileOutcomeLookup.ContainsKey(key))
            {
                hostileOutcomeLookup[key] = entry.outcome;
                hostileCount++;
            }
        }
        
        Debug.Log($"Built friendly outcome lookup with {friendlyOutcomeLookup.Count} entries and hostile outcome lookup with {hostileOutcomeLookup.Count} entries.");
    }

    // Overloaded method with bowler details
    public OutCome CalculateOutcome(BattingStrategy battingStrategy, BallThrow ballThrow, BattingTiming timing, PitchCondition pitchCondition)
    {
        // Select the appropriate lookup based on pitch condition
        Dictionary<OutcomeKey, OutCome> selectedLookup = pitchCondition == PitchCondition.Friendly 
            ? friendlyOutcomeLookup 
            : hostileOutcomeLookup;
        
        if (selectedLookup == null || selectedLookup.Count == 0)
        {
            BuildLookupDictionary();
            selectedLookup = pitchCondition == PitchCondition.Friendly 
                ? friendlyOutcomeLookup 
                : hostileOutcomeLookup;
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
        if (selectedLookup.TryGetValue(key, out OutCome outcome))
        {
            Debug.Log($"Outcome found: {outcome} for {battingStrategy} with keys {debugKey} on {pitchCondition} pitch");
            return outcome;
        }
        Debug.LogWarning($"No outcome found for: {battingStrategy} with {timing} timing vs Key: {debugKey} on {pitchCondition} pitch");

        return OutCome.NoRun;
    }
    
    //original method for backward compatibility
    public OutCome CalculateOutcome(BattingStrategy battingStrategy, BallThrow ballThrow, BattingTiming timing)
    {
        // Default to friendly pitch for backward compatibility
        return CalculateOutcome(battingStrategy, ballThrow, timing, PitchCondition.Friendly);
    }
    public BallThrow GetRandomBallThrow(TypeOfBowler bowlerType, Side bowlerSide, PitchCondition pitchCondition = PitchCondition.Friendly)
    {
        Dictionary<OutcomeKey, OutCome> selectedLookup = pitchCondition == PitchCondition.Friendly 
            ? friendlyOutcomeLookup 
            : hostileOutcomeLookup;
            
        if (selectedLookup == null || selectedLookup.Count == 0)
        {
            BuildLookupDictionary();
            selectedLookup = pitchCondition == PitchCondition.Friendly 
                ? friendlyOutcomeLookup 
                : hostileOutcomeLookup;
        }

        var matchingKeys = selectedLookup.Keys.Where(key =>
            key.typeOfBowler == bowlerType &&
            key.side == bowlerSide).ToList();

        if (matchingKeys.Count == 0)
        {
            Debug.LogWarning($"No matching keys found for bowler type: {bowlerType}, side: {bowlerSide}. Using random generation.");
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

        var random2 = new System.Random();
        var selectedKey = matchingKeys[random2.Next(matchingKeys.Count)];

        var ballThrow = new BallThrow
        {
            bowlerType = selectedKey.typeOfBowler,
            bowlerSide = selectedKey.side,
            ballType = selectedKey.typeOfBall,
            ballLine = selectedKey.lineOfBall,
            ballLength = selectedKey.lengthOfBall,
            pitchCondition = pitchCondition
        };

        return ballThrow;
    }

#if UNITY_EDITOR
    List<string> exceptions;
    
    [ContextMenu("Load Friendly Excel")]
    public void LoadFriendlyExcel()
    {
        LoadFromExcel(friendlyExcelFilePath, friendlyOutcomeEntries, "Friendly");
    }
    
    [ContextMenu("Load Hostile Excel")]
    public void LoadHostileExcel()
    {
        LoadFromExcel(hostileExcelFilePath, hostileOutcomeEntries, "Hostile");
    }
    
    [ContextMenu("Load Both Excel Files")]
    public void LoadBothExcelFiles()
    {
        LoadFriendlyExcel();
        LoadHostileExcel();
    }
    
    private void LoadFromExcel(string excelPath, List<OutcomeEntry> targetList, string pitchType)
    {
        // ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        ExcelPackage.License.SetNonCommercialPersonal("Alpha Code Labs");
        Debug.Log($"Loading {pitchType} outcome data from Excel...");
        exceptions = new List<string>();
        string fullPath = Path.Combine(Application.dataPath, excelPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Excel file not found at: {fullPath}");
            return;
        }

        int sheetIndex = pitchType == "Friendly" ? 3 : 2;

        try
        {
            targetList.Clear();
            using (var package = new ExcelPackage(new FileInfo(fullPath)))
            {
                var worksheet = package.Workbook.Worksheets[sheetIndex];
                int rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    Debug.LogError("Excel file appears to be empty or has no data rows");
                    return;
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var entry = ParseRow(worksheet, row);
                    if (entry != null)
                    {
                        targetList.Add(entry);
                    }
                }
            }
            string exceptionsText = "Exceptions:\n:";
            foreach (var ex in exceptions)
            {
                exceptionsText += ex + "\n";
            }

            Debug.Log($"Successfully loaded {targetList.Count} {pitchType} outcome entries from Excel with {exceptions.Count} Exceptions!\n" + exceptionsText);
            CheckForExtraEnums(targetList);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading Excel file: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void CheckForExtraEnums(List<OutcomeEntry> entries)
    {
        var usedBowlerTypes = new HashSet<TypeOfBowler>();
        var usedSides = new HashSet<Side>();
        var usedBallTypes = new HashSet<BallType>();
        var usedBallLines = new HashSet<BallLine>();
        var usedBallLengths = new HashSet<BallLength>();
        var usedTimings = new HashSet<BattingTiming>();
        var usedStrategies = new HashSet<BattingStrategy>();
        var usedOutcomes = new HashSet<OutCome>();

        foreach (var entry in entries)
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
    }

    private OutcomeEntry ParseRow(ExcelWorksheet worksheet, int row)
    {
        try
        {
            var entry = new OutcomeEntry
            {
                typeOfBowler = ParseEnum<TypeOfBowler>(worksheet.Cells[row, 1].Value?.ToString()),
                side = ParseEnum<Side>(worksheet.Cells[row, 2].Value?.ToString()),
                typeOfBall = ParseEnum<BallType>(worksheet.Cells[row, 3].Value?.ToString()),
                lineOfBall = ParseEnum<BallLine>(worksheet.Cells[row, 4].Value?.ToString()),
                lengthOfBall = ParseEnum<BallLength>(worksheet.Cells[row, 5].Value?.ToString()),
                shotSelected = ParseEnum<BattingStrategy>(worksheet.Cells[row, 6].Value?.ToString()),
            };

            string outcomeStr = worksheet.Cells[row, 8].Value?.ToString();
            string specialOutcome = worksheet.Cells[row, 9].Value?.ToString();

            if ((outcomeStr == "1 Run" || outcomeStr == "1 Run Run") && specialOutcome == "Wide Ball")
            {
                entry.outcome = OutCome.OneRunWideBall;
            }
            else if (outcomeStr == "1 Run Run")
            {
                entry.outcome = OutCome.OneRuns;
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

        if (typeof(T) == typeof(OutCome))
        {
            string trimmedValue = value.Trim();

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
        friendlyOutcomeEntries.Clear();
        hostileOutcomeEntries.Clear();
        friendlyOutcomeLookup?.Clear();
        hostileOutcomeLookup?.Clear();
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log("Cleared all outcome data");
    }

    [ContextMenu("Debug: Show Entry Count")]
    public void ShowEntryCount()
    {
        Debug.Log($"Friendly entries: {friendlyOutcomeEntries.Count}");
        Debug.Log($"Hostile entries: {hostileOutcomeEntries.Count}");
        if (friendlyOutcomeEntries.Count > 0)
        {
            Debug.Log($"First friendly entry: {friendlyOutcomeEntries[0].shotSelected} vs {friendlyOutcomeEntries[0].typeOfBall} = {friendlyOutcomeEntries[0].outcome}");
        }
        if (hostileOutcomeEntries.Count > 0)
        {
            Debug.Log($"First hostile entry: {hostileOutcomeEntries[0].shotSelected} vs {hostileOutcomeEntries[0].typeOfBall} = {hostileOutcomeEntries[0].outcome}");
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
    public BattingStrategy shotSelected;
    public BattingTiming timing;
    public OutCome outcome;
    public SpecialOutcome specialOutcome; // New field for special outcomes
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
