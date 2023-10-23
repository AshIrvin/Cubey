using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Lean.Touch;
//using Mono.CompilerServices.SymbolWriter;

[RequireComponent(typeof(LineRenderer))]
public class LaunchRenderArc : MonoBehaviour
{
    [SerializeField] private LeanForceRigidbodyCustom leanForce;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BoolGlobalVariable launchArc;
    
    public float extraPower = 2f; // controls the distance of Cubey's power
    public static float DefaultExtraPower = 2f; 
    
    [Header("Public Floats")]
    public float velocity;
    public float angle;

    [Header("Floats")]
    [SerializeField] private float mouseOffset = 1;
    [SerializeField] private float maxVelocity = 10.5f;
    [SerializeField] private float arcOffset = 0f;
    [SerializeField] private float distance = 18;

    [Header("Other")]
    [SerializeField] private int resolution = 13;
    [SerializeField] private Vector3 groundOffset;
    [SerializeField] private Vector3 mousePos;
    public Vector3 leanEndPos;
    [SerializeField] private Vector3 direction;
    //[SerializeField] private bool renderArcAllowed;
    [SerializeField] private GameObject cubey;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteImage;

    [SerializeField] private List<GameObject> dotSprites = new List<GameObject>();
    [SerializeField] private GameObject dottedLineArcGroup;
    // [SerializeField] private Color[] spriteColours;

    private float leanAngle;
    private LineRenderer lr;
    private List<LeanTouch> lt;
    private float g;
    private float radianAngle;
    private bool allowFade;

    public float timeToFade = 2f;
    public Vector3 firstPosition;
    
    [SerializeField] private Vector3[] arcArray;
    private CanvasGroup canvasGroup;

    private Tween tween;
    private float fadeSpeed = 1f;


    private void Awake()
    {
        launchArc.OnValueChanged += EnableLaunchArc;
        FingerPos.belowCubey += DelayBeforeRenderArc;
        MapManager.MapOpened += SubDisableArc;
    }

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        g = Mathf.Abs(Physics2D.gravity.y);
        cubey = transform.parent.gameObject;
        
        if (leanForce == null)
        {
            leanForce = transform.GetComponentInParent<LeanForceRigidbodyCustom>();
        }
        
        dotSprites.Clear();
        var arc = Instantiate(dottedLineArcGroup);
        arc.SetActive(true);
        
        for (int i = 0; i < arc.transform.childCount; i++)
        {
            dotSprites.Add(arc.transform.GetChild(i).gameObject);
        }

        resolution = dotSprites.Count;

        canvasGroup = arc.GetComponent<CanvasGroup>();

        EnableLaunchArc(false);
    }

    private void OnEnable()
    {
        SubDisableArc();
    }

    private void OnDestroy()
    {
        launchArc.OnValueChanged -= EnableLaunchArc;
        FingerPos.belowCubey -= DelayBeforeRenderArc;
        MapManager.MapOpened -= SubDisableArc;
    }

    private void OnDisable()
    {
        SubDisableArc();
    }

    private void DelayBeforeRenderArc()
    {
        if (Time.timeScale < 0.2f)
        {
            return;
        }
        
        if (launchArc.CurrentValue)
        {
            RenderArc();
        }
    }

    private void EnableLaunchArc(bool on)
    {
        if (!gameObject.activeInHierarchy) return;
            
        //renderArcAllowed = on;
        gameManager.LaunchArc = on;
        if (!on)
        {
            EnableArc(false);
        }
    }

    private void RenderArc()
    {
        ResetArc();
        
        if (!lr.enabled)
        {
            lr.enabled = false;
        }

        if (!dottedLineArcGroup.activeSelf)
        {
            dottedLineArcGroup.SetActive(true);
        }

        lr.positionCount = resolution + 1;
        PositionArcSprites(CalculateArcArray());

        leanAngle = leanForce.Angle;
    }

    private IEnumerator DelayFadeArc()
    {
        allowFade = true;
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeArc());
    }

    private IEnumerator FadeArc()
    {
        if (!allowFade) yield break;
        tween.Kill();
        tween = DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, fadeSpeed);

        yield return null;
    }

    private void ResetArc()
    {
        allowFade = false;
        
        SetCanvasAlpha(1);
    }

    private void PositionArcSprites(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length-1; i++)
        {
            dotSprites[i].transform.position = positions[i];
        }
    }

    private void SubDisableArc()
    {
        if (lr != null)
        {
            lr.positionCount = 0;
        }
        SetCanvasAlpha(0);
        dottedLineArcGroup.SetActive(false);
    }
    
    public void EnableArc(bool enable)
    {
        if (lr != null)
        {
            lr.positionCount = 0;
        }

        dottedLineArcGroup.SetActive(enable);
        
        if (!enable)
        {
            StartCoroutine(DelayFadeArc());
        }
    }

    private void SetCanvasAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
        
        if (tween != null)
        {
            tween.Kill();
            tween = null;
        }
    }

    private float CalculateMouseAngle()
    {
        var startPosition = cubey.transform.position;

        // mousePos = FingerPos.FingerPosition;
        mousePos = GetMousePoint();
        velocity = Vector3.Distance(mousePos, startPosition);
        velocity *= extraPower;
        velocity = Mathf.Min(maxVelocity, velocity);
        velocity = Mathf.Max(0, velocity);

        direction = mousePos - startPosition;

        if (direction.sqrMagnitude < 0.1f)
        {
            return 0;
        }

        float angle2 = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle2 < 0) angle2 += 360;

        return angle2;
    }

    private Vector3 GetMousePoint()
    {
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        mousePos = castPoint.origin + (castPoint.direction * distance);

        return mousePos;
    }
    
    private Vector3[] CalculateArcArray()
    {
        arcArray = new Vector3[resolution + 1];
        // firstPosition = Vector3.zero;
        
        angle = CalculateMouseAngle();

        radianAngle = Mathf.Deg2Rad * angle;
        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;
        arcArray[0] = cubey.transform.position + groundOffset;

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance) + arcArray[0]; 
        }

        firstPosition = arcArray[2];
        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);
    }

}
