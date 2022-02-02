using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StickyObject : MonoBehaviour
{
    [SerializeField] private BoolGlobalVariable stickyObject;
    [SerializeField] private float delayTime = 0.15f;
    private bool allowPlayerToStick;
    private Transform parent;

    [SerializeField] private bool drawBridge;
    [SerializeField] private bool spindle;
    private Rigidbody drawBridgeRb;
    private Rigidbody playerRb;
    private HingeJoint hinge;
    [SerializeField] private float massEnter;
    [SerializeField] private float massExit;
    
    private void OnEnable()
    {
        allowPlayerToStick = true;
        parent = transform.parent;
        if (drawBridge)
        {
            drawBridgeRb = transform.parent.GetComponent<Rigidbody>();
            hinge = transform.parent.GetComponent<HingeJoint>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !stickyObject.CurrentValue && allowPlayerToStick)
        {
            stickyObject.CurrentValue = true;
            allowPlayerToStick = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DelayBelowActivatingSticky(null));
        }
    }

    private IEnumerator DelayBelowActivatingSticky(Collider other)
    {
        if (!stickyObject.CurrentValue)
        {
            yield return new WaitForSeconds(delayTime);
            allowPlayerToStick = true;
            if (other != null)
                other.transform.SetParent(GameManager.gameFolder.transform, true);
            if (drawBridge)
            {
                hinge.massScale = massExit;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !stickyObject.CurrentValue && allowPlayerToStick)
        {
            other.transform.SetParent(parent, true);
            stickyObject.CurrentValue = true;
            allowPlayerToStick = false;

            if (drawBridge)
            {
                hinge.massScale = massEnter;
                ToggleFreezePosition(true);
            }

            if (spindle)
            {
                if (playerRb == null)
                {
                    playerRb = other.GetComponent<Rigidbody>();
                }
                playerRb.isKinematic = true;
            }
        }
    }

    // For wakening the rb
    private void ToggleFreezePosition(bool on)
    {
        drawBridgeRb.constraints = on ? RigidbodyConstraints.FreezePositionX : ResetFreeze();
    }

    private RigidbodyConstraints ResetFreeze()
    {
        return RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DelayBelowActivatingSticky(null));
            if (drawBridge)
            {
                ToggleFreezePosition(false);
            }
        }
    }
}

