using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        // hit anything and is destroyed?
        gameObject.SetActive(false);
        // play particle effect
        // VisualEffects.Instance.PlayEffect();
    }
}
