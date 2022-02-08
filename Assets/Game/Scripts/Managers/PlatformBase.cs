using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformBase : MonoBehaviour
{
    [Header("Start offsets")]
    [SerializeField] private protected float startXOffset;
    [SerializeField] private protected float startYOffset;
    
    [Header("Distance to target")]
    [SerializeField] private protected float horPlatformDistance;
    [SerializeField] private protected float vertPlatformDistance;
    
    [Header("Speed")]
    [SerializeField] private protected float smoothTime = 5f;
    [SerializeField] private protected float smoothScaleDownTime = 3;
    [SerializeField] private protected float smoothScaleUpTime = 1;

    [Header("Bools")] 
    [SerializeField] private protected bool allowPlatformMovement;
    // [SerializeField] private protected bool moveVertically;
    
    [Header("Distance from object to activate")]
    [SerializeField] private protected float distanceOffset = 5f;
    
    [Header("Other")]
    private protected Vector3 velocity;
    private protected Vector3 velocityScale;
    private protected Vector3 velocityScale2;
    private protected Vector3 targetPos;
    private protected Collider playerCol;
    
    public float peOffset = 1;
    private protected bool moveRight;
    private protected bool movePlatform;
    private protected bool moveUp;

    private protected Rigidbody platformRb;
    private protected Rigidbody playerRb;
    private protected Vector3 startPos;
    private protected Vector3 targetScale;
    
}
