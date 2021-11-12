using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ExitManager : MonoBehaviour
{
    [SerializeField] private BoolGlobalVariable exitProperty;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            exitProperty.CurrentValue = true;
            VisualEffects.Instance.ExitCompletion();
        }
    }
}
