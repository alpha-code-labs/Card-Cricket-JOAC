using UnityEngine;
using Yarn.Unity;

public class DialoguePositioner : MonoBehaviour
{
    public RectTransform dialogueBox; // assign the Dialogue UI panel here

    // Move to an anchored position on screen (0-1 normalized)
    public void MoveToScreenAnchor(Vector2 anchor)
    {
        // anchor = (0,0) bottom-left, (0.5,0.5) center, (1,1) top-right
        dialogueBox.anchorMin = anchor;
        dialogueBox.anchorMax = anchor;
        dialogueBox.pivot = anchor;
        dialogueBox.anchoredPosition = Vector2.zero;
    }
}
