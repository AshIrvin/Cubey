using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Game.Scripts
{
    public class VisualEffects : MonoBehaviour
    {
        public static VisualEffects Instance { get; private set; }

        #region Public
        
        [Header("Scripts")]
        public AudioManager audioManager;

        [Header("Particle Effects")]
        public ParticleSystem pePowerJump;
        public ParticleSystem peLandingDust;
        public ParticleSystem pePlayerTrail;
        public ParticleSystem peExitStars;
        public ParticleSystem peAirBoom;
        public ParticleSystem peHintArrow;
        public ParticleSystem peSnow;
        public ParticleSystem peSnowClose;
        public ParticleSystem peLeaves;
        public ParticleSystem peSweetPickup;
        public ParticleSystem pePlatformDust;
        public ParticleSystem pePlatformExplode1;
        public ParticleSystem pePlatformExplode2;
        public ParticleSystem peExitOpened;
        public ParticleSystem peExitExplosion;
        public ParticleSystem peNewLevel;

        public GameObject peExitSwirl;

        [Header("Used?")]
        public LaunchRenderArc launchRenderArc;

        [Header("Players Object")]
        public GameObject player;

        [Header("Floats")]
        public float spawnPeBlastDist;
        public float timer = 2;
        public float peSpeed;
        public float angle;
        public float powerLength;

        [Header("Bools")]
        public bool powerTextEnable;

        public bool enableSizeIncrease;
        public bool enableColour;
        //public bool hintArrow;
        //public bool powerJump;
        
        public Text powerText;
        private Scene currentScene;

        #endregion


        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start() {

            currentScene = SceneManager.GetActiveScene();

            if (currentScene.name != "MainMenu")
            {
                if (player == null)
                    player = GameObject.Find("PlayerCharacter");

                if (powerText != null)
                    powerText.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (currentScene.name != null && currentScene.name != "MainMenu")
            {
                BlastOffEffect();

                if (powerTextEnable && !GameManager.Instance.allowFlight)
                {
                    timer -= Time.fixedDeltaTime;

                    var alpha = powerText.color.a;
                    powerText.color = new Color(1, 0.948f, 0, Mathf.Lerp(alpha, 0, Time.fixedDeltaTime));
                    //powerText.color = new Color(1, 0, 0, Mathf.Lerp(alpha, 0, Time.fixedDeltaTime));

                    if (timer < 0)
                    {
                        timer = 2;
                        powerText.text = "";
                        //powerTextEnable = false;
                    }
                }
            }
        }

        private void BlastOffEffect()
        {
            if (Input.GetMouseButtonUp(0))
            {
                CancelPe(pePowerJump);
            }

            if (Input.GetMouseButton(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Todo - needs fixed - cam to button erroring out
                var target = player.transform.position;
                target.z -= 0.2f;
                var distance = Vector3.Distance(target, Camera.main.transform.position);
                var startPoint = ray.GetPoint(distance);

                // Todo - fix start point to keep pe on z 0
                startPoint.z = angle;

                if (Physics.Raycast(ray) && startPoint.y < target.y)
                {
                    audioManager.PlayAudio(audioManager.cubeyPowerUp);

                    var peMain = pePowerJump.main;
                    peMain.loop = true;
                    
                    var peEmit = pePowerJump.emission;
                    peEmit.enabled = true;

                    var direction = target - startPoint;

                    pePowerJump.Play();

                    Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
                    pePowerJump.transform.rotation = rotation;

                    Vector3 offset = startPoint - target;
                    pePowerJump.transform.position = target + Vector3.ClampMagnitude(offset, spawnPeBlastDist);


                    var pePlayerDist = Vector3.Distance(target, pePowerJump.transform.position);

                    //peMain.startLifetime = pePlayerDist/6;
                    peMain.startSpeed = pePlayerDist * peSpeed;
                    if (enableSizeIncrease)
                        peMain.startSize = pePlayerDist / powerLength;

                    if (enableColour)
                    {
                        peMain.startColor = Color.Lerp(new Color(0.9f, 1f, 0f), new Color(1f, 0.1f, 0f), pePlayerDist/ powerLength);
                        
                    }
                    //peMain.startColor.colorMin = Color.Lerp(Color.yellow, Color.red, pePlayerDist / 6);

                    if (!GameManager.Instance.allowFlight && powerTextEnable)
                    {
                        powerText.gameObject.SetActive(true);
                        powerText.text = pePlayerDist.ToString("0.0");
                        powerText.transform.position = startPoint;
                        powerText.color = new Color(1, 0.948f, 0, 1);
                        //powerText.color = new Color(1, 0, 0, 1);
                    }

                    // Hint Arrow - put it here to get better angle etc
                    //launchRenderArc.velocity = -direction.y * 2.6f * launchRenderArc.extraPower;

                    //launchRenderArc.angle = Mathf.Abs(angle + launchRenderArc.fixAngle * launchRenderArc.multipleAngle);


                    //PlayEffect(peHintArrow, player.transform.position);
                    //if (hintArrow)
                    //{
                    //    var peMain2 = peHintArrow.main;
                    //    peMain2.loop = true;
                    //    var peEmit2 = peHintArrow.emission;
                    //    peEmit2.enabled = true;

                    //    peHintArrow.gameObject.transform.position = player.transform.position;
                    //    peHintArrow.Play();

                    //    peMain2.startSpeed = pePlayerDist * peSpeed;
                    //    //rotation = new Quaternion(rotation.x, 90, rotation.z, rotation.w);
                    //    peHintArrow.transform.rotation = rotation;
                    //}

                }
            }
            else
            {
                CancelPe(pePowerJump);
                audioManager.StopAudio(audioManager.cubeyPowerUp);
            }

        }

        private void CancelPe(ParticleSystem effect)
        {
            var peMain = effect.main;
            peMain.loop = false;

            var peEmit = effect.emission;
            peEmit.enabled = false;

            //powerTextEnable = true;
            timer = 2;
        }

        public void LandingDust()
        {
            // find out when player lands
            // velocity hits 0

            peLandingDust.gameObject.SetActive(true);
            //peLandingDust.Clear();
            peLandingDust.Play();
            peLandingDust.gameObject.transform.position = player.transform.position;
        }

        public void PlayerTrail()
        {
            pePlayerTrail.gameObject.SetActive(true);
            pePlayerTrail.Play();
            pePlayerTrail.gameObject.transform.position = player.transform.position;
        }

        public void ExitCompletion()
        {
            var exitGo = GameObject.Find("Exit");
            peExitStars.transform.position = exitGo.transform.position;
            peExitStars.Play();
        }

        public void PlayEffect(ParticleSystem effect, Vector3 pos)
        {
            //var pe = effect.main;
            effect.gameObject.transform.position = pos;
            //effect.Stop();
            effect.Play();
        }

        public void PlayEffectOverScreen(ParticleSystem effect)
        {
            effect.gameObject.SetActive(true);
            //var pe = effect.main;
            //effect.gameObject.transform.position = pos;
            //effect.Stop();
            effect.Play();
        }


    }
}