using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryResources : MonoBehaviour
{
    public List<Sprite> cardSprites;
    public static TemporaryResources instance;
    void Awake()
    {
        instance = this;
    }
}
