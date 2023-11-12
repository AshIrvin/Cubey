using UnityEngine;

public class ParticleOnTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool useLocal;
    [SerializeField] private bool playAudio;

    private GameObject particleEffectsGroup;
    private ParticleSystem particleEffect;


    private void Start()
    {
        if (useLocal)
            return;

        particleEffectsGroup = VisualEffects.Instance.ParticleEffectsGroup;
        particleEffect = VisualEffects.Instance.peWaterSplash;

        if (particleEffectsGroup == null)
        {
            Logger.Instance.ShowDebugError("Can't find Particle Effects object");
            return;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (particleEffect == null) return;

        if (other.CompareTag("Player"))
        {
            if (playAudio)
            {
                AudioManager.Instance.PlayAudio(AudioManager.Instance.waterSplash);
            }

            VisualEffects.Instance.PlayEffect(particleEffect, other.transform.position + offset);
        }
    }
}
