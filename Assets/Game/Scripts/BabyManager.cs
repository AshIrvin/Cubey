using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
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

        // Start is called before the first frame update
        private void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
            //player = GameObject.Find("PlayerCharacter").gameObject;
            player = GameObject.FindGameObjectWithTag("Player").gameObject;
            flip = transform.localScale;

            RandomTime();
        }

        // Update is called once per frame
        private void Update()
        {
            time -= Time.deltaTime;

            if (time < 0 && allowJump)
            {
                // jump
                MakeCharacterJump();
                // reset time
                RandomTime();

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
            if (player.transform.position.x < transform.position.x)
                flip.x = -1.54f;
            else
                flip.x = 1.54f;


            transform.localScale = flip;
        }

    }
}