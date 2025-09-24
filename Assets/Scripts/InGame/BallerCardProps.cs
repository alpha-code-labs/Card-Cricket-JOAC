using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class BallerCardProps : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TypeOfBallerText;
    [SerializeField] TextMeshProUGUI PitchConditionText;
    [SerializeField] TextMeshProUGUI BallerSideText;
    [SerializeField] TextMeshProUGUI TypeOfBallText;
    [SerializeField] TextMeshProUGUI LengthOfBallText;
    [SerializeField] TextMeshProUGUI LineOfBallText;

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
        if (PitchConditionText.text != null)
            PitchConditionText.text = ballThrow.pitchCondition.ToString();
        else
            Debug.LogWarning("TypeOfBallerText is not assigned!");

        if (LineOfBallText != null)
            LineOfBallText.text = ballThrow.ballLine.ToString();  // FIX: Changed from ballLength to ballLine
        else
            Debug.LogWarning("LineOfBallText is not assigned!");

        if (LengthOfBallText != null)
            LengthOfBallText.text = ballThrow.ballLength.ToString();
        else
            Debug.LogWarning("LengthOfBallText is not assigned!");

        if (TypeOfBallText != null)
            TypeOfBallText.text = ballThrow.ballType.ToString();
        else
            Debug.LogWarning("TypeOfBallText is not assigned!");

        if (BallerSideText != null)
            BallerSideText.text = ballThrow.bowlerSide.ToString();
        else
            Debug.LogWarning("BallerSideText is not assigned!");

        if (TypeOfBallerText.text != null)
            TypeOfBallerText.text = ballThrow.bowlerType.ToString();
        else
            Debug.LogWarning("PitchConditionText is not assigned!");

        // Force UI update
        Canvas.ForceUpdateCanvases();
    }
}