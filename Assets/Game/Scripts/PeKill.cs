using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeKill : MonoBehaviour
{
    ParticleSystem pe;
    ParticleSystem.MainModule peMain;

    private void Start()
    {
        pe = GetComponent<ParticleSystem>();
        peMain = pe.main;
    }

    private void OnParticleCollision(GameObject other)
    {
        peMain.startColor = new Color(0, 0, 0, 0);

    }
}
