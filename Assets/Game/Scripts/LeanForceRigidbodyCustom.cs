﻿using Game.Scripts;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// Purchased Unity Asset and tweaked
namespace Lean.Touch
{
    [RequireComponent(typeof(Rigidbody))]
	public class LeanForceRigidbodyCustom : MonoBehaviour
	{
        [SerializeField] private Camera Camera;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private LaunchRenderArc launchRenderArc;
        [SerializeField] private BoolGlobalVariable stickyObject;
        
        [SerializeField] private bool useMass;
        [SerializeField] private bool rotateToVelocity;

        public float playerMagnitude;

        [SerializeField] private Vector3 direction;
        [SerializeField] private Vector3 directionNormalised;

        public float velocityMultiplier = -1;
        private Rigidbody cachedBody;
        private float angle;
        public bool canJump;
        public bool moveScreen;
        
        public float Angle => angle;

        public Action<bool> onGround;
        
        protected virtual void OnEnable()
        {
            cachedBody = GetComponent<Rigidbody>();
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
            if (audioManager == null) audioManager = FindObjectOfType<AudioManager>();
            if (launchRenderArc == null) launchRenderArc = FindObjectOfType<LaunchRenderArc>();
        }

        // comes from leanFingerLine in game
        // Only gets used on input released
        public void ApplyToOpposite(Vector3 end)
        {
            if (cachedBody == null || !cachedBody.gameObject.activeInHierarchy)
                return;

            if (/*canJump &&*/ FingerPos.belowPlayer && Time.timeScale > 0.2f)
            {
                ApplyBetweenOpposite(transform.position, end);
            }
        }

        // Pull back on cube to fling the opposite direction
        private void ApplyBetweenOpposite(Vector3 playerPos, Vector3 fingerPos)
        {
            if (LeanTouch.Fingers.Count >= 2)
                return;
            
            if (gameManager.allowPlayerMovement)
            {
                launchRenderArc.leanEndPos = fingerPos;
                fingerPos.z = playerPos.z = transform.position.z;

                directionNormalised = (fingerPos - playerPos).normalized;
                direction = directionNormalised * launchRenderArc.velocity;

                angle = launchRenderArc.angle;
                if (angle < 0)
                {
                    angle += 360;
                }
                
                var forceMode = useMass ? ForceMode.Impulse : ForceMode.VelocityChange;
                
                if (rotateToVelocity)
                {
                    transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
                }
                
                stickyObject.CurrentValue = false;
                cachedBody.AddForce(direction * velocityMultiplier, forceMode);
                onGround?.Invoke(false);
                canJump = false;
                
                gameManager.PlayerFaceDirection(direction.x > 0);
                StartCoroutine(PlayerJumped());
            }
        }

        private IEnumerator PlayerJumped()
        {
            yield return new WaitForSeconds(0.01f);
            gameManager.PlayerJumped();
        }

        private void Update()
        {
            playerMagnitude = cachedBody.velocity.sqrMagnitude;
        }

        public float cubeyJumpMagValue = 0.4f;

        private void OnCollisionEnter(Collision collision)
        {
            if (cachedBody.velocity.sqrMagnitude < cubeyJumpMagValue)
            {
                CheckGroundBeforeJump();
            }
            
            if (collision.collider.CompareTag("Sticky"))
            {
                // Debug.Log("Hit sticky object");
                // onGround?.Invoke(true);
            }
            else if (collision.collider.CompareTag("Platform") || collision.collider.CompareTag("MovingPlatform"))
            {
                // Debug.Log("Hit a platform");
                audioManager.PlayAudio(audioManager.cubeyLand);
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingGrass, transform.position);
                // onGround?.Invoke(true);
            }
            else if (collision.collider.CompareTag("GroundGrass"))
            {
                // Debug.Log("Hit grass");
                audioManager.PlayAudio(audioManager.cubeyLand);
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingGrass, transform.position);
                // onGround?.Invoke(true);
            }
            else if (collision.collider.CompareTag("GroundNormal"))
            {
                // Debug.Log("Hit normal ground");
                audioManager.PlayAudio(audioManager.cubeyLand);
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingDust, transform.position);
                // onGround?.Invoke(true);
            }
            else if (collision.collider.CompareTag("GroundSnow"))
            {
                // Debug.Log("Hit snow");
                // audioManager.PlayAudio(audioManager.cubeyLand);
                audioManager.PlayAudio(audioManager.cubeyLandingSnow);
                // VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pe, transform.position);
                // onGround?.Invoke(true);
            }
        }

        public void CheckGroundBeforeJump()
        {
            if (stickyObject.CurrentValue)
            {
                onGround?.Invoke(true);
                canJump = true;
                return;
            }
            
            if (canJump) return;
            
            RaycastHit hit;
            bool hitGround = false;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
            {
                Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.blue);
                hitGround = true;
            }
            
            if (hitGround)
            {
                onGround?.Invoke(true);
                canJump = true;
            }
        }
    }
}