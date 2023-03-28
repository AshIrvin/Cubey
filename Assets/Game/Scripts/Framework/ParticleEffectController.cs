using System;
using UnityEngine;

public class ParticleEffectController : MonoBehaviour
{
    [SerializeField] 
    private FloatProperty property;
    
    [SerializeField] 
    private ParticleSystem particle;

    // private PropController prop;
    private ParticleSystem.EmissionModule emission;
    private void Awake()
    {
        // prop = GetComponentInParent<PropController>();
        // prop.OnPropertiesUpdated += PropertiesUpdated;

        emission = particle.emission;
    }
    
    private void PropertiesUpdated(PropProperties properties)
    {
        emission.rateOverTime = properties.GetFloat(property);
    }
}
