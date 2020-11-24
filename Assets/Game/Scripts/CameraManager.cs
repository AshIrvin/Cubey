using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Assets.Game.Scripts
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; set; }

        private SaveLoadManager saveLoadManager;

        public StoryManager storyManager;

        Camera cam;
        public Vector3 fixedPos;
        //public Vector3 startCamPos;
		public Vector3 endCamPos;

        public GameObject maps;
        public GameObject player;
        public float speed = 0.1f;
        public float camTime = 1f;
        public float camToLevelTime = 1.4f;
        public float gameCamTime = 1f;//0.3f;
        public float camMapDragTime = 0.3f;
        private Vector3 velocity = Vector3.zero;

        private Vector3 dragOrigin; //Where are we moving?
        private Vector3 clickOrigin = Vector3.zero; //Where are we starting?
        private Vector3 basePos = Vector3.zero; //Where should the camera be initially?

        public float mapClampX;
        public float mapClampY;

        public bool playOnce;
        public bool panToLevel, disableAutoPanMapCam, camSmoothEnd;

        public bool storyMode;

        private void Awake()
		{
			Instance = this;

            if (SceneManager.GetActiveScene().name == "MainMenu")
                saveLoadManager = GameObject.Find("SaveLoadManager").GetComponent<SaveLoadManager>();
		}

        private void Start()
        {
            panToLevel = false;

            fixedPos = new Vector3(0, -3, 0);

            if (SceneManager.GetActiveScene().name == "Game_Levels")
            {
                if (player == null)
                {
                    player = GameObject.FindGameObjectWithTag("Player");
                    player.SetActive(true);
                }

                if (GameObject.Find("Exit") != null)
                {
                    GameObject exit = GameObject.Find("Exit");
                    transform.position = exit.gameObject.transform.position; // moves from exit to player on start
                }
            }

            StartCoroutine(ChangeCamSpeed(0.3f));

            storyMode = false;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu" && !playOnce && !MainMenuManager.Instance.mapActive)
                StartCoroutine(PlayStartCameraSweep());

            if (SceneManager.GetActiveScene().name == "MainMenu" && MainMenuManager.Instance.mapActive && !panToLevel)
                StartCoroutine(PanToLevelButton());


            if (player != null && GameManager.Instance.camMovement)
            {
                // Smoothly move the camera towards that target position
                if (!GameManager.Instance.allowFlight)
                {
                    transform.position = Vector3.SmoothDamp(transform.position, player.transform.position, ref velocity, gameCamTime);
                } else
                {
                    transform.position = Vector3.SmoothDamp(fixedPos, player.transform.position, ref velocity, gameCamTime);
                }

            }
        }

        IEnumerator ChangeCamSpeed(float n)
        {
            yield return new WaitForSeconds(2);
            gameCamTime = n;
        }

        public void ResetCamPosition()
        {
            transform.position = endCamPos;
        }


        Vector3 ClampCam(Vector3 smoothPos)
        {
            var posX = Mathf.Clamp(smoothPos.x, -18, 18);
            var posY = Mathf.Clamp(smoothPos.y, -16, 8);
            smoothPos.x = posX;
            smoothPos.y = posY;

            return smoothPos;
        }

        IEnumerator PlayStartCameraSweep()
        {
            transform.position = Vector3.SmoothDamp(transform.position, endCamPos, ref velocity, camTime);

            yield return new WaitForSeconds(3);

            playOnce = true;
        }

        public Vector3 GetLastLevelButtonPos()
        {
            print("get last level button position");

            var lastLevel = Vector3.zero;

            if (PlayerPrefs.HasKey("lastLevel"))
            {
                var l = PlayerPrefs.GetInt("lastLevel");
                print("last level was: " + l);

                lastLevel = saveLoadManager.levelButtons[l].transform.position;
                //PlayerPrefs.DeleteKey("lastLevel");
                return lastLevel;
            } else
            {
                lastLevel = saveLoadManager.levelButtons[0].transform.position;
            }
            return lastLevel;
        }

        public IEnumerator PanToLevelButton()
        {
            if (!disableAutoPanMapCam && MainMenuManager.Instance.mapActive)
            {
                if (Input.GetMouseButtonDown(0) && !storyMode)
                    disableAutoPanMapCam = true;

                var levelButtons = saveLoadManager.levelButtons;
                var buttonToStartFrom = transform.position;

                Vector3 lastLevelButtonPos = Vector3.zero;
                if (saveLoadManager.levelUnlocked < saveLoadManager.levelButtons.Count)
                {
                    lastLevelButtonPos = saveLoadManager.levelButtons[saveLoadManager.levelUnlocked].transform.position; // nextLevel
                }

                lastLevelButtonPos.z = -0.4f;

                if (saveLoadManager.level == 0)
                {
                    lastLevelButtonPos = levelButtons[0].transform.position;
                    lastLevelButtonPos.z = -0.4f;
                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, lastLevelButtonPos);

                    if (saveLoadManager.levelUnlocked > saveLoadManager.level)
                    {
                        lastLevelButtonPos = saveLoadManager.levelButtons[saveLoadManager.levelUnlocked].transform.position;
                        lastLevelButtonPos.z = -0.4f;
                        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, lastLevelButtonPos);
                    }

                }
                else if (saveLoadManager.level > 0 && saveLoadManager.level < levelButtons.Count)
                {

                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, lastLevelButtonPos);
                }
                else if (saveLoadManager.level == levelButtons.Count)
                {
                    lastLevelButtonPos = levelButtons[levelButtons.Count-1].transform.position;
                    lastLevelButtonPos.z = -0.4f;
                    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peNewLevel, lastLevelButtonPos);
                }

                var pos = Vector3.SmoothDamp(buttonToStartFrom, lastLevelButtonPos, ref velocity, camToLevelTime);

                transform.position = ClampCam(pos);

                yield return new WaitForSeconds(1.5f);
                panToLevel = true;
            }
        }
    }
}