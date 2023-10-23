using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleOnTrigger : MonoBehaviour
{
    [SerializeField] private ParticleSystem visualEffect;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool useLocal;
    [SerializeField] private bool playAudio;
    
    private void Start()
    {
        if (useLocal)
            return;
        
        var ParticleEffectsGroup = GameObject.Find("Game/ParticleEffects").gameObject;

        if (ParticleEffectsGroup == null)
        {
            Logger.Instance.ShowDebugError("Can't find Particle Effects object");
            return;
        }
        
        for (int i = 0; i < ParticleEffectsGroup.transform.childCount; i++)
        {
            if (ParticleEffectsGroup.transform.GetChild(i).name.Contains(visualEffect.name))
            {
                visualEffect = ParticleEffectsGroup.transform.GetChild(i).GetComponent<ParticleSystem>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (visualEffect != null)
            {
                if (playAudio)
                {
                    AudioManager.Instance.PlayAudio(AudioManager.Instance.waterSplash);
                }
                VisualEffects.Instance.PlayEffect(visualEffect, other.transform.position + offset);
            }
        }
    }
}
