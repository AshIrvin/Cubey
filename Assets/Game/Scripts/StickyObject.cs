using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StickyObject : MonoBehaviour
{
    [SerializeField] private BoolGlobalVariable stickyObject;

    // [SerializeField] private bool startTimer;
    // [SerializeField] private float resetTime = 0.1f;
    // [SerializeField] private float time = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))// && !stickyObject.currentValue)
        {
            stickyObject.CurrentValue = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            stickyObject.CurrentValue = false;
            collision.transform.parent = null;
        }
    }
}

