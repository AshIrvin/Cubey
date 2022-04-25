using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UIElements;

public class PlatformCloud : PlatformBase
{
    [SerializeField] private BoolGlobalVariable stickyObject;
    
    [SerializeField] private float playerDragThruCloud = 25f;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private bool allowCloudMovement;
    
    private float cloudScaleOffset = 0.3f;
    private Vector3 defaultColScale;
    
    
    private void Awake()
    {
        platformRb = GetComponent<Rigidbody>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        collider = GetComponentInChildren<BoxCollider>();
    }

    private void Start()
    {
        startPos = transform.position;
        startPos.x += startXOffset;
        startPos.y += startYOffset;

        targetScale = Vector3.zero;
        
        targetPos = new Vector3(startPos.x + horPlatformDistance, startPos.y + vertPlatformDistance);

        defaultColScale = collider.size;
    }

    private void Update()
    {
        MoveCloudPlatform();
    }

    private void MoveCloudPlatform()
    {
        if (!allowCloudMovement) return;

        if (allowPlatformMovement)
        {
            transform.position = Vector3.SmoothDamp(transform.position, movePlatform ? targetPos : startPos, ref velocity,
                smoothTime);

            if (movePlatform)
            {
                if (Vector3.Distance(transform.position, targetPos) < distanceOffset)
                {
                    movePlatform = false;
                }
            }
            else if (Vector3.Distance(transform.position, startPos) < distanceOffset)
            {
                movePlatform = true;
            }
        }
        else
        {
            if (moveRight)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

                if (Vector3.Distance(transform.position, targetPos) < distanceOffset)
                {
                    StartCoroutine(ScaleCollider(true));
                    StartCoroutine(ScaleCloudPlatform(true));
                }
            }
            else if (Vector3.Distance(transform.position, startPos) < distanceOffset)
            {
                StartCoroutine(ScaleCloudPlatform(false));
                StartCoroutine(ScaleCollider(false));
            }
        }
    }

    private IEnumerator ScaleCollider(bool reduceSize)
    {
        collider.size = Vector3.SmoothDamp(collider.size, reduceSize ? Vector3.zero : defaultColScale, 
            ref velocityScale2, reduceSize ? smoothScaleDownTime : smoothScaleUpTime);
        
        yield return null;
    }
    
    private IEnumerator ScaleCloudPlatform(bool reduceSize)
    {
        bool reachedTargetScale = false;
        sprite.transform.localScale = Vector3.SmoothDamp(sprite.transform.localScale, reduceSize ? Vector3.zero : Vector3.one, 
            ref velocityScale, reduceSize ? smoothScaleDownTime : smoothScaleUpTime);     
        
        // reduce the scale of the object
        if (sprite.transform.localScale.x < cloudScaleOffset && reduceSize /*&& collider.size.x < 1.5f*/)
        {
            sprite.transform.localScale = collider.size = Vector3.zero;
            reachedTargetScale = true;
        }
        // increase/return scale
        else if (sprite.transform.localScale.x > 0.99f && !reduceSize /*&& collider.size.x > 3*/)
        {
            sprite.transform.localScale = Vector3.one;
            collider.size = defaultColScale;
            reachedTargetScale = true;
        }

        yield return new WaitUntil(() => reachedTargetScale);

        moveRight = false;

        if (sprite.transform.localScale.x > 0.8f)
            moveRight = true;
        else
        {
            if (playerRb != null)
                playerRb.transform.parent = null;
        }

        transform.position = startPos;
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !transform.CompareTag("MovingPlatform"))
        {
            other.transform.SetParent(transform, true);
            
            if (playerRb == null)
                playerRb = other.GetComponent<Rigidbody>();
            playerRb.drag = playerDragThruCloud;
            
            stickyObject.CurrentValue = true;
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
