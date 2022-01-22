using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlatformCloud : PlatformBase
{
    public float playerDragThruCloud = 25f;
    private float cloudScaleOffset = 0.3f;
    public SpriteRenderer sprite;
    public BoxCollider collider;
    private Vector3 defaultColScale;
    
    private void Awake()
    {
        platformRb = GetComponent<Rigidbody>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        collider = GetComponent<BoxCollider>();
        
        
    }

    private void Start()
    {
        startPos = transform.position;
        startPos.x += startXOffset;
        startPos.y += startYOffset;

        targetScale = Vector3.zero;
        
        targetPos = new Vector3(startPos.x + horPlatformDistance, startPos.y + vertPlatformDistance);
        // playerDragThruCloud = 25f;

        defaultColScale = collider.size;
    }

    private void Update()
    {
        MoveCloudPlatform();
    }

    private void MoveCloudPlatform()
    {
        if (moveCloud)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
            
            if (Vector3.Distance(transform.position, targetPos) < distanceOffset)
            {
                StartCoroutine(ScaleCloudPlatform(true));
            }
        }
        else if (Vector3.Distance(transform.position, startPos) < distanceOffset)
        {
            StartCoroutine(ScaleCloudPlatform(false));
        }
    }

    private IEnumerator ScaleCloudPlatform(bool reduceSize)
    {
        bool reachedTargetScale = false;
        sprite.transform.localScale = Vector3.SmoothDamp(sprite.transform.localScale, reduceSize ? Vector3.zero : Vector3.one, 
            ref velocityScale, reduceSize ? smoothScaleDownTime : smoothScaleUpTime);     
        
        collider.size = Vector3.SmoothDamp(collider.size, reduceSize ? Vector3.zero : defaultColScale, 
            ref velocityScale, reduceSize ? smoothScaleDownTime : smoothScaleUpTime);
        
        sprite.transform.localScale = Vector3.SmoothDamp(sprite.transform.localScale, reduceSize ? Vector3.zero : Vector3.one, 
            ref velocityScale, reduceSize ? smoothScaleDownTime : smoothScaleUpTime);
        
        if (sprite.transform.localScale.x < cloudScaleOffset && reduceSize)
        {
            sprite.transform.localScale = collider.size = Vector3.zero;
            reachedTargetScale = true;
        }
        else if (sprite.transform.localScale.x > 0.99f && !reduceSize)
        {
            sprite.transform.localScale = Vector3.one;
            collider.size = defaultColScale;
            reachedTargetScale = true;
        }

        yield return new WaitUntil(() => reachedTargetScale);
        transform.position = startPos;
        moveCloud = false;
        
        if (sprite.transform.localScale.x > 0.8f)
            moveCloud = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !transform.CompareTag("MovingPlatform"))
        {
            other.transform.SetParent(transform, true);
            
            if (playerRb == null)
                playerRb = other.GetComponent<Rigidbody>();
            playerRb.drag = playerDragThruCloud;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = null;

            if (playerRb == null)
                playerRb = other.GetComponent<Rigidbody>();
            playerRb.drag = 0f;
        }
    }
}
