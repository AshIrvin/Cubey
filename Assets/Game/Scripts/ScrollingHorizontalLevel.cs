using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts
{
    public class ScrollingHorizontalLevel : MonoBehaviour
    {
        public static ScrollingHorizontalLevel Instance { get; set; }

        public List<GameObject> platforms;
        public List<GameObject> spawnedPlatforms;
        //public GameObject platformVert;

        public float time = 3f;
        //public float distanceBetweenPlatforms = 0.1f;
        public float countdown;

        public float speed = 0.5f;

        public int maxPlatforms = 4;
        int prevPos;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            //GameManager.Instance.allowFlight |= gameObject.activeInHierarchy;

            //GameManager.Instance.LoadLevelPresets();

            /*if (gameObject.activeInHierarchy || GameManager.Instance.levelNo == 5)
            {
                GameManager.Instance.allowFlight = true;
                print("ALLOW FLIGHT: " + GameManager.Instance.allowFlight + ", level: " + GameManager.Instance.levelNo);
                GameManager.Instance.playerRb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezePositionZ;
                GameManager.Instance.deathWalls.SetActive(true);
                CameraStatic();
            }*/

            countdown = time;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (gameObject.activeInHierarchy)
            {
                countdown -= Time.deltaTime;

                if (countdown < 0 && spawnedPlatforms.Count < maxPlatforms)
                {
                    SpawnRandomPlatform();
                    countdown = time;
                }

                if (spawnedPlatforms.Count > 0)
                {

                    for (int i = 0; i < spawnedPlatforms.Count; i++)
                    {
                        spawnedPlatforms[i].transform.Translate(Vector3.left * Time.deltaTime * speed, Space.World);

                        if (spawnedPlatforms[i].transform.position.x < (-8)){
                            Destroy(spawnedPlatforms[i].gameObject);
                            spawnedPlatforms.RemoveAt(i);
                        }
                    }
                }

            }
        }

        void CameraStatic()
        {
            GameObject cam = GameObject.Find("Camera");
            var xPos = cam.transform.position;
            xPos.x = 0;
            cam.transform.position = xPos;
        }

        void SpawnRandomPlatform()
        {
            GameObject go = Instantiate(RandomChoosePlatform(), RandomChoosePosition(), Quaternion.identity, gameObject.transform.Find("platforms"));
            spawnedPlatforms.Add(go);
            go.transform.Translate(Vector3.left * Time.deltaTime * speed);
            go.SetActive(true);

        }

        GameObject RandomChoosePlatform()
        {
            GameObject platform = platforms[(int)GetRandomNumber(0, platforms.Count-1)].gameObject;
            //GameObject platform = platforms[0].gameObject;


            return platform;
        }

        Vector3 RandomChoosePosition()
        {
            // choose position off the screen from a min of top of screen to a max of bottom of screen
            Vector3 pos = new Vector3(6, RandomIntExcept(-1, 12, prevPos), 0);

            /*
            if (spawnedPlatforms.Count == 0)
                return pos;

            float minDist = Mathf.Infinity;
            var closestPos = pos;

            bool allow = false;
            //do {
                //bool farAway = false;

                print("spawnedPlatforms.Count: " + spawnedPlatforms.Count);

                // get all platforms
                for (int i = 0; i < spawnedPlatforms.Count; i++)
                {
                    var currentPos = spawnedPlatforms[i].transform.position;
                    var dist = Vector3.Distance(currentPos, closestPos);
                    // get closest to new pos position
                    if (dist < minDist)
                    {
                        closestPos = currentPos;
                        minDist = dist;
                        print("1.minDist: " + minDist);
                    }

                }
                print("minDist: " + minDist);
            // check distance and spawn beyond n
            //if (Vector3.Distance(closestPos, pos) < 0.05f ||
            //} while
            if ((Mathf.Abs(closestPos.y) - Mathf.Abs(pos.y) < 2.5f) &&
               (Mathf.Abs(closestPos.x) - Mathf.Abs(pos.x) < 3))
            {
                //RandomChoosePosition();
                allow = false;
            }
            else
                allow = true;

            if (allow)
            {
                Debug.DrawLine(closestPos, pos, Color.yellow, 0.5f);
                return pos;
            }

                */
            return pos;
        }

        float GetRandomNumber(float min, float max)
        {
            float randomNumber = Random.Range(min, max);
            return randomNumber;
        }

        int RandomIntExcept(int min, int max, int except)
        {
            int result = Random.Range(min, max - 1);

            if (result == except || result == (except+1) || result == (except-1))
            {
                result += 2;
            }

            return result;
        }

    }
}