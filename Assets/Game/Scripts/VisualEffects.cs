using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VisualEffects : MonoBehaviour
{
    public static VisualEffects Instance { get; private set; }

    [Header("Scripts")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject particleEffectsGo;

    [Header("Particle Effects")] // keep as public
    public ParticleSystem pePowerJump;
    public ParticleSystem pePowerDust;
    public ParticleSystem peAirBoom;
    public ParticleSystem peCloudPoof;
    public ParticleSystem peExitStars;
    public ParticleSystem peExitOpened;
    public ParticleSystem peExitExplosion;
    public ParticleSystem peHintArrow;
    public ParticleSystem peLandingDust;
    public ParticleSystem peLandingGrass;
    public ParticleSystem peLeaves;
    public ParticleSystem peNewLevel;
    public ParticleSystem pePlayerTrail;
    public ParticleSystem pePlatformIceDust;
    public ParticleSystem pePlatformRockDust;
    public ParticleSystem pePlatformExplode1;
    public ParticleSystem pePlatformExplode2;
    public ParticleSystem pePortalEffects;
    public ParticleSystem pePortalEffectsExit;
    public ParticleSystem peRockBreakage;
    public ParticleSystem peSnow;
    public ParticleSystem peSnowClose;
    public ParticleSystem peSweetPickup;
    public ParticleSystem peWaterSplash;

    [Header("Exit object")]
    public GameObject peExitSwirl;

    [Header("Floats")]
    [SerializeField] private float spawnPeBlastDist = 1;
    [SerializeField] private float timer = 2;
    [SerializeField] private float peSpeed = 1.8f;
    [SerializeField] private float angle = 0;
    [SerializeField] private float powerLength = 7;

    [Header("Bools")]
    public bool enableSizeIncrease;
    public bool enableColour;

    public GameObject ParticleEffectsGo
    {
        get => particleEffectsGo;
        set => particleEffectsGo.SetActive(value);
    }
    
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!gameManager.enabled)
            return;
        
        PowerDustEffect();
    }

    public float powerDustOffset = 0.1f;
    public float powerDustDistanceOffset = 2f;
    
    private void PowerDustEffect()
    {
        /*if (Input.GetMouseButtonUp(0))
        {
            StopEffect(pePowerDust);
        }*/

        if (Input.GetMouseButton(0) && FingerPos.belowPlayer)
        {
            var playerPos = gameManager.CubeyPlayer.transform.position;
            playerPos.y -= powerDustOffset;
            PlayEffect(pePowerDust, playerPos);
            var pePlayerDist = Vector3.Distance(playerPos, FingerPos.FingerPosition);
            
            var peMain = pePowerDust.main;
            peMain.startSpeed =pePlayerDist * powerDustDistanceOffset;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            StopEffect(pePowerDust);
        }
    }
    
    // no longer used
    private void BlastOffEffect()
    {
        if (Input.GetMouseButtonUp(0))
        {
            StopEffect(pePowerJump);
        }

        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var playerPos = gameManager.CubeyPlayer.transform.position;
            playerPos.z -= 0.2f;
            var distance = FingerPos.GetCameraPlayerDistance();
            var fingerPos = FingerPos.FingerPosition;

            // Todo - fix start point to keep PE on z 0
            fingerPos.z = angle;

            if (Physics.Raycast(ray) && FingerPos.belowPlayer)
            {
                audioManager.PlayAudio(audioManager.cubeyPowerUp);

                var peMain = pePowerJump.main;
                peMain.loop = true;
                var peEmit = pePowerJump.emission;
                peEmit.enabled = true;
                var direction = FingerPos.FingerPlayerDirection;
                pePowerJump.Play();

                pePowerJump.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);;
                pePowerJump.transform.position = playerPos + Vector3.ClampMagnitude(direction, spawnPeBlastDist);

                var pePlayerDist = Vector3.Distance(playerPos, pePowerJump.transform.position);
                peMain.startLifetime = pePlayerDist;
                peMain.startSpeed = pePlayerDist * peSpeed;

                if (enableSizeIncrease)
                    peMain.startSize = pePlayerDist / powerLength;

                if (enableColour)
                {
                    peMain.startColor = Color.Lerp(new Color(0.9f, 1f, 0f), new Color(1f, 0.1f, 0f), pePlayerDist/ powerLength);
                    var startColor = peMain.startColor;
                    startColor.colorMin = Color.Lerp(Color.yellow, Color.red, pePlayerDist / 6);;
                    peMain.startColor = startColor;
                }
            }
        }
        else
        {
            StopEffect(pePowerJump);
            audioManager.StopAudio(audioManager.cubeyPowerUp);
        }
    }

    public void StopEffect(ParticleSystem effect)
    {
        effect.Stop(true);
    }

    /*public void LandingDust()
    {
        peLandingDust.gameObject.SetActive(true);
        peLandingDust.Play();
        var pos = gameManager.CubeyPlayer.transform.position;
        pos.y -= powerDustOffset;
        PlayEffect(peLandingDust, pos);
    }*/

    /*public void PlayerTrail()
    {
        pePlayerTrail.gameObject.SetActive(true);
        pePlayerTrail.Play();
        pePlayerTrail.gameObject.transform.position = gameManager.CubeyPlayer.transform.position;
    }*/

    public void ExitCompletion()
    {
        var exitGo = gameManager.GetLevelExit;
        
        peExitStars.transform.position = exitGo.transform.position;
        peExitStars.Play();
    }

    public void PlayEffect(ParticleSystem effect, Vector3 pos)
    {
        effect.gameObject.SetActive(true);
        effect.gameObject.transform.position = pos;
        effect.Play();
    }

    public void PlayEffect(ParticleSystem effect)
    {
        effect.gameObject.SetActive(true);
        effect.Play();
    }

    public void PlayEffect(ParticleSystem effect, Vector3 pos, Quaternion rot)
    {
        effect.gameObject.SetActive(true);
        effect.gameObject.transform.position = pos;
        effect.gameObject.transform.rotation = rot;
        effect.gameObject.transform.localScale = Vector3.one;
        effect.Play();
    }

    /// <summary>
    /// Used for creating a single PE within a loop
    /// </summary>
    public void PlayEffectOnce(ParticleSystem effect, Vector3 pos)
    {
        if (!effect.isPlaying)
        {
            effect.gameObject.SetActive(true);
            effect.gameObject.transform.position = pos;
            effect.Play();
        }
    }
}
