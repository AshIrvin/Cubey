using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Game.Scripts
{
    public class PlatformManager : MonoBehaviour
    {
        #region Public Variables

        [Header("Scripts")]
        public VisualEffects visualEffects;
        public GameManager gameManager;

        public GameObject arrow;

        [Header("Bools")]
        public bool enableArrow;
        public bool moveHor, fallingPlatform, fixedObject;

        [Header("Floats")]
        public float startXOffset;
        public float startYOffset;
        public float horPlatformDistance = 3.5f;
        public float vertPlatformDistance = 2f;
        public float smoothTime = 2F;     // how it takes to get there
        public float offset = 0.02f;

        [Header("Player Collider")]
        public Collider playerCol;

        #endregion

        #region Private Variables

        [SerializeField] private bool fadeInOut;
        [SerializeField] private Vector3 velocity;
        [SerializeField] private Vector3 targetPos;
        [SerializeField] private Vector3 currentPos;
        [SerializeField] private float playerDragThruCloud = 25f;
        [SerializeField] private SpriteRenderer sprite;

        private Vector3 startPos;
        private GameObject spawnedArrow;

        private bool movingPlatform;
        private bool verticalMovingPlatform;
        private bool moveRight;
        private bool moveUp;
        
        #endregion


        private void Start()
        {
            if (sprite == null)
                sprite = GetComponent<SpriteRenderer>();

            if (visualEffects == null)
                visualEffects = GameObject.Find("GameManager").GetComponent<VisualEffects>();

            if (gameManager == null)
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            movingPlatform = true;
            verticalMovingPlatform = true;

            startPos = transform.position;
            startPos.x += startXOffset;
            startPos.y += startYOffset;

            targetPos = new Vector3(startPos.x + horPlatformDistance, startPos.y + vertPlatformDistance);
            playerDragThruCloud = 25f;
        }

        private void FixedUpdate()
        {
            HorizontalPlatforms();
            VerticalPlatforms();
            currentPos = transform.position;
        }

        // for moving horizontal platforms
        void HorizontalPlatforms()
        {
            if (movingPlatform && moveHor && !fixedObject)
            {
                if (moveRight)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
                    if (transform.position.x > (targetPos.x - offset))
                        moveRight = false;
                }
                else
                {
                    transform.position = Vector3.SmoothDamp(transform.position, startPos, ref velocity, smoothTime);
                    if (transform.position.x < (startPos.x + offset))
                        moveRight = true;
                }
            }
        }

        // for moving vertical platforms
        void VerticalPlatforms()
        {
            if (verticalMovingPlatform && !moveHor && !fixedObject)
            {
                if (moveUp)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
                    if (transform.position.y > (targetPos.y - offset))
                        moveUp = false;
                }
                else
                {
                    transform.position = Vector3.SmoothDamp(transform.position, startPos, ref velocity, smoothTime);
                    if (transform.position.y < (startPos.y + offset))
                        moveUp = true;
                }

                // checks against start pos on how far to move
                if (transform.position.y > (startPos.y + vertPlatformDistance))
                {
                    moveUp = false;
                }
                else if (transform.position.y < startPos.y)
                {
                    moveUp = true;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Pickup"))
            {
                GameManager.Instance.PlayerAllowedJump(true);

                if (GameManager.Instance.isActiveAndEnabled && fallingPlatform)
                    GameManager.Instance.onBreakablePlatform = true;

                if (fallingPlatform)
                {
                    playerCol = collision.collider;
                    StartCoroutine(WaitForPlatform(3));
                }

                if (verticalMovingPlatform || (movingPlatform && fixedObject))
                {
                    collision.transform.parent = transform;

                    fixedObject = false;

                    if (spawnedArrow != null)
                        spawnedArrow.SetActive(false);
                }
            } 
        }

        // Player cant jump in the air
        private void OnCollisionExit(Collision collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                GameManager.Instance.PlayerAllowedJump(false);

                if (GameManager.Instance.isActiveAndEnabled)
                    GameManager.Instance.onBreakablePlatform = false;

                collision.transform.parent = null;
                playerCol = null;
            }
        }

        // Play PE when platform falls
        private IEnumerator WaitForPlatform(float timer)
        {
            visualEffects.PlayEffect(visualEffects.pePlatformDust, transform.position);

            yield return new WaitForSeconds(timer);
            FallingPlatform();
        }

        private void FallingPlatform() 
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            DestroyObject();
        }

        private void DestroyObject()
        {
            visualEffects.PlayEffect(visualEffects.pePlatformExplode1, transform.position);
            visualEffects.PlayEffect(visualEffects.pePlatformExplode2, transform.position);

            PlatformEnable(false);

            if (playerCol != null)
                playerCol.transform.parent = null;

            StartCoroutine(RespawnPlatform(3));
        }

        private void PlatformEnable(bool enable)
        {
            var col = transform.GetChild(1).GetComponent<BoxCollider>();
            var sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
            col.enabled = enable;
            sprite.enabled = enable;

        }

        private IEnumerator RespawnPlatform(float timer)
        {
            yield return new WaitForSeconds(timer);
            visualEffects.pePlatformExplode2.Play();
            PlatformEnable(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.transform.parent = transform;

                Rigidbody rb = other.GetComponent<Rigidbody>();
                rb.drag = playerDragThruCloud;

                GameManager.Instance.forceJump = true;
                GameManager.Instance.PlayerAllowedJump(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.forceJump = false;
                GameManager.Instance.PlayerAllowedJump(false);

                other.transform.parent = null;

                Rigidbody rb = other.GetComponent<Rigidbody>();
                rb.drag = 0f;
            }
        }           

    }
}