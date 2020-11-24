using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class StickyManager : MonoBehaviour
    {
        public static StickyManager Instance { get; set; }

        bool startTimer;
        float resetTime = 0.1f;
        float time = 0.1f;

        //Rigidbody playerRb;

        [HideInInspector]
        public GameManager gameManager;
        Timer timer;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // change if using multiple players
            //playerRb = GameObject.Find("PlayerCube").GetComponent<Rigidbody>();

            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            //timer = GameObject.Find("GameManager").GetComponent<Timer>();
        }

        private void FixedUpdate()
        {
            if (startTimer)
            {
                time -= Time.deltaTime;
                //print("time: " + time);
                //timer.DisplayTimer(time);

                if (time <= 0)
                {
                    startTimer = false;
                    time = resetTime;
                    gameManager.playerRb.isKinematic = false;
                }
            }

          
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") && !gameManager.playerStuck)
            {
                print("Player has triggered sticky platform");

                gameManager.playerStuck = true;
                gameManager.playerRb.useGravity = false;

                gameManager.playerRb.velocity = Vector3.zero;
                gameManager.playerRb.angularVelocity = Vector3.zero;
                gameManager.playerRb.isKinematic = true;
                gameManager.PlayerVelocity(0);
                gameManager.PlayerAllowedJump(true);
                gameManager.playerRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

                other.transform.parent = transform;
                startTimer = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                other.transform.parent = null;
                //print("Player exited trigger stickyness");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player") && !gameManager.playerStuck)
            {
                //print("Player has touched sticky platform");

                gameManager.playerStuck = true;
                gameManager.playerRb.useGravity = false;

                gameManager.playerRb.velocity = Vector3.zero;
                gameManager.playerRb.angularVelocity = Vector3.zero;
                gameManager.playerRb.isKinematic = true;
                gameManager.PlayerVelocity(0);
                gameManager.PlayerAllowedJump(true);
                gameManager.playerRb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

                collision.transform.parent = transform;
                startTimer = true;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.parent = null;
                //print("Player exited stickyness");
            }
        }

        public void DelayUnfreeze()
        {
            //gameManager.playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
            //gameManager.playerRb.constraints = RigidbodyConstraints.FreezePosition;

            gameManager.playerRb.isKinematic = false;
            print("done. kinematic: " + gameManager.playerRb.isKinematic);
            //gameManager.playerStuck = true;
            //gameManager.PlayerAllowedJump(true);


        }

    }
}
