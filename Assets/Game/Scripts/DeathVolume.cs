using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Assets.Game.Scripts
{
    public class DeathVolume : MonoBehaviour
    {


        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                GameManager.Instance.FailedScreen(true);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameManager.Instance.FailedScreen(true);
            }
        }


    }
}