using UnityEngine;
using UnityEngine.Audio;

public class MusicIntensity : MonoBehaviour
{
    [Header("Mixer & Exposed Params")]
    public AudioSource musicSource;
    public AudioMixer mixer;
    public string pExcite = "Music_Excitement";
    public string pLPF    = "Music_LPF_Cutoff";
    public string pHigh   = "Music_HighShelf_Gain";
    public string pDrive  = "Music_Drive";
    public string pThresh = "Music_Comp_Thresh";
    public string pRev    = "Music_RevSend";

    [Header("Behavior")]
    [Range(0f,1f)] public float target = 0f;
    public float rampSeconds = 1.0f;

    float current;

    void OnEnable() { current = target; Apply(current); }

    public void SetExcitement(float x, float seconds = -1f)
    {
        target = Mathf.Clamp01(x);
        if (seconds >= 0f) rampSeconds = Mathf.Max(0.01f, seconds);
    }

    void Update()
    {
        if (Mathf.Approximately(current, target)) return;
        float speed = (rampSeconds <= 0.01f) ? 1e9f : (Time.unscaledDeltaTime / rampSeconds);
        current = Mathf.MoveTowards(current, target, speed);
        Apply(current);
    }

    void Apply(float x)
    {
        // Curves (tweak to taste)

        // LPF cutoff (exp curve from ~900 Hz to 18 kHz)
        float cutoff = Mathf.Lerp(Mathf.Log(900f), Mathf.Log(18000f), x);
        mixer.SetFloat(pLPF, Mathf.Exp(cutoff)); // LPF wants Hz

        // High shelf gain (dB): -3 → +4
        float high = Mathf.Lerp(-3f, 4f, x);
        mixer.SetFloat(pHigh, high);

        // Drive (0 → 0.15)
        float drive = Mathf.Lerp(0f, 0.15f, Mathf.SmoothStep(0f, 1f, x));
        mixer.SetFloat(pDrive, drive);

        // Compressor threshold (dB): -10 → -22 (more clamp when excited)
        float thr = Mathf.Lerp(-10f, -22f, x);
        mixer.SetFloat(pThresh, thr);

        // Reverb send (dB): -6 → -20 (dryer when excited)
        float rev = Mathf.Lerp(-6f, -20f, x);
        // mixer.SetFloat(pRev, rev);

        // Expose raw for debugging/visualization if you like
        // mixer.SetFloat(pExcite, x);

         //Add pitch modulation
        float pitch = Mathf.Lerp(1.0f, 1.05f, Mathf.SmoothStep(0f, 1f, x)); // up to +5% speed when fully excited
        // musicSource.pitch = pitch;
    }
}
