using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class AreaBackground : MonoBehaviour
{
    void Start()
    {
        if (gameObject.layer != 2)
        {
            Debug.LogWarning($"{gameObject.name} should be on the Ignore Raycast layer (layer 2) to avoid interfering with click events.");
        }
    }
    private void OnMouseDown()
    {
        // Debug.Log("Clicked on AreaBackground");
    }
}
