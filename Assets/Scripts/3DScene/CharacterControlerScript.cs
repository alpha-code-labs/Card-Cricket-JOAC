using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterControlerScript : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool IsWalking = false;
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            IsWalking = true;
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            IsWalking = true;
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.A))
        {
            IsWalking = true;
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.D))
        {
            IsWalking = true;
            direction += Vector3.right;
        }
        if (direction != Vector3.zero)
        {
            // Rotate character to face movement direction
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 360 * Time.deltaTime);
        }
        animator.SetBool("IsWalking", IsWalking);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryPickUp();
        }
    }
    [SerializeField] HoldableObjectScript holdableObject;
    [SerializeField] HoldableObjectScript nearestHoldableObject;
    [SerializeField] Transform rightHand;
    void TryPickUp()
    {
        HoldableObjectScript lastNearestHoldableObject = nearestHoldableObject;
        if (holdableObject != null)
        {
            Debug.Log("Dropping Object");
            DropObject(holdableObject);
            return;
        }
        if (lastNearestHoldableObject != null)
        {
            Debug.Log("Picking Up Object");
            PickUpObject(lastNearestHoldableObject);
            animator.SetTrigger("PickUP");
            return;
        }

    }
    void PickUpObject(HoldableObjectScript obj)
    {
        holdableObject = obj;
        holdableObject.PickUp();

        holdableObject.transform.SetParent(rightHand);
        holdableObject.transform.localPosition = Vector3.zero;
    }
    void DropObject(HoldableObjectScript obj)
    {
        holdableObject = null;
        obj.Drop();
        obj.transform.SetParent(null);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<HoldableObjectScript>())
        {
            nearestHoldableObject = other.GetComponentInParent<HoldableObjectScript>();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<HoldableObjectScript>() == nearestHoldableObject)
        {
            nearestHoldableObject = null;
        }
    }
}
