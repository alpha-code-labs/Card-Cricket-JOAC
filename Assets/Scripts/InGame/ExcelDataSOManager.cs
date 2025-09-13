using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExcelDataSOManager : MonoBehaviour
{
    public static ExcelDataSOManager Instance;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        outComeCalculator.BuildLookupDictionary();
    }
    public OutComeCalculator outComeCalculator;
}
