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
                LevelManager.Instance.RestartLevel();
                return;
            }

            UiManager.Instance.SetFailedScreen(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            
            if (autoRestart)
            {
                LevelManager.Instance.RestartLevel();
                return;
            }

            UiManager.Instance.SetFailedScreen(true);    
        }
    }
}