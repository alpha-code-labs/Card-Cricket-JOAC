using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObjectScript : MonoBehaviour
{
    Rigidbody rb;
    Collider col;
    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.Log("Add a RB and a Trigger Collider on the childerne of this pickupableobject");
        }
        col = GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.Log("Add a RB and a Trigger Collider on the childerne of this pickupableobject");
        }
    }
    public void PickUp()
    {
        rb.isKinematic = true;
        col.enabled = false;
    }
    public void Drop()
    {
        rb.isKinematic = false;
        col.enabled = true;
    }

}
