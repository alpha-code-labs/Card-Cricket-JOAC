using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using System;

public class DialogueScriptCommandHandler : MonoBehaviour
{
    public static DialogueScriptCommandHandler Instance;

    [Header("All Sprites - Characters and Backgrounds")]

    [Header("Character Display Images")]
    [SerializeField] Image centerCharacterImage;

    [Header("UI Components")]
    [SerializeField] Image currentBGSprite;

    [Header("Dialogue Settings")]
    public static string currentNode;

    [Header("Audio Settings")]
    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] List<AudioClip> backgroundMusicClips;
    [SerializeField] float musicFadeDuration = 1f;

    // Dictionary for sprite name to index mapping
    public static Dictionary<string, Sprite> spriteNameToIndex;

    private Dictionary<String, AudioClip> musicDictionary;
    void Awake()
    {
        Instance = this;
        InitializeMusicDictionary();
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
                Debug.Log($"Resrarting Current Node at: {currentNode}");
                YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue(currentNode);
            }
            else
            {
                Debug.LogError("DialogueRunner not assigned!");
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (YarnDialogSystemSingleTonMaker.instance.dialogueRunner != null)
            {
                Debug.Log($"Ending Current Node at: {currentNode}");
                YarnDialogSystemSingleTonMaker.instance.dialogueRunner.StartDialogue("EndEvent");
            }
            else
            {
                Debug.LogError("DialogueRunner not assigned!");
            }
        }
    }

    public static void InitializeSpriteMapping()
    {
        // Initialize sprite name to index mapping
        spriteNameToIndex = new Dictionary<string, Sprite>();
        List<Sprite> allSprites;
        // Load Characters and Locations separately for clarity and predictable organization
        List<Sprite> characterSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Textures/Characters"));
        List<Sprite> locationSprites = new List<Sprite>(Resources.LoadAll<Sprite>("Textures/Locations"));

        allSprites = new List<Sprite>(characterSprites.Count + locationSprites.Count);
        allSprites.AddRange(locationSprites);
        allSprites.AddRange(characterSprites);

        List<string> duplicateNames = new List<string>();// For Debugging
        // Auto-populate based on sprite names in the list
        for (int i = 0; i < allSprites.Count; i++)
        {
            if (allSprites[i] != null)
            {
                string actualSpriteName = allSprites[i].name;
                string normalizedKey = NormalizeName(actualSpriteName);
                if (!spriteNameToIndex.ContainsKey(normalizedKey))
                {
                    spriteNameToIndex[normalizedKey] = allSprites[i];
                }
                else
                {
                    duplicateNames.Add(actualSpriteName + " (normalized: " + normalizedKey + ")");
                }
            }
        }
        if (duplicateNames.Count > 0)
        {
            Debug.LogWarning($"Sprite load: Total={allSprites.Count},Mapped={spriteNameToIndex.Count}, Duplicates={duplicateNames.Count}\n{(duplicateNames.Count > 0 ? "Duplicates:\n" + string.Join("\n", duplicateNames) : "")}");
        }
    }

    private void InitializeMusicDictionary()
    {
        musicDictionary = new Dictionary<string, AudioClip>();

        // Map each AudioClip by its actual name, not by index or enum value
        for (int i = 0; i < backgroundMusicClips.Count; i++)
        {
            if (backgroundMusicClips[i] != null)
            {
                AudioClip clip = backgroundMusicClips[i];
                string clipName = clip.name; // Get the actual name of the AudioClip

                // Add the clip with its exact name
                musicDictionary[clipName] = clip;

                // Also add normalized version for flexibility
                string normalizedName = NormalizeName(clipName);
                if (normalizedName != clipName)
                {
                    musicDictionary[normalizedName] = clip;
                }
            }
        }

        Debug.Log($"Initialized music dictionary with {musicDictionary.Count} entries");
    }
    public static Sprite GetSpriteByName(string spriteName, bool logErrorIfNotFound = true)
    {
        string normalizedKey = NormalizeName(spriteName);
        if (spriteNameToIndex.ContainsKey(normalizedKey) && spriteNameToIndex[normalizedKey] != null)
        {
            return spriteNameToIndex[normalizedKey];
        }
        else
        {
            if (logErrorIfNotFound)
                Debug.LogError($"Sprite '{spriteName}' (normalized: '{normalizedKey}') not found in sprite list!");
            return null;
        }
    }

    // Helper method to normalize sprite names for lookup
    private static string NormalizeName(string name)
    {
        // Remove all special characters, keep only alphabetic characters, and convert to lowercase
        string result = "";
        foreach (char c in name)
        {
            if (char.IsLetter(c))
            {
                result += c;
            }
        }
        return result.ToLower();
    }

    // MASTER CHARACTER EXPRESSION METHOD - Now callable from Yarn
    [YarnCommand("SetCharacterExpression")]
    public static void SetCharacterExpression(string characterName, string emotion)
    {
        if (!System.Enum.TryParse<Characters>(characterName, true, out Characters character))
        {
            Debug.LogError($"Unknown character name: {characterName}");
            return;
        }

        // Create sprite name: CharacterEmotion (e.g., "RamuExcited", "RajuSerious")
        string spriteName = character.ToString() + emotion;
        Sprite targetSprite = GetSpriteByName(spriteName);
        AnimationCharacterSpriteChange();
        // Local function to handle sprite change animation
        void AnimationCharacterSpriteChange()
        {
            // Use a local id so we can both kill and create the tween with the same identifier
            const string localTweenId = "CenterCharacterFade";
            // Kill any existing tween with the same id to clean up old animations
            DOTween.Kill(localTweenId);

            // (Removed redundant instance Kill - DOTween.Kill(localTweenId) handles global cleanup)
            // Set sprite and ensure image alpha is zero before fade-in
            if (Instance.centerCharacterImage.sprite == targetSprite)
            {
                return;
            }
            Instance.centerCharacterImage.sprite = targetSprite;
            Instance.centerCharacterImage.gameObject.SetActive(true);

            // Ensure color alpha is set to 0
            Color startCol = Instance.centerCharacterImage.color;
            startCol.a = 0f;
            Instance.centerCharacterImage.color = startCol;

            // Position the image below the view so it can slide up
            var rt = Instance.centerCharacterImage.rectTransform;
            var anchored = rt.anchoredPosition;
            anchored.y = -25f;
            rt.anchoredPosition = anchored;

            // Shared restore function for both Complete and Kill - ensure final position and alpha
            Action restoreToSolid = () =>
            {
                // Ensure fully opaque
                Color cc = Instance.centerCharacterImage.color;
                cc.a = 1f;
                Instance.centerCharacterImage.color = cc;

                // Ensure anchored position is at 0
                var a = rt.anchoredPosition;
                a.y = 0f;
                rt.anchoredPosition = a;
            };

            // Create a sequence to move up and fade in at the same time, assign id so it can be killed later
            Sequence seq = DOTween.Sequence();
            seq.SetId(localTweenId)
               .SetUpdate(true)
               .Append(rt.DOAnchorPosY(0f, 0.2f).SetEase(Ease.OutCubic))
               .Join(Instance.centerCharacterImage.DOFade(1f, 0.25f))
               .OnComplete(() => restoreToSolid())
               .OnKill(() => restoreToSolid());
        }
    }



    // BACKGROUND COMMANDS
    [YarnCommand("SetBGSprite")]
    public static void SetBGSprite(string backgroundName)
    {
        Instance.currentBGSprite.sprite = GetSpriteByName(backgroundName);
    }

    // UTILITY COMMANDS
    [YarnCommand("HideAllCharacters")]
    public static void HideAllCharacters()
    {
        Instance.centerCharacterImage.gameObject.SetActive(false);
    }

    // AUDIO COMMANDS
    [YarnCommand("PlayBackgroundMusic")]
    public static void PlayBackgroundMusic(string musicName)
    {
        // Try to find music in dictionary
        if (Instance.musicDictionary.ContainsKey(musicName))
        {
            AudioClip musicClip = Instance.musicDictionary[musicName];
            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(musicClip));
        }
        // Try normalized name
        else if (Instance.musicDictionary.ContainsKey(NormalizeName(musicName)))
        {
            AudioClip musicClip = Instance.musicDictionary[NormalizeName(musicName)];
            Instance.StartCoroutine(Instance.PlayBackgroundMusicCoroutine(musicClip));
        }
        else
        {
            Debug.LogError($"Music '{musicName}' not found in dictionary!");
        }
    }
    [YarnCommand("StopBackgroundMusic")]
    public static void StopBackgroundMusic()
    {
        Instance.StartCoroutine(Instance.StopBackgroundMusicCoroutine());
    }

    // AUDIO COROUTINES
    private IEnumerator PlayBackgroundMusicCoroutine(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            // If same music is already playing, exit immediately
            if (musicAudioSource.clip == musicClip && musicAudioSource.isPlaying)
            {
                yield break; // Exit immediately
            }

            // Handle music switching
            if (musicAudioSource.isPlaying && musicAudioSource.clip != musicClip)
            {
                // Don't yield - start fade-out independently
                StartCoroutine(FadeOutMusic());
            }

            musicAudioSource.clip = musicClip;
            musicAudioSource.loop = true;
            musicAudioSource.volume = 0f;
            musicAudioSource.Play();

            // Don't yield - start fade-in independently
            StartCoroutine(FadeInMusic());
        }

        // Coroutine ends immediately
        yield break;
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

public enum EmotionType
{
    Angry,
    Anxious,
    Astonished,
    BattingPose,
    Bored,
    Confident,
    Confused,
    Crying,
    Curious,
    Determined,
    Disappointed,
    Disapproving,
    Disgusted,
    Embarrassed,
    Excited,
    ExcitedWithBat,
    Furious,
    Irritated,
    Loving,
    Mischievous,
    Neutral,
    Overjoyed,
    Pained,
    Proud,
    Quizzical,
    Relaxed,
    Relieved,
    Sad,
    Serious,
    SeriousWithBat,
    Shocked,
    Skeptical,
    Smiling,
    Terrified,
    Tired,
    Worried,
    BaseSprite
}

public enum MusicType
{
    Heartbeat = 0,
    Exciting_1 = 1,
    Disappointing_1 = 2,
    Light_1 = 3,
    Emotional_1 = 4,
    Angry_1 = 5,

    Spiritual_1 = 6,

    Light_3 = 7,

    Emotional_2 = 8,


}


