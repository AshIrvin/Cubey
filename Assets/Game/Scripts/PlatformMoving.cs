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
        HorizontalPlatforms();
        VerticalPlatforms();
    }

    private void HorizontalPlatforms()
    {
        if (moveCloud)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);

            if (transform.position.x > (targetPos.x - distanceOffset))
            {
                moveCloud = false;
            }
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, startPos, ref velocity, smoothTime);
            if (transform.position.x < (startPos.x + distanceOffset))
            {
                moveCloud = true;
            }
        }
    }

    
    // for moving vertical platforms
    private void VerticalPlatforms()
    {
        if (moveUp)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
            if (transform.position.y > (targetPos.y - distanceOffset))
            {
                moveUp = false;
            }
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, startPos, ref velocity, smoothTime);
            if (transform.position.y < (startPos.y + distanceOffset))
            {
                moveUp = true;
            }
        }

        // checks against start pos on how far to move
        if (transform.position.y > (startPos.y + vertPlatformDistance))
        {
            moveUp = false;
        }
        else if (transform.position.y < startPos.y)
        {
            moveUp = true;
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