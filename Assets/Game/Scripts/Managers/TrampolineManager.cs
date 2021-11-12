using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jumparooni
{
    public class TrampolineManager : MonoBehaviour
    {
        public float trampForce = 3f;
        public Quaternion trampolineAngle;

        private Rigidbody playerRb;


        private void Start()
        {
            trampolineAngle = new Quaternion(gameObject.transform.rotation.x, gameObject.transform.rotation.y, 0, 0);
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        print("1.Entered trampoline");
        //        // when player enters trigger, the get thrown upwards
        //        playerRb = other.gameObject.GetComponent<Rigidbody>();
        //        //playerRb.AddForceAtPosition(windForce, other.gameObject.transform.position);
        //        playerRb.AddForce(0, trampForce, 0);
        //    }
        //}

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //print("2.Entered trampoline");
                playerRb = collision.gameObject.GetComponent<Rigidbody>();
                //playerRb.AddForce(trampolineAngle.x * trampForce, trampolineAngle.y * trampForce, 0, ForceMode.Impulse);
                playerRb.AddForce(transform.forward * trampForce, ForceMode.Impulse);
            }
        }

    }
}