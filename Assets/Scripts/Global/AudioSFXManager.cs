using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSFXManager : MonoBehaviour
{
    public static AudioSFXManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public AudioSource audioSource;
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public List<OneShotSFXData> oneShotSFXDatas = new List<OneShotSFXData>();
    public void PlayOneShotSFX(SFXType type)
    {
        OneShotSFXData data = oneShotSFXDatas.Find(x => x.type == type);
        if (data != null && data.clip != null)
        {
            audioSource.PlayOneShot(data.clip, data.volume);
        }
    }
}
[Serializable]
public class OneShotSFXData
{
    public string name;
    public SFXType type;
    public AudioClip clip;
    public float volume = 1f;

    public OneShotSFXData(AudioClip clip, float volume = 1f)
    {
        this.clip = clip;
        this.volume = volume;
    }
}
public enum SFXType
{
    ButtonClick,
    ProjectorClick,
}