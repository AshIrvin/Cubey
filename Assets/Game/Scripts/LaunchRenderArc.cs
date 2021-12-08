﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

[RequireComponent(typeof(LineRenderer))]
public class LaunchRenderArc : MonoBehaviour
{
    
    [SerializeField] private LeanForceRigidbody leanForce;
    [SerializeField] private BoolGlobalVariable launchArc;
    
    [Header("Floats")]
    public float velocity;
    public float angle;
    [SerializeField] private float leanAngle;
    [SerializeField] private float extraPower = 1;
    [SerializeField] private float mouseOffset;
    [SerializeField] private float maxVelocity = 10.5f;
    [SerializeField] private float arcOffset = 0f;
    [SerializeField] private float distance = 1;

    [Header("Other")]
    [SerializeField] private int resolution = 15;
    [SerializeField] private Vector3 groundOffset;
    [SerializeField] private Vector3 mousePos;
    [SerializeField] public Vector3 leanEndPos;
    [SerializeField] private Vector3 direction;
    [SerializeField] private bool renderArcAllowed;
    [SerializeField] private GameObject cubey;

    [Header("Sprites")]
    [SerializeField] private Sprite spriteImage;
    [SerializeField] private List<GameObject> dotSprites = new List<GameObject>();
    [SerializeField] private GameObject dottedLineArcGroup;
    [SerializeField] private Color[] spriteColours;

    private LineRenderer lr;
    private List<LeanTouch> lt;
    private float g;
    private float radianAngle;
    
    public float timeToFade = 2f;
    
    private void Awake()
    {
        launchArc.OnValueChanged += EnableLaunchArc;
        
        lr = GetComponent<LineRenderer>();
        g = Mathf.Abs(Physics2D.gravity.y);
        cubey = transform.parent.gameObject;
        leanForce = transform.GetComponentInParent<LeanForceRigidbody>();
        
        dotSprites.Clear();
        var arc = Instantiate(dottedLineArcGroup);
        arc.SetActive(true);
        
        for (int i = 0; i < arc.transform.childCount; i++)
        {
            dotSprites.Add(arc.transform.GetChild(i).gameObject);
        }
        
        spriteColours = new Color[dotSprites.Count];
        for (int i = 0; i < dotSprites.Count; i++)
        {
            spriteColours[i] = dotSprites[i].GetComponent<SpriteRenderer>().color;
        }
        
        EnableArc(false);
    }


    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && renderArcAllowed)
            RenderArc();
        else if (Input.GetMouseButtonUp(0) && spriteColours[0].a > 0)
            StartCoroutine(DelayFadeArc());
    }

    private void OnDestroy()
    {
        launchArc.OnValueChanged -= EnableLaunchArc;
    }

    private void EnableLaunchArc(bool on)
    {
        renderArcAllowed = on;
        if (!on)
            EnableArc(false);
        
        // Debug.Log("launch arc set to: " + on);
    }
    
    private void RenderArc()
    {
        ResetArc();
        
        if (!lr.enabled)
            lr.enabled = false;

        if (!dottedLineArcGroup.activeSelf)
            dottedLineArcGroup.SetActive(true);

        lr.positionCount = resolution + 1;
        SpawnArcSprites(CalculateArcArray());

        leanAngle = leanForce.angle;
        
        // Debug.Log("Rendering Arc");
    }

    IEnumerator DelayFadeArc()
    {
        allowFade = true;
        yield return new WaitForSeconds(1);
        StartCoroutine(FadeArc(0.5f));
    }

    private bool allowFade;
    
    IEnumerator FadeArc(float fadeSpeed)
    {
        if (!allowFade) yield break;
        
        while (spriteColours[0].a > 0)
        {
            for (int i = 0; i < spriteColours.Length; i++)
            {
                float alpha = spriteColours[i].a - (fadeSpeed * Time.deltaTime);
                spriteColours[i] = new Color(spriteColours[i].r, spriteColours[i].g, spriteColours[i].b, alpha);
            }

            AssignToSprite();
            yield return null;
        }
    }

    private void AssignToSprite()
    {
        for (int i = 0; i < dotSprites.Count; i++)
        {
            dotSprites[i].GetComponent<SpriteRenderer>().color = spriteColours[i];
        }
    }

    private void ResetArc()
    {
        allowFade = false;
        
        LoopSpriteColours(1);
    }
    
    private void SpawnArcSprites(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length-1; i++)
        {
            dotSprites[i].transform.position = positions[i];
        }
    }

    public void EnableArc(bool enable)
    {
        // Debug.Log("Enable Arc " + enable);
        lr.positionCount = 0;
        dottedLineArcGroup.SetActive(enable);
        
        if (!enable)
        {
            // Debug.Log("Settings arc to 0 alpha");
            LoopSpriteColours(0);
        }
    }

    private void LoopSpriteColours(float alpha)
    {
        for (int i = 0; i < spriteColours.Length; i++)
        {
            spriteColours[i] = new Color(spriteColours[i].r, spriteColours[i].g, spriteColours[i].b, alpha);
        }
        AssignToSprite();
    }

    private Vector3 GetMousePoint()
    {
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        mousePos = castPoint.origin + (castPoint.direction * distance);

        return mousePos;
    }

    private float CalculateMouseAngle()
    {
        var startPosition = cubey.transform.position;

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

    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        angle = CalculateMouseAngle();

        radianAngle = Mathf.Deg2Rad * angle;

        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;

        arcArray[0] = cubey.transform.position + groundOffset;

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance) + arcArray[0]; 
        }

        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);
    }

}
