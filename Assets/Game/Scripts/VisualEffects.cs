using UnityEngine;

public class VisualEffects : MonoBehaviour
{
    public static VisualEffects Instance { get; private set; }

    [Header("Scripts")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject particleEffectsGroup;

    [Header("Particle Effects")] // keep as public
    public ParticleSystem pePowerJump;
    public ParticleSystem pePowerDust;
    public ParticleSystem peAirBoom;
    public ParticleSystem peCloudPoof;
    public ParticleSystem peExitStars;
    public ParticleSystem peExitOpened;
    public ParticleSystem peExitExplosion;
    public ParticleSystem peGravityImplode;
    public ParticleSystem peGravityExplode;
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

    [Header("Bools")]
    public bool enableSizeIncrease;
    public bool enableColour;

    public float powerDustOffset = 0.1f;
    public float powerDustDistanceOffset = 2f;

    public GameObject ParticleEffectsGroup
    {
        get => particleEffectsGroup;
        set => particleEffectsGroup.SetActive(value);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        FingerPos.belowCubey += PowerDustEffect;
        // FingerPos.aboveCubey += StopDustEffect;
    }

    private void OnDestroy()
    {
        FingerPos.belowCubey -= PowerDustEffect;
        // FingerPos.aboveCubey -= StopDustEffect;
    }

    private void LateUpdate()
    {
        if (pePowerDust.isPlaying && Input.GetMouseButtonUp(0))
        {
            StopEffect(pePowerDust);
        }
    }
    
    private void PowerDustEffect()
    {
        if (!gameManager.enabled || Time.timeScale < 0.5f || !gameManager.allowPlayerMovement)
            return;
        
        var playerPos = gameManager.CubeyPlayer.transform.position;
        playerPos.y += powerDustOffset;
        PlayEffectOnceUpdatePos(pePowerDust, playerPos);
        var pePlayerDist = Vector3.Distance(playerPos, FingerPos.FingerPosition);
        var peMain = pePowerDust.main;
        peMain.startSpeed =pePlayerDist * powerDustDistanceOffset;
    }
    
    public void StopEffect(ParticleSystem effect)
    {
        effect.Stop(true);
        effect.Clear(true);
    }

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
    
    public void PlayEffectOnceUpdatePos(ParticleSystem effect, Vector3 pos)
    {
        if (!effect.isPlaying)
        {
            effect.gameObject.transform.position = pos;
            effect.gameObject.SetActive(true);
            effect.Play();
        }
        else
        {
            effect.gameObject.transform.position = pos;
        }
    }
}
