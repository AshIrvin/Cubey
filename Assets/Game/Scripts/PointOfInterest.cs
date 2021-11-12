using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class PointOfInterest : MonoBehaviour
{
    [SerializeField] private Transform transformToMove;
    [SerializeField] private Transform[] pointOfInterests;

    [SerializeField] private float eyeSpeed = 5;
    [SerializeField] private float minTime;
    [SerializeField] private float maxTime;
    [SerializeField] private float distance = 1f;
    
    [SerializeField] private bool timerEnabled;

    private float randomTime = 0.1f;
    private float countdown;
    private Vector3 velocity;
    private Vector3 newPos;
    private bool newPosFound;
    
    private void Update()
    {
        CountdownTimer();
    }

    private void CountdownTimer()
    {
        if (timerEnabled)
        {
            countdown += Time.deltaTime;

            if (!newPosFound)
            {
                ChooseNewPosition();
                newPosFound = true;
            }

            MoveToPosition();
            
            if (Vector3.Distance(transformToMove.position, newPos) < distance)
            {
                randomTime = SetRandomTime();
                if (countdown > randomTime)
                {
                    countdown = 0;
                    newPosFound = false;
                }
            }
        }
    }

    private float SetRandomTime()
    {
        return Random.Range(minTime, maxTime);
    }
    
    private void ChooseNewPosition()
    {
        newPos = pointOfInterests[Random.Range(0, pointOfInterests.Length)].position;
    }
    
    private void MoveToPosition()
    {
        transformToMove.position = Vector3.SmoothDamp(transformToMove.position, newPos, ref velocity, eyeSpeed);
    }
}
