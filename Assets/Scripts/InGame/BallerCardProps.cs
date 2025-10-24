using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

[Serializable]
public class BallerCardProps : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TypeOfBallerText;
    [SerializeField] TextMeshProUGUI PitchConditionText;
    [SerializeField] TextMeshProUGUI BallerSideText;
    [SerializeField] TextMeshProUGUI TypeOfBallText;
    [SerializeField] TextMeshProUGUI LengthOfBallText;
    [SerializeField] TextMeshProUGUI LineOfBallText;

    private string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrEmpty(camelCase))
            return camelCase;

        // Insert spaces before capital letters (except the first one)
        string withSpaces = Regex.Replace(camelCase, "([a-z])([A-Z])", "$1 $2");
        
        // Capitalize the first letter
        if (withSpaces.Length > 0)
        {
            withSpaces = char.ToUpper(withSpaces[0]) + withSpaces.Substring(1);
        }
        
        return withSpaces;
    }
    public void assignBallerProps(BallThrow ballThrow)
    {
        if (ballThrow == null)
        {
            Debug.LogError("BallThrow is null in assignBallerProps!");
            return;
        }

        // Log for debugging
        Debug.Log($"Assigning Baller Props - Turn {CardsPoolManager.Instance.CurrntTurn}: " +
                  $"Bowler: {ballThrow.bowlerType}, " +
                  $"Ball: {ballThrow.ballType}, " +
                  $"Line: {ballThrow.ballLine}, " +  // Note: changed to ballLine
                  $"Length: {ballThrow.ballLength}");

        // Assign the correct properties
        if (PitchConditionText != null)
            PitchConditionText.text = CamelCaseToTitleCase(ballThrow.pitchCondition.ToString());
        else
            Debug.LogWarning("PitchConditionText is not assigned!");

        if (LineOfBallText != null)
        {
            string line = CamelCaseToTitleCase(ballThrow.ballLine.ToString());
            //Downthe Leg special case--- handling seperately
            line = line.Replace("Downthe", "Down the");
            LineOfBallText.text = line;
            
        }
            
        else
            Debug.LogWarning("LineOfBallText is not assigned!");

        if (LengthOfBallText != null)
            LengthOfBallText.text = CamelCaseToTitleCase(ballThrow.ballLength.ToString());
        else
            Debug.LogWarning("LengthOfBallText is not assigned!");

        if (TypeOfBallText != null)
            TypeOfBallText.text = CamelCaseToTitleCase(ballThrow.ballType.ToString());
        else
            Debug.LogWarning("TypeOfBallText is not assigned!");

        if (BallerSideText != null)
            BallerSideText.text = CamelCaseToTitleCase(ballThrow.bowlerSide.ToString());
        else
            Debug.LogWarning("BallerSideText is not assigned!");

        if (TypeOfBallerText != null)
            TypeOfBallerText.text = CamelCaseToTitleCase(ballThrow.bowlerType.ToString());
        else
            Debug.LogWarning("TypeOfBallerText is not assigned!");
        // Force UI update
        Canvas.ForceUpdateCanvases();
    }
}