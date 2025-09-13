using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AttackCardData
{
    [SerializeField] internal BattingStrategy excelBattinStrategy;
    [SerializeField] internal Sprite cardSprite;
    [SerializeField] internal int EnergyCost = 3;
    public AttackCardData(BattingStrategy battingStrategy)
    {
        this.excelBattinStrategy = battingStrategy;
        EnergyCost = (int)GetBattingIntensityFromExcelName(battingStrategy);
        cardSprite = TryGetSprite(battingStrategy.ToString().ToLower());
    }
    TrueBattingStrategy11 GetTrueBattingStrategyFromExcelName(BattingStrategy battingStrategy)
    {
        string result = battingStrategy.ToString().ToLower();

        // Remove intensity suffixes first
        result = result
            .Replace("push", "")
            .Replace("normal", "")
            .Replace("aggressive", "")
            .Replace("lofted", "");
        if (Enum.TryParse(result, out TrueBattingStrategy11 trueBattingStrategy))
            return trueBattingStrategy;

        Debug.LogWarning($"Could not parse '{result}' to TrueBattingStrategy11");
        return TrueBattingStrategy11.Leave; // Default value
    }
    BattingIntensity GetBattingIntensityFromExcelName(BattingStrategy battingStrategy)
    {
        string result = battingStrategy.ToString().ToLower();

        if (result.Contains("push"))
            return BattingIntensity.Push;
        else if (result.Contains("normal"))
            return BattingIntensity.Normal;
        else if (result.Contains("aggressive"))
            return BattingIntensity.Aggressive;
        else if (result.Contains("lofted"))
            return BattingIntensity.Lofted;
        else if (result.Contains("defense"))
            return BattingIntensity.Defensive;
        else if (result.Contains("leave"))
            return BattingIntensity.Leave;

        Debug.LogWarning($"Could not determine intensity from '{result}'");
        return BattingIntensity.Normal; // Default value
    }
    Sprite TryGetSprite(string spriteName)
    {
        List<Sprite> sprites = TemporaryResources.instance.cardSprites;
        return FindRelevantSprite(spriteName);
    }

    Sprite FindRelevantSprite(string strategyName)
    {
        List<Sprite> sprites = TemporaryResources.instance.cardSprites;

        if (sprites == null || sprites.Count == 0)
            return null;

        // Convert strategy name to expected sprite format (e.g., "StraightDrivePush" -> "straight_drive")
        string expectedSpriteName = ConvertStrategyToSpriteName(strategyName);

        // First, try to find an exact match with the converted name
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null && sprite.name.Equals(expectedSpriteName, StringComparison.OrdinalIgnoreCase))
            {
                return sprite;
            }
        }

        // If no exact match, try partial matching
        foreach (Sprite sprite in sprites)
        {
            if (sprite != null)
            {
                string lowerSpriteName = sprite.name.ToLower();

                // Check if sprite name contains key parts of the expected name
                if (lowerSpriteName.Contains(expectedSpriteName) || expectedSpriteName.Contains(lowerSpriteName))
                {
                    return sprite;
                }
            }
        }

        // If still no match, return the first available sprite as fallback
        return sprites.Find(s => s != null);
    }

    string ConvertStrategyToSpriteName(string strategyName)
    {
        // Convert from PascalCase to snake_case and remove intensity suffixes
        string result = strategyName.ToLower();

        // Remove intensity suffixes first
        result = result
            .Replace("push", "")
            .Replace("normal", "")
            .Replace("aggressive", "")
            .Replace("lofted", "");

        // Handle specific mappings
        if (result.Contains("straightdrive"))
            return "straight_drive";
        else if (result.Contains("coverdrive"))
            return "cover_drive";
        else if (result.Contains("squaredrive"))
            return "square_drive";
        else if (result.Contains("ondrive"))
            return "on_drive";
        else if (result.Contains("legglance"))
            return "leg_glance";
        else if (result.Contains("forwarddefense"))
            return "forward_defense";
        else if (result.Contains("backfootdefense"))
            return "backfoot_defense";
        else if (result.Contains("cut"))
            return "cut";
        else if (result.Contains("pull"))
            return "pull";
        else if (result.Contains("sweep"))
            return "sweep";
        else if (result.Contains("leave"))
            return "leave";

        // Default: convert PascalCase to snake_case
        return System.Text.RegularExpressions.Regex.Replace(result, "(?<!^)([A-Z])", "_$1").ToLower();
    }
}



public enum BattingStrategy
{
    // BackfootDefence,
    // CoverDrive,
    // Cut,
    // ForwardDefence,
    // LegGlance,
    Leave,
    // OnDrive,
    // Pull,
    // SquareDrive,
    // StraightDrive,
    // Sweep
    ///// Shots with Intensity

    // Straight Drive variations
    StraightDrivePush,
    StraightDriveNormal,
    StraightDriveAggressive,
    StraightDriveLofted,

    // Cover Drive variations
    CoverDrivePush,
    CoverDriveNormal,
    CoverDriveAggressive,
    CoverDriveLofted,

    // Square Drive variations
    SquareDrivePush,
    SquareDriveNormal,
    SquareDriveAggressive,
    SquareDriveLofted,

    // On Drive variations
    OnDrivePush,
    OnDriveNormal,
    OnDriveAggressive,
    OnDriveLofted,

    // Cut variations
    CutPush,
    CutNormal,
    CutAggressive,
    CutLofted,

    // Pull variations
    PullPush,
    PullNormal,
    PullAggressive,
    PullLofted,

    // Sweep variations
    SweepPush,
    SweepNormal,
    SweepAggressive,
    SweepLofted,

    // Leg Glance variations
    LegGlancePush,
    LegGlanceNormal,
    LegGlanceAggressive,
    LegGlanceLofted,

    // Defensive shots
    ForwardDefensePush,
    BackfootDefensePush
}

public enum TrueBattingStrategy11
{
    BackfootDefence,
    CoverDrive,
    Cut,
    ForwardDefence,
    LegGlance,
    Leave,
    OnDrive,
    Pull,
    SquareDrive,
    StraightDrive,
    Sweep
}

public enum OutCome
{
    NoRunWideBall = -2,
    Out = -1,
    NoRun = 0,
    OneRuns = 1,
    TwoRuns = 2,
    ThreeRuns = 3,
    FourRuns = 4,
    SixRuns = 6
}

public enum BattingIntensity
{
    Leave = -1,
    Defensive = +1,
    Push = 0,
    Normal = 1,
    Aggressive = 2,
    Lofted = 3
}