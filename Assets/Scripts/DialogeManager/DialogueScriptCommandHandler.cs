using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class DialogueScriptCommandHandler : MonoBehaviour
{
    public static DialogueScriptCommandHandler Instance;

    [Header("All Sprites - Characters and Backgrounds")]
    [SerializeField] List<Sprite> allSprites;

    [Header("Character Display Images")]
    [SerializeField] Image leftCharacterImage;
    [SerializeField] Image rightCharacterImage;

    [Header("UI Components")]
    [SerializeField] Image currentBGSprite;

    [Header("Dialogue Settings")]
    public static string currentNode;

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
        if (currentNode == null)//This should only happen if we directly load the scene for testing
        {
            currentNode = "scene_1";//Default Starting Node
            Debug.LogWarning("Testing?: currentNode was null, defaulting to 'scene_1'");
        }

        Debug.Log($"Starting Dialogue at node: {currentNode}");
        HideAllCharacters();
        YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(currentNode);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (YarnDialogSystemSingleTonMaker.instance.dialogueRunner != null)
            {
                YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(currentNode);
            }
            else
            {
                Debug.LogError("DialogueRunner not assigned!");
            }
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
                //Debug.Log($"Mapped '{actualSpriteName}' to normalized key '{normalizedKey}' at index {i}");
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
            //Debug.LogError($"Sprite '{spriteName}' (normalized: '{normalizedKey}') not found in sprite list!");
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

    // FADE EFFECTS
    [YarnCommand("FadeToBlack")]
    public static void FadeToBlack()
    {
        //Instance.StartCoroutine(Instance.FadeToBlackSequence());
        NewDayManager.currentEventIndex++;
        TransitionScreenManager.instance.LoadScene(SceneNames.NewDayScene);//Instead of loading this scne make newday manager a proper singleton and call BeginNewDaySequence directly
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
    Vivek,
    Amarjeet,
    Bed,
    Amit,
    Sumit,
    Pinky,
    RamCharan,
    Priya,
    Naresh,
    CoachSharma,
    SunitaMam,
    CricketDada,
    Suresh,
    AgarwalUncle,
    Fatima,
    MochiUncle,
    MunnaBhai,
    Vikram
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

public enum MusicType
{
    Heartbeat = 0,
    Exciting = 1,
    Disappointing = 2,
    Light = 3,
    Emotional = 4,
    Angry = 5
}