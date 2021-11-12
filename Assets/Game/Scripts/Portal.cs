using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject portalExit;
    [SerializeField] private float delay = 0.3f;
    [SerializeField] private BoolGlobalVariable usePortal;
    
    private void OnEnable()
    {
        usePortal.CurrentValue = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && usePortal.CurrentValue)
        {
            usePortal.CurrentValue = false;
            other.transform.position = portalExit.transform.position;
            other.transform.rotation = Quaternion.Euler(0,0,Random.Range(0, 360));
            StartCoroutine(DelayPortalActivation());
        }
    }

    private IEnumerator DelayPortalActivation()
    {
        yield return new WaitForSeconds(delay);
        usePortal.CurrentValue = true;
    }
}
