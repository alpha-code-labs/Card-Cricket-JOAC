using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class DialoguePositioner : MonoBehaviour
{
    public RectTransform dialogueBox; // assign the Dialogue UI panel here
    //currently don't see any use case.. might be needed to update character expressions
    private Image characterImage;
    [SerializeField] private GameObject characterImageContainer;
    public static DialoguePositioner Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        characterImageContainer.SetActive(false);
    }

    public void SetPosition(Vector2 pixelPosition)
    {
        Debug.Log("setting position to" + pixelPosition);
        // This directly moves the dialogue box to the specified pixel position
        dialogueBox.anchoredPosition = pixelPosition;
    }

    public void ShowCharacterImage(Vector2 pos, float width, float height)
    {
        if (characterImageContainer == null) return;
        characterImageContainer.SetActive(true);
        // Force it to stay active
        StartCoroutine(KeepActiveForFrames());
        
        RectTransform containerRect = characterImageContainer.GetComponent<RectTransform>();
        containerRect.anchoredPosition = pos;
        containerRect.sizeDelta = new Vector2(width, height);
    }
    
    IEnumerator KeepActiveForFrames()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            if (!characterImageContainer.activeSelf)
            {
                Debug.LogError($"Container was deactivated on frame {i}!");
                characterImageContainer.SetActive(true);
            }
        }
    }
    public void HideCharacterImage()
    {
        Debug.Log("Hiding character image");
        if (characterImageContainer != null)
        {
            characterImageContainer.SetActive(false);
        }
    }
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
