using UnityEngine;

public class ExitManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.LoadEndScreen(true);
            VisualEffects.Instance.ExitCompletion();
        }
    }
}
