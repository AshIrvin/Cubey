using Game.Scripts;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Purchased Unity Asset and modified
namespace Lean.Touch
{
    [RequireComponent(typeof(Rigidbody))]
	public class LeanForceRigidbodyCustom : MonoBehaviour
	{
        [SerializeField] private Camera Camera;
        [SerializeField] private LaunchRenderArc launchRenderArc;
        [SerializeField] private BoolGlobalVariable stickyObject;
        [SerializeField] private bool useMass;
        [SerializeField] private bool rotateToVelocity;
        [SerializeField] private Vector3 direction;
        [SerializeField] private Vector3 directionNormalised;

        private GameManager gameManager;
        private AudioManager audioManager;
        private Rigidbody cachedBody;
        private float angle;

        public float velocityMultiplier = -1;
        public float playerMagnitude;
        public bool canJump;
        public bool moveScreen;
        public Action<bool> onGround;
        
        public float Angle => angle;

        
        protected virtual void OnEnable()
        {
            cachedBody = GetComponent<Rigidbody>();
            gameManager = GameManager.Instance;
            audioManager = AudioManager.Instance;
            if (launchRenderArc == null) launchRenderArc = FindFirstObjectByType<LaunchRenderArc>();
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

            CheckGround(collision);
        }

        private void CheckGround(Collision collision)
        {
            switch (collision.collider.name)
            {
                case "Sticky":
                    break;
                case "Platform":
                case "MovingPlatform":
                    audioManager.PlayAudio(audioManager.cubeyLand);
                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingGrass, transform.position);
                    break;
                case "GroundGrass":
                    audioManager.PlayAudio(audioManager.cubeyLand);
                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingGrass, transform.position);
                    break;
                case "GroundNormal":
                    audioManager.PlayAudio(audioManager.cubeyLand);
                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peLandingDust, transform.position);
                    break;
                case "GroundSnow":
                    // audioManager.PlayAudio(audioManager.cubeyLand);
                    audioManager.PlayAudio(audioManager.cubeyLandingSnow);
                    // VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pe, transform.position);
                    break;
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

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
            {
                Debug.DrawRay(transform.position, Vector3.down * hit.distance, Color.blue);

                onGround?.Invoke(true);
                canJump = true;
            }
        }
    }
}