using Game.Scripts;
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
        
        private void FixedUpdate()
        {
            if (cachedBody.velocity.magnitude > 0.01f)
                playerMagnitude = cachedBody.velocity.magnitude;
        }

        protected virtual void OnEnable()
        {
            cachedBody = GetComponent<Rigidbody>();
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            if (audioManager == null)
                audioManager = FindObjectOfType<AudioManager>();
            if (launchRenderArc == null)
                launchRenderArc = FindObjectOfType<LaunchRenderArc>();
        }
        
        // still used?
        public void Apply(Vector2 screenDelta)
		{
			// Make sure the camera exists
			var camera = LeanTouch.GetCamera(Camera, gameObject);

			if (camera != null)
			{
				var oldPoint    = transform.position;
				var screenPoint = camera.WorldToScreenPoint(oldPoint);

				screenPoint.x += screenDelta.x;
				screenPoint.y += screenDelta.y;

				var newPoint = camera.ScreenToWorldPoint(screenPoint);

				//ApplyBetween(oldPoint, newPoint);
                //print("apply");
			}
		}

        // comes from leanFingerLine in game
        public void ApplyToOpposite(Vector3 end)
        {
            if (cachedBody == null || !cachedBody.gameObject.activeInHierarchy)
                return;

            // check if touch is below player
            if (end.y < cachedBody.transform.position.y)
            {
                // if within a good distance, allow movement, otherwise move screen
                gameManager.allowPlayerMovement = true;
                gameManager.LaunchArc = true;
            }
            else
            {
                gameManager.allowPlayerMovement = false;
                gameManager.LaunchArc = false;
                return;
            }

            if (canJump)
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
                
                // gameManager.playerRb.drag = 0;
                launchRenderArc.leanEndPos = fingerPos;
                fingerPos.z = playerPos.z = transform.position.z;

                directionNormalised = (fingerPos - playerPos).normalized;
                direction = directionNormalised * launchRenderArc.velocity;

                angle = launchRenderArc.angle;
                if (angle < 0) angle += 360;
                var forceMode = useMass ? ForceMode.Impulse : ForceMode.VelocityChange;
                if (rotateToVelocity)
                {
                    transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
                }
                
                stickyObject.CurrentValue = false;
                cachedBody.AddForce(direction * velocityMultiplier, forceMode);
                
                gameManager.PlayerFaceDirection(direction.x > 0);
                StartCoroutine(PlayerJumped());
            }
        }

        private IEnumerator PlayerJumped()
        {
            
            yield return new WaitForSeconds(0.01f);
            gameManager.PlayerJumped();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("GroundNormal"))
            {
                audioManager.PlayAudio(audioManager.cubeyLand);
            }

            if (collision.collider.CompareTag("GroundSnow"))
            {
                audioManager.PlayAudio(audioManager.cubeyLandingSnow);
            }
        }

    }
}