using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    [SerializeField] List<Flag> flags = new List<Flag>();
    public static FlagManager Instance;
    void Awake()
    {
        Instance = this;
    }
    bool isFlagExisting(string flagName)
    {
        foreach (var flag in flags)
        {
            if (flag.flagName == flagName)
                return true;
        }
        return false;
    }
    public void SetFlag(string flagName, bool value)
    {
        if (isFlagExisting(flagName))
        {
            foreach (var flag in flags)
            {
                if (flag.flagName == flagName)
                {
                    flag.flagValue = value;
                    return;
                }
            }
        }
        else
        {
            flags.Add(new Flag(flagName, value));
        }
    }
    public bool GetFlag(string flagName)
    {
        foreach (var flag in flags)
        {
            if (flag.flagName == flagName)
                return flag.flagValue;
        }
        Debug.LogWarning($"Flag '{flagName}' does not exist.");
        return false;
    }
}
[Serializable]
class Flag
{
    public string flagName;
    public bool flagValue;

    public Flag(string name, bool value)
    {
        flagName = name;
        flagValue = value;
    }
}