using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class PointOfInterest : MonoBehaviour
{
    // currently for the Alien eyes only
    [SerializeField] private Transform transformToMove;
    [SerializeField] private Transform[] pointOfInterests;

    [SerializeField] private float eyeSpeed = 12f;
    [SerializeField] private float minTime = 2;
    [SerializeField] private float maxTime = 5;
    [SerializeField] private bool timerEnabled;
    [SerializeField] private float countdown;
    [SerializeField] private Vector3 newPos;
    [SerializeField] private Vector3 localPos;

    private float minEyeSpeed = 10;
    private float maxEyeSpeed = 20;

    public bool objectMoving;
    
    private void Start()
    {
        GetNewPosition();
        countdown = GetRandomNumber(0, 2);
        eyeSpeed = GetRandomNumber(minEyeSpeed, maxEyeSpeed);
    }

    private void LateUpdate()
    {
        if (timerEnabled)
            CountdownTimer();
    }

    private void CountdownTimer()
    {
        countdown -= Time.deltaTime;

        if (countdown < 0)
        {
            // MoveObject();
            StartCoroutine(MoveObject());
        }
    }

    private IEnumerator MoveObject()
    {
        if (transformToMove == null || objectMoving)
        {
            yield break;
        }

        objectMoving = true;
        
        transformToMove.DOLocalMove(localPos, eyeSpeed).SetEase(Ease.InOutBack).onComplete = () =>
        {
            countdown = GetRandomNumber(minTime, maxTime);
            GetNewPosition();
            eyeSpeed = GetRandomNumber(minEyeSpeed, maxEyeSpeed);
            objectMoving = false;
        };
    }

    private float GetRandomNumber(float min, float max)
    {
        return Random.Range(min, max);
    }

    private void GetNewPosition()
    {
        localPos = pointOfInterests[Random.Range(0, pointOfInterests.Length)].localPosition;
    }
}
