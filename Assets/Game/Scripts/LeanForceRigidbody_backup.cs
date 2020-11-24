//using Jumparooni;
//using UnityEngine;

//namespace Lean.Touch
//{
//    [RequireComponent(typeof(Rigidbody))]
//    public class LeanForceRigidbody : MonoBehaviour
//    {
//        [Tooltip("The camera the force will be calculated using (None = MainCamera)")]
//        public Camera Camera;
//        public GameManager gameManager;
//        //public Camera MainCam;

//        [Tooltip("Use mass in velocity calculation?")]
//        public bool UseMass;

//        [Tooltip("The force multiplier")]
//        public float VelocityMultiplier = 1.0f;

//        [Tooltip("Rotate if using ApplyBetween?")]
//        public bool RotateToVelocity;

//        [System.NonSerialized]
//        private Rigidbody cachedBody;

//        //[HideInInspector]
//        public bool canJump;

//        //StickyManager stickyManager;

//        private void Awake()
//        {
//            Camera = GameObject.Find("Main Camera").gameObject.GetComponent<Camera>();
//            gameManager = GameObject.Find("GameManager").gameObject.GetComponent<GameManager>();
//        }

//        public void Apply(Vector2 screenDelta)
//        {
//            // Make sure the camera exists
//            var camera = LeanTouch.GetCamera(Camera, gameObject);

//            if (camera != null)
//            {
//                var oldPoint = transform.position;
//                var screenPoint = camera.WorldToScreenPoint(oldPoint);

//                screenPoint.x += screenDelta.x;
//                screenPoint.y += screenDelta.y;

//                var newPoint = camera.ScreenToWorldPoint(screenPoint);

//                //ApplyBetween(oldPoint, newPoint);
//                //print("apply");
//            }
//        }

//        protected virtual void OnEnable()
//        {
//            cachedBody = GetComponent<Rigidbody>();
//        }

//        // 
//        public void ApplyToOpposite(Vector3 end)
//        {
//            //print("gameManager.playerStuck: " + gameManager.playerStuck + ", canJump: " + canJump);

//            if (canJump)
//            {

//                // re-enable gravity for player
//                if (gameManager.playerStuck)
//                {
//                    gameManager.playerRb.isKinematic = false;
//                    gameManager.playerRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
//                    gameManager.playerRb.useGravity = true;
//                    VelocityMultiplier = 2.3f;
//                    //StickyManager.Instance.DelayUnfreeze();


//                    gameManager.playerStuck = false;
//                }

//                ApplyBetweenOpposite(transform.position, end);
//            }
//        }

//        // Pull back on cube to fling the opposite direction
//        public void ApplyBetweenOpposite(Vector3 start, Vector3 end)
//        {
//            var direction = end - start;
//            var angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
//            var forceMode = UseMass == true ? ForceMode.Impulse : ForceMode.VelocityChange;

//            if (RotateToVelocity == true)
//            {
//                transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);
//            }

//            cachedBody.AddForce(-direction * VelocityMultiplier, forceMode);

//            // change players facing direction
//            GameManager.Instance.PlayerFaceDirection(direction.x > 0);

//            // count jumps
//            GameManager.Instance.PlayerJumped();

//        }


//    }
//}