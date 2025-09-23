//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Yarn.Unity;
//using TMPro;

//public class BasicFlagManager : MonoBehaviour
//{
//    public static BasicFlagManager Instance;

//    [Header("Background Sprites")]
//    [SerializeField] List<Sprite> backgroundSprites;

//    [Header("Character Sprite Lists")]
//    [SerializeField] List<Sprite> ramuSprites;
//    [SerializeField] List<Sprite> rajuSprites;
//    [SerializeField] List<Sprite> kamlaSprites;
//    [SerializeField] List<Sprite> shivPrasadSprites;
//    [SerializeField] List<Sprite> coachDevSprites;
//    [SerializeField] List<Sprite> aryanSprites;
//    [SerializeField] List<Sprite> imtiazSprites;
//    [SerializeField] List<Sprite> vivekSprites;

//    [Header("UI Components")]
//    [SerializeField] Image currentBGSprite;
//    [SerializeField] Image ramuSprite;
//    [SerializeField] Image rajuSprite;
//    [SerializeField] Image kamlaSprite;
//    [SerializeField] Image shivPrasadSprite;
//    [SerializeField] Image coachDevSprite;
//    [SerializeField] Image aryanSprite;
//    [SerializeField] Image imtiazSprite;
//    [SerializeField] Image vivekSprite;

//    [Header("Dialogue Settings")]
//    [SerializeField] string currentNode;

//    [Header("Aryan Path")]
//    [SerializeField] public AryanPath aryanPath;


//    [Header("Date Display")]
//    [SerializeField] GameObject dateDisplayCanvas;
//    [SerializeField] TextMeshProUGUI dateText;
//    [SerializeField] float fadeDuration = 1f;

//    [Header("Fade Settings")]
//    [SerializeField] GameObject fadePanel;


//    [Header("Audio Settings")]
//    [SerializeField] AudioSource musicAudioSource;
//    [SerializeField] List<AudioClip> backgroundMusicClips;
//    [SerializeField] float musicFadeDuration = 1f;

//    void Awake()
//    {
//        Instance = this;
//    }

//    void Start()
//    {
//        DialogueRunner dialogueRunner = GetComponent<DialogueRunner>();
//        // Initialize all characters to neutral/default expression

//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.A))
//        {
//            DialogueRunner dialogueRunner = GetComponent<DialogueRunner>();
//            dialogueRunner.StartDialogue(currentNode);
//        }
//    }





//    // YARN FUNCTIONS
//    [YarnFunction("GetAryanPath")]
//    public static string GetAryanPath()
//    {
//        return Instance.aryanPath.ToString();
//    }

//    [YarnFunction("GetScene_one")]
//    public static string GetScene_one()
//    {
//        Debug.Log("GetScene one");
//        return "Scene_one";
//    }

//    // ARYAN PATH COMMANDS
//    [YarnCommand("SetAryanPath")]
//    public static void SetAryanPath(string path)
//    {
//        switch (path)
//        {
//            case "Serious":
//                Instance.aryanPath = AryanPath.Serious;
//                break;
//            case "Angry":
//                Instance.aryanPath = AryanPath.Angry;
//                break;
//            case "Quizical":
//                Instance.aryanPath = AryanPath.Quizical;
//                break;
//            default:
//                Debug.LogError("Invalid path string passed to SetAryanPath");
//                break;
//        }
//    }

//    // BACKGROUND COMMANDS
//    [YarnCommand("SetBGSprite")]
//    public static void SetBGSprite(int index)
//    {
//        if (index >= 0 && index < Instance.backgroundSprites.Count)
//        {
//            Instance.currentBGSprite.sprite = Instance.backgroundSprites[index];
//        }
//        else
//        {
//            Debug.LogError($"Background sprite index {index} out of range!");
//        }
//    }

//    // CHARACTER EXPRESSION COMMANDS
//    [YarnCommand("SetRamuExpression")]
//    public static void SetRamuExpression(string expression)
//    {
//        int index = GetRamuExpressionIndex(expression);
//        if (index >= 0 && index < Instance.ramuSprites.Count)
//        {
//            Instance.ramuSprite.sprite = Instance.ramuSprites[index];
//            Instance.ramuSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Ramu expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetRajuExpression")]
//    public static void SetRajuExpression(string expression)
//    {
//        int index = GetRajuExpressionIndex(expression);
//        if (index >= 0 && index < Instance.rajuSprites.Count)
//        {
//            Instance.rajuSprite.sprite = Instance.rajuSprites[index];
//            Instance.rajuSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Raju expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetKamlaExpression")]
//    public static void SetKamlaExpression(string expression)
//    {
//        int index = GetKamlaExpressionIndex(expression);
//        if (index >= 0 && index < Instance.kamlaSprites.Count)
//        {
//            Instance.kamlaSprite.sprite = Instance.kamlaSprites[index];
//            Instance.kamlaSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Kamla expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetShivPrasadExpression")]
//    public static void SetShivPrasadExpression(string expression)
//    {
//        int index = GetShivPrasadExpressionIndex(expression);
//        if (index >= 0 && index < Instance.shivPrasadSprites.Count)
//        {
//            Instance.shivPrasadSprite.sprite = Instance.shivPrasadSprites[index];
//            Instance.shivPrasadSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"ShivPrasad expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetCoachDevExpression")]
//    public static void SetCoachDevExpression(string expression)
//    {
//        int index = GetCoachDevExpressionIndex(expression);
//        if (index >= 0 && index < Instance.coachDevSprites.Count)
//        {
//            Instance.coachDevSprite.sprite = Instance.coachDevSprites[index];
//            Instance.coachDevSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"CoachDev expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetAryanExpression")]
//    public static void SetAryanExpression(string expression)
//    {
//        int index = GetAryanExpressionIndex(expression);
//        if (index >= 0 && index < Instance.aryanSprites.Count)
//        {
//            Instance.aryanSprite.sprite = Instance.aryanSprites[index];
//            Instance.aryanSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Aryan expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetImtiazExpression")]
//    public static void SetImtiazExpression(string expression)
//    {
//        int index = GetImtiazExpressionIndex(expression);
//        if (index >= 0 && index < Instance.imtiazSprites.Count)
//        {
//            Instance.imtiazSprite.sprite = Instance.imtiazSprites[index];
//            Instance.imtiazSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Imtiaz expression '{expression}' not found or index out of range!");
//        }
//    }

//    [YarnCommand("SetVivekExpression")]
//    public static void SetVivekExpression(string expression)
//    {
//        int index = GetVivekExpressionIndex(expression);
//        if (index >= 0 && index < Instance.vivekSprites.Count)
//        {
//            Instance.vivekSprite.sprite = Instance.vivekSprites[index];
//            Instance.vivekSprite.gameObject.SetActive(true);
//        }
//        else
//        {
//            Debug.LogError($"Vivek expression '{expression}' not found or index out of range!");
//        }
//    }

//    // EXPRESSION INDEX HELPERS
//    private static int GetRamuExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "neutral": return 0;
//            case "excited": return 1;
//            case "smiling": return 2;
//            case "serious": return 3;
//            case "sad": return 4;
//            case "worried": return 5;
//            case "quizzical": return 6;
//            default: return -1;
//        }
//    }

//    private static int GetRajuExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {

//            case "groggy": return 0;
//            case "excited": return 1;
//            case "smiling": return 2;
//            case "serious": return 3;
//            case "sad": return 4;
//            case "worried": return 5;
//            case "disgusted": return 6;
//            case "quizzical": return 7;
//            case "sleeping": return 8;
//            case "excitedwithbat": return 9;
//            case "smilingwithbat": return 10;
//            case "seriouswithbat": return 11;
//            case "rajubattingpose": return 12;
//            default: return -1;
//        }
//    }

//    private static int GetKamlaExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "neutral": return 0;
//            case "smiling": return 1;
//            case "serious": return 2;
//            case "quizzical": return 3;
//            default: return -1;
//        }
//    }

//    private static int GetShivPrasadExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "smiling": return 0;
//            case "serious": return 1;
//            case "quizzical": return 2;
//            default: return -1;
//        }
//    }

//    private static int GetCoachDevExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "smiling": return 0;
//            case "serious": return 1;
//            case "excited": return 2;
//            default: return -1;
//        }
//    }

//    private static int GetAryanExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "angry": return 0;
//            case "serious": return 1;
//            default: return -1;
//        }
//    }

//    private static int GetImtiazExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "neutral": return 0;
//            case "excited": return 1;
//            default: return -1;
//        }
//    }

//    private static int GetVivekExpressionIndex(string expression)
//    {
//        switch (expression.ToLower())
//        {
//            case "neutral": return 0;
//            case "excited": return 1;
//            default: return -1;
//        }
//    }

//    // UTILITY COMMANDS
//    [YarnCommand("HideAllCharacters")]
//    public static void HideAllCharacters()
//    {
//        Instance.ramuSprite.gameObject.SetActive(false);
//        Instance.rajuSprite.gameObject.SetActive(false);
//        Instance.kamlaSprite.gameObject.SetActive(false);
//        Instance.shivPrasadSprite.gameObject.SetActive(false);
//        Instance.coachDevSprite.gameObject.SetActive(false);
//        Instance.aryanSprite.gameObject.SetActive(false);
//        Instance.imtiazSprite.gameObject.SetActive(false);
//        Instance.vivekSprite.gameObject.SetActive(false);
//    }

//    [YarnCommand("HideCharacter")]
//    public static void HideCharacter(string characterName)
//    {
//        switch (characterName.ToLower())
//        {
//            case "ramu":
//                Instance.ramuSprite.gameObject.SetActive(false);
//                break;
//            case "raju":
//                Instance.rajuSprite.gameObject.SetActive(false);
//                break;
//            case "kamla":
//                Instance.kamlaSprite.gameObject.SetActive(false);
//                break;
//            case "shivprasad":
//                Instance.shivPrasadSprite.gameObject.SetActive(false);
//                break;
//            case "coachdev":
//                Instance.coachDevSprite.gameObject.SetActive(false);
//                break;
//            case "aryan":
//                Instance.aryanSprite.gameObject.SetActive(false);
//                break;
//            case "imtiaz":
//                Instance.imtiazSprite.gameObject.SetActive(false);
//                break;
//            case "vivek":
//                Instance.vivekSprite.gameObject.SetActive(false);
//                break;
//            default:
//                Debug.LogError($"Unknown character name: {characterName}");
//                break;
//        }
//    }

//    //[YarnCommand("ResetAllToNeutral")]
//    //public static void ResetAllToNeutral()
//    //{
//    //    Instance.SetAllCharactersToNeutral();
//    //}

//    // LEGACY SUPPORT
//    [YarnCommand("SetCharSprite")]
//    public static void SetCharSprite(int index)
//    {
//        Debug.LogWarning("SetCharSprite is deprecated. Use SetRajuExpression instead.");
//        SetRajuExpression("Neutral");
//    }

//    [YarnCommand("ShowDateScreen")]
//    public static void ShowDateScreen(string dateString)
//    {
//        Instance.StartCoroutine(Instance.DisplayDateSequence(dateString));
//    }

//    private IEnumerator DisplayDateSequence(string dateString)
//    {
//        // Show date canvas
//        dateDisplayCanvas.SetActive(true);
//        dateText.text = dateString;

//        // Fade in effect
//        CanvasGroup canvasGroup = dateDisplayCanvas.GetComponent<CanvasGroup>();
//        if (canvasGroup == null) canvasGroup = dateDisplayCanvas.AddComponent<CanvasGroup>();

//        canvasGroup.alpha = 0f;

//        // Fade in
//        float elapsed = 0f;
//        while (elapsed < fadeDuration)
//        {
//            canvasGroup.alpha = elapsed / fadeDuration;
//            elapsed += Time.deltaTime;
//            yield return null;
//        }
//        canvasGroup.alpha = 1f;

//        // Hold for 3 seconds
//        yield return new WaitForSeconds(3f);

//        // Fade out
//        elapsed = 0f;
//        while (elapsed < fadeDuration)
//        {
//            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        // Hide canvas
//        dateDisplayCanvas.SetActive(false);

//        // DON'T restart dialogue here - let it continue naturally
//    }



//    [YarnCommand("FadeToBlack")]
//    public static void FadeToBlack()
//    {
//        Instance.StartCoroutine(Instance.FadeToBlackSequence());
//    }

//    private IEnumerator FadeToBlackSequence()
//    {
//        if (fadePanel != null)
//        {
//            CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
//            if (canvasGroup == null)
//                canvasGroup = fadePanel.AddComponent<CanvasGroup>();

//            fadePanel.SetActive(true);
//            canvasGroup.alpha = 0f;

//            // Fade to black
//            float elapsed = 0f;
//            while (elapsed < fadeDuration)
//            {
//                canvasGroup.alpha = elapsed / fadeDuration;
//                elapsed += Time.deltaTime;
//                yield return null;
//            }
//            canvasGroup.alpha = 1f;
//        }
//    }


//    [YarnCommand("PlayBackgroundMusicByIndex")]
//    public static void PlayBackgroundMusic(int index)
//    {
//        Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(index));
//    }

//    [YarnCommand("PlayBackgroundMusic")]
//    public static void PlayBackgroundMusic(string musicName)
//    {
//        int index = GetMusicIndex(musicName);
//        if (index >= 0)
//        {
//            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(index));
//        }
//        else
//        {
//            Debug.LogError($"Music '{musicName}' not found!");
//        }
//    }

//    [YarnCommand("StopBackgroundMusic")]
//    public static void StopBackgroundMusic()
//    {
//        Instance.StartCoroutine(Instance.StopBackgroundMusicCoroutine());
//    }

//    // HELPER METHOD
//    private static int GetMusicIndex(string musicName)
//    {
//        switch (musicName.ToLower())
//        {
//            case "heartbeat": return 0;
//            case "exciting": return 1;
//            case "disappointing": return 2;
//            default: return -1;
//        }
//    }

//    // COROUTINES
//    private IEnumerator PlayBackgroundMusicCoroutine(int index)
//    {
//        if (index >= 0 && index < backgroundMusicClips.Count)
//        {
//            // Fade out current music if playing
//            if (musicAudioSource.isPlaying)
//            {
//                yield return StartCoroutine(FadeOutMusic());
//            }

//            // Set new music and fade in
//            musicAudioSource.clip = backgroundMusicClips[index];
//            musicAudioSource.loop = true;
//            musicAudioSource.volume = 0f;
//            musicAudioSource.Play();

//            yield return StartCoroutine(FadeInMusic());
//        }
//        else
//        {
//            Debug.LogError($"Music index {index} out of range!");
//        }
//    }

//    private IEnumerator StopBackgroundMusicCoroutine()
//    {
//        if (musicAudioSource.isPlaying)
//        {
//            yield return StartCoroutine(FadeOutMusic());
//            musicAudioSource.Stop();
//        }
//    }

//    private IEnumerator FadeInMusic()
//    {
//        float elapsed = 0f;
//        while (elapsed < musicFadeDuration)
//        {
//            musicAudioSource.volume = elapsed / musicFadeDuration;
//            elapsed += Time.deltaTime;
//            yield return null;
//        }
//        musicAudioSource.volume = 1f;
//    }

//    private IEnumerator FadeOutMusic()
//    {
//        float startVolume = musicAudioSource.volume;
//        float elapsed = 0f;

//        while (elapsed < musicFadeDuration)
//        {
//            musicAudioSource.volume = startVolume * (1f - (elapsed / musicFadeDuration));
//            elapsed += Time.deltaTime;
//            yield return null;
//        }
//        musicAudioSource.volume = 0f;
//    }

//}




//public enum AryanPath
//{
//    Serious,
//    Angry,
//    Quizical
//}





using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;
using System.Collections;

public class BasicFlagManager : MonoBehaviour
{
    public static BasicFlagManager Instance;

    [Header("All Sprites - Characters and Backgrounds")]
    [SerializeField] List<Sprite> allSprites;

    [Header("Character Display Images")]
    [SerializeField] Image leftCharacterImage;
    [SerializeField] Image rightCharacterImage;

    [Header("UI Components")]
    [SerializeField] Image currentBGSprite;

    [Header("Dialogue Settings")]
    [SerializeField] string currentNode;

    [Header("Aryan Path")]
    [SerializeField] public AryanPath aryanPath;

    [Header("Date Display")]
    [SerializeField] GameObject dateDisplayCanvas;
    [SerializeField] TextMeshProUGUI dateText;
    [SerializeField] float fadeDuration = 1f;

    [Header("Fade Settings")]
    [SerializeField] GameObject fadePanel;

    [Header("Audio Settings")]
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] List<AudioClip> backgroundMusicClips;
    [SerializeField] float musicFadeDuration = 1f;

    // Dictionary for sprite name to index mapping
    private Dictionary<string, int> spriteNameToIndex;
    private CharacterType currentActiveCharacter = CharacterType.Ramu; // Track active character
    private bool isCharacterOnLeft = true; // Track which side current character is on

    void Awake()
    {
        Instance = this;
        InitializeSpriteMapping();
    }

    void Start()
    {
        DialogueRunner dialogueRunner = GetComponent<DialogueRunner>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            DialogueRunner dialogueRunner = GetComponent<DialogueRunner>();
            dialogueRunner.StartDialogue(currentNode);
        }
    }

    private void InitializeSpriteMapping()
    {
        // Initialize sprite name to index mapping
        spriteNameToIndex = new Dictionary<string, int>();

        // Auto-populate based on sprite names in the list
        for (int i = 0; i < allSprites.Count; i++)
        {
            if (allSprites[i] != null)
            {
                string actualSpriteName = allSprites[i].name;
                string normalizedKey = NormalizeName(actualSpriteName);
                spriteNameToIndex[normalizedKey] = i;

                // Debug to see the mapping
                Debug.Log($"Mapped '{actualSpriteName}' to normalized key '{normalizedKey}' at index {i}");
            }
        }
    }

    // Helper method to normalize sprite names for lookup
    private static string NormalizeName(string name)
    {
        // Remove spaces, underscores, and convert to lowercase
        return name.Replace(" ", "").Replace("_", "").ToLower();
    }

    // MASTER CHARACTER EXPRESSION METHOD - Now callable from Yarn
    [YarnCommand("SetCharacterExpression")]
    public static void SetCharacterExpression(string characterName, string emotion)
    {
        if (!System.Enum.TryParse<CharacterType>(characterName, true, out CharacterType character))
        {
            Debug.LogError($"Unknown character name: {characterName}");
            return;
        }

        SetCharacterExpressionInternal(character, emotion);
    }

    // Internal method that does the actual work
    private static void SetCharacterExpressionInternal(CharacterType character, string emotion)
    {
        // Create sprite name: CharacterEmotion (e.g., "RamuExcited", "RajuSerious")
        string spriteName = character.ToString() + emotion;
        string normalizedKey = NormalizeName(spriteName);

        if (!Instance.spriteNameToIndex.ContainsKey(normalizedKey))
        {
            Debug.LogError($"Sprite '{spriteName}' (normalized: '{normalizedKey}') not found in sprite list!");
            return;
        }

        int spriteIndex = Instance.spriteNameToIndex[normalizedKey];
        if (spriteIndex >= 0 && spriteIndex < Instance.allSprites.Count)
        {
            Sprite targetSprite = Instance.allSprites[spriteIndex];
            if (targetSprite != null)
            {
                // Hide both character images first
                Instance.leftCharacterImage.gameObject.SetActive(false);
                Instance.rightCharacterImage.gameObject.SetActive(false);

                // If different character, switch sides
                if (Instance.currentActiveCharacter != character)
                {
                    Instance.isCharacterOnLeft = !Instance.isCharacterOnLeft;
                    Instance.currentActiveCharacter = character;
                }

                // Show character on the appropriate side
                if (Instance.isCharacterOnLeft)
                {
                    Instance.leftCharacterImage.sprite = targetSprite;
                    Instance.leftCharacterImage.gameObject.SetActive(true);
                }
                else
                {
                    Instance.rightCharacterImage.sprite = targetSprite;
                    Instance.rightCharacterImage.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError($"Sprite at index {spriteIndex} is null!");
            }
        }
        else
        {
            Debug.LogError($"Sprite index {spriteIndex} out of range!");
        }
    }

    // YARN FUNCTIONS
    [YarnFunction("GetAryanPath")]
    public static string GetAryanPath()
    {
        return Instance.aryanPath.ToString();
    }

    [YarnFunction("GetScene_one")]
    public static string GetScene_one()
    {
        Debug.Log("GetScene one");
        return "Scene_one";
    }

    // ARYAN PATH COMMANDS
    [YarnCommand("SetAryanPath")]
    public static void SetAryanPath(string path)
    {
        if (System.Enum.TryParse<AryanPath>(path, out AryanPath result))
        {
            Instance.aryanPath = result;
        }
        else
        {
            Debug.LogError($"Invalid AryanPath: {path}");
        }
    }

    // BACKGROUND COMMANDS
    [YarnCommand("SetBGSprite")]
    public static void SetBGSprite(string backgroundName)
    {
        string normalizedKey = NormalizeName(backgroundName);

        if (!Instance.spriteNameToIndex.ContainsKey(normalizedKey))
        {
            Debug.LogError($"Background sprite '{backgroundName}' (normalized: '{normalizedKey}') not found in sprite list!");
            return;
        }

        int spriteIndex = Instance.spriteNameToIndex[normalizedKey];
        if (spriteIndex >= 0 && spriteIndex < Instance.allSprites.Count)
        {
            Instance.currentBGSprite.sprite = Instance.allSprites[spriteIndex];
        }
        else
        {
            Debug.LogError($"Background sprite index {spriteIndex} out of range!");
        }
    }

    // BACKGROUND COMMANDS - Legacy index version (for backward compatibility)
    [YarnCommand("SetBGSpriteIndex")]
    public static void SetBGSpriteIndex(int index)
    {
        if (index >= 0 && index < Instance.allSprites.Count)
        {
            Instance.currentBGSprite.sprite = Instance.allSprites[index];
        }
        else
        {
            Debug.LogError($"Background sprite index {index} out of range!");
        }
    }

    // CHARACTER EXPRESSION COMMANDS - Updated to use internal method
    [YarnCommand("SetRamuExpression")]
    public static void SetRamuExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Ramu, expression);
    }

    [YarnCommand("SetRajuExpression")]
    public static void SetRajuExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Raju, expression);
    }

    [YarnCommand("SetKamlaExpression")]
    public static void SetKamlaExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Kamla, expression);
    }

    [YarnCommand("SetShivPrasadExpression")]
    public static void SetShivPrasadExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.ShivPrasad, expression);
    }

    [YarnCommand("SetCoachDevExpression")]
    public static void SetCoachDevExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.CoachDev, expression);
    }

    [YarnCommand("SetAryanExpression")]
    public static void SetAryanExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Aryan, expression);
    }

    [YarnCommand("SetImtiazExpression")]
    public static void SetImtiazExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Imtiaz, expression);
    }

    [YarnCommand("SetVivekExpression")]
    public static void SetVivekExpression(string expression)
    {
        SetCharacterExpressionInternal(CharacterType.Vivek, expression);
    }

    // Method to manually set character position
    [YarnCommand("SetCharacterPosition")]
    public static void SetCharacterPosition(string position)
    {
        bool shouldBeOnLeft = position.ToLower() == "left";

        if (shouldBeOnLeft != Instance.isCharacterOnLeft)
        {
            // Hide current side
            if (Instance.isCharacterOnLeft)
            {
                Instance.rightCharacterImage.sprite = Instance.leftCharacterImage.sprite;
                Instance.leftCharacterImage.gameObject.SetActive(false);
                Instance.rightCharacterImage.gameObject.SetActive(true);
            }
            else
            {
                Instance.leftCharacterImage.sprite = Instance.rightCharacterImage.sprite;
                Instance.rightCharacterImage.gameObject.SetActive(false);
                Instance.leftCharacterImage.gameObject.SetActive(true);
            }

            Instance.isCharacterOnLeft = shouldBeOnLeft;
        }
    }

    // UTILITY COMMANDS
    [YarnCommand("HideAllCharacters")]
    public static void HideAllCharacters()
    {
        Instance.leftCharacterImage.gameObject.SetActive(false);
        Instance.rightCharacterImage.gameObject.SetActive(false);
    }

    [YarnCommand("HideCharacter")]
    public static void HideCharacter()
    {
        Instance.leftCharacterImage.gameObject.SetActive(false);
        Instance.rightCharacterImage.gameObject.SetActive(false);
    }

    // DATE DISPLAY
    [YarnCommand("ShowDateScreen")]
    public static void ShowDateScreen(string dateString)
    {
        Instance.StartCoroutine(Instance.DisplayDateSequence(dateString));
    }

    private IEnumerator DisplayDateSequence(string dateString)
    {
        dateDisplayCanvas.SetActive(true);
        dateText.text = dateString;

        CanvasGroup canvasGroup = dateDisplayCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = dateDisplayCanvas.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = elapsed / fadeDuration;
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(3f);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        dateDisplayCanvas.SetActive(false);
    }

    // FADE EFFECTS
    [YarnCommand("FadeToBlack")]
    public static void FadeToBlack()
    {
        Instance.StartCoroutine(Instance.FadeToBlackSequence());
    }

    private IEnumerator FadeToBlackSequence()
    {
        if (fadePanel != null)
        {
            CanvasGroup canvasGroup = fadePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = fadePanel.AddComponent<CanvasGroup>();

            fadePanel.SetActive(true);
            canvasGroup.alpha = 0f;

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                canvasGroup.alpha = elapsed / fadeDuration;
                elapsed += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }

    // AUDIO COMMANDS
    [YarnCommand("PlayBackgroundMusic")]
    public static void PlayBackgroundMusic(string musicName)
    {
        if (System.Enum.TryParse<MusicType>(musicName, true, out MusicType musicType))
        {
            int index = (int)musicType;
            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(index));
        }
        else
        {
            Debug.LogError($"Music '{musicName}' not found!");
        }
    }

    [YarnCommand("StopBackgroundMusic")]
    public static void StopBackgroundMusic()
    {
        Instance.StartCoroutine(Instance.StopBackgroundMusicCoroutine());
    }

    // AUDIO COROUTINES
    private IEnumerator PlayBackgroundMusicCoroutine(int index)
    {
        if (index >= 0 && index < backgroundMusicClips.Count)
        {
            if (musicAudioSource.isPlaying)
            {
                yield return StartCoroutine(FadeOutMusic());
            }

            musicAudioSource.clip = backgroundMusicClips[index];
            musicAudioSource.loop = true;
            musicAudioSource.volume = 0f;
            musicAudioSource.Play();

            yield return StartCoroutine(FadeInMusic());
        }
        else
        {
            Debug.LogError($"Music index {index} out of range!");
        }
    }

    private IEnumerator StopBackgroundMusicCoroutine()
    {
        if (musicAudioSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
            musicAudioSource.Stop();
        }
    }

    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;
        while (elapsed < musicFadeDuration)
        {
            musicAudioSource.volume = elapsed / musicFadeDuration;
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicAudioSource.volume = 1f;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicAudioSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            musicAudioSource.volume = startVolume * (1f - (elapsed / musicFadeDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        musicAudioSource.volume = 0f;
    }
}

// ENUMS
public enum CharacterType
{
    Ramu,
    Raju,
    Kamla,
    ShivPrasad,
    CoachDev,
    Aryan,
    Imtiaz,
    Vivek
}

public enum EmotionType
{
    Neutral,
    Excited,
    Smiling,
    Serious,
    Sad,
    Worried,
    Quizzical,
    Groggy,
    Disgusted,
    Sleeping,
    Angry,
    Happy,
    Surprised,
    Confused
}

public enum AryanPath
{
    Serious,
    Angry,
    Quizical
}

public enum MusicType
{
    Heartbeat = 0,  
    Exciting = 1,      
    Disappointing = 2, 
    Light = 3,        
    Emotional = 4,    
    Angry = 5
}