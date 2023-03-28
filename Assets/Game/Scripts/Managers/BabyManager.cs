using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class BabyManager : MonoBehaviour
    {
        private float time;
        public float force = 2f;
        private Rigidbody rb;
        private GameObject player;
        public float timeMin, timeMax;
        private Vector2 flip;
        public bool allowJump;

        private void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
            player = GameObject.FindGameObjectWithTag("Player").gameObject;
            flip = transform.localScale;

            RandomTime();

            var n = Random.Range(0, 2);
            if (n == 1)
            {
                gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            time -= Time.deltaTime;

            if (time < 0 && allowJump)
            {
                RandomTime();
                
                MakeCharacterJump();

                ChangeDirection();
            }
        }

        private void RandomTime() 
        {
            time = Random.Range(timeMin, timeMax);
        }

        private void MakeCharacterJump()
        {
            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        }

        private void ChangeDirection()
        {
            flip.x = player.transform.position.x < transform.position.x ? -flip.x : flip.x;

            transform.localScale = flip;
        }

    }
}