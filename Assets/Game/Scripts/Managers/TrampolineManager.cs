﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jumparooni
{
    public class TrampolineManager : MonoBehaviour
    {
        [SerializeField] private float trampForce = 3f;
        private Quaternion trampolineAngle;
        private Rigidbody playerRb;

        private void Start()
        {
            trampolineAngle = new Quaternion(gameObject.transform.rotation.x, gameObject.transform.rotation.y, 0, 0);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerRb = collision.gameObject.GetComponent<Rigidbody>();
                playerRb.AddForce(transform.forward * trampForce, ForceMode.Impulse);
            }
        }
    }
}