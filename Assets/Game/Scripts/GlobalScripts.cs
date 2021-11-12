using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

namespace Game.Scripts
{
    public class GlobalScripts : MonoBehaviour
    {

        public LeanForceRigidbody leanForce;

        public GameManager gameManager;
        public VisualEffects visualEffects;
        public Camera cam;
        public AudioManager audioManager;
        public LaunchRenderArc launchRenderArc;

        private void Start()
        {
            if (visualEffects == null)
                visualEffects = GameObject.Find("GameManager").GetComponent<VisualEffects>();

            if (gameManager == null)
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
    }
}