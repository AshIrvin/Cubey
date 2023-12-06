using UnityEngine;

namespace Game.Scripts
{
    public class DeathVolume : MonoBehaviour
    {
        [SerializeField] private bool autoRestart;

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.collider.CompareTag("Player")) return;
            
            if (autoRestart)
            {
                UiManager.Instance.RestartLevel();
                return;
            }

            UiManager.Instance.ShowFailedScreen(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            
            if (autoRestart)
            {
                UiManager.Instance.RestartLevel();
                return;
            }

            UiManager.Instance.ShowFailedScreen(true);    
        }
    }
}