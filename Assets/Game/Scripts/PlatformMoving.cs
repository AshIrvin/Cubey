using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformMoving : PlatformBase
{
    private void Start()
    {
        startPos = transform.position;
        startPos.x += startXOffset;
        startPos.y += startYOffset;

        targetScale = Vector3.zero;
        
        targetPos = new Vector3(startPos.x + horPlatformDistance, startPos.y + vertPlatformDistance);
        
        platformRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (allowPlatformMovement)
            MovePlatform();
    }

    private void MovePlatform()
    {
        if (!allowPlatformMovement) return;

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

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            other.transform.parent = transform;
        }
    }
    
    // Player cant jump in the air
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.parent = null;
            playerCol = null;
        }
    }
}