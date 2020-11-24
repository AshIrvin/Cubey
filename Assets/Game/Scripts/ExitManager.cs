using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class ExitManager : MonoBehaviour
    {
   
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                VisualEffects.Instance.ExitCompletion();
                GameManager.Instance.LoadEndScreen(true);

            }
        }

    }
}