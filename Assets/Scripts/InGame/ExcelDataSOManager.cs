using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcelDataSOManager : MonoBehaviour
{
    public static ExcelDataSOManager Instance;
    public OutComeCalculator outComeCalculator;

    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        outComeCalculator.BuildLookupDictionary();
    }

    [ContextMenu("Test Outcome Calculator")]
    public void Test()
    {
        Debug.Log("Testing Outcome Calculator");
    }
}
