using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Game.Scripts
{
    public class DeathVolume : MonoBehaviour
    {
        private GameManager gameManager;
        [SerializeField] private bool autoRestart;

        private void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                if (autoRestart)
                {
                    gameManager.RestartLevel();
                }
                else
                {
                    gameManager.FailedScreen(true);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (autoRestart)
                {
                    gameManager.RestartLevel();
                }
                else
                {
                    gameManager.FailedScreen(true);
                }
            }
        }


    }
}