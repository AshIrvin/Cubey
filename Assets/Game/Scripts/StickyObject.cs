using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StickyObject : MonoBehaviour
{
    [SerializeField] private BoolGlobalVariable stickyObject;
    private float delayTime = 0.3f;
    private bool allowPlayerToStick;
    private Transform parent;

    [SerializeField] private bool drawBridge;
    [SerializeField] private bool spindle;
    private Rigidbody drawBridgeRb;
    private Rigidbody playerRb;
    private HingeJoint hinge;
    [SerializeField] private float massEnter;
    [SerializeField] private float massExit;

    public bool allowJointNudge = true;

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
            AudioManager.Instance.PlayAudio(AudioManager.Instance.sticky);
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
            AudioManager.Instance.PlayAudio(AudioManager.Instance.sticky);
            yield return new WaitForSeconds(delayTime);
            allowPlayerToStick = true;
            
            if (other != null)
            {
                other.transform.SetParent(GameManager.gameFolder.transform, true);
            }
            
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
            AudioManager.Instance.PlayAudio(AudioManager.Instance.sticky);
            
            other.transform.SetParent(parent, true);
            stickyObject.CurrentValue = true;
            allowPlayerToStick = false;

            if (drawBridge && allowJointNudge)
            {
                hinge.massScale = massEnter;
                StartCoroutine(ToggleFreezePosition(true));
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
    private IEnumerator ToggleFreezePosition(bool state)
    {
        drawBridgeRb.constraints = ResetFreeze();
        yield return new WaitForSeconds(0.1f);
    }

    private RigidbodyConstraints ResetFreeze()
    {
        return RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!spindle)
            {
                stickyObject.CurrentValue = false;
            }
            
            StartCoroutine(DelayBelowActivatingSticky(null));

            if (drawBridge && allowJointNudge)
            {
                StartCoroutine(ToggleFreezePosition(false));
                allowPlayerToStick = true;
            }
        }
    }
}

