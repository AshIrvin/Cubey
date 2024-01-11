﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using Lean.Touch;

public class EyesLookingManager : MonoBehaviour
{
    [SerializeField] private PointsOfInterestManager pointsOfInterestManager;
    [SerializeField] private LeanForceRigidbodyCustom leanForceRigidbodyCustom;
    [SerializeField] private LaunchRenderArc launchRenderArc;


    [SerializeField] private float eyeXmin;
    [SerializeField] private float eyeXmax;
    [SerializeField] private float eyeYmin;
    [SerializeField] private float eyeYmax;
    [SerializeField] private float eyeTimeMovementMin;
    [SerializeField] private float eyeTimeMovementMax;
    [SerializeField] private float eyeMovementSpeed;

    [SerializeField] private GameObject playerTarget;
    [SerializeField] private GameObject pupils;

    [SerializeField] private GameObject pupilCenterTarget;
    [SerializeField] private List<Vector3> pointsOfInterestList;
    [SerializeField] private float dist;
    [SerializeField] private bool isCubey;

    //private Vector3 _target;
    //private Vector2 currentTarget;
    //private Vector2 newTarget;
    //private Vector2 resetEyes;
    private int randomNo;
    private float randomEyeMovement;
    private float randomEyeReset;
    //private bool eyesMoved;
    private string pointOfInterest;
    //private GameObject randomPoint;
    private Vector3 defaultPos;
    private Vector3 target;
    private Camera cam;


    private void Awake()
    {
        pointsOfInterestManager = FindFirstObjectByType<PointsOfInterestManager>();

        FingerPos.belowCubey += SubEyesMovement;
    }

    private void OnDestroy()
    {
        FingerPos.belowCubey -= SubEyesMovement;
    }

    private void Start()
    {
        if (cam == null)
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        AssignPointOfInterest();
        CheckPupilObjects();
        FindPointsOfInterests();
        SetEyeDelay();
        
        if (!gameObject.CompareTag("Player") && playerTarget == null)
            playerTarget = GameObject.FindWithTag("Player").gameObject;

        target = RandomlyChoosePoint();
        
        if (target != null)
            EyesMovement(target);

        // showPupilMovement = false;

        if (playerTarget == null)
        {
            Logger.Instance.ShowDebugLog("Missing Cubey gameobject");
            return;
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy || playerTarget == null)
            return;
        
        if (isCubey && randomEyeMovement < 0)
        {
            EyesMovement(RandomlyChoosePoint());
        }
        
        if (!isCubey && randomEyeMovement < 0)
        {
            EyesMovement(playerTarget.transform.position);
            SetEyeDelay();
        }

        if (randomEyeReset < 0)
        {
            StartCoroutine(ResetEyes(UnityEngine.Random.Range(1, 4)));
        }
    }

    private void LateUpdate()
    {
        randomEyeMovement -= Time.deltaTime;
        randomEyeReset -= Time.deltaTime;
    }

    private void AssignPointOfInterest()
    {
        if (gameObject.CompareTag("Player"))
        {
            pointOfInterest = "PointOfInterest";
        }
        else
        {
            pointOfInterest = "Player";
        }
    }

    private void CheckPupilObjects()
    {
        if (pupils == null)
            pupils = transform.Find("Pupils").gameObject;

        if (pupilCenterTarget == null)
            pupilCenterTarget = transform.Find("EyeCenter").gameObject;

        if (pupilCenterTarget != null)
            defaultPos = pupilCenterTarget.transform.localPosition;

    }

    IEnumerator ResetEyes(float delay)
    {
        if (defaultPos != Vector3.zero)
            pupils.transform.localPosition = defaultPos;

        yield return new WaitForSeconds(delay);
        RandomEyeReset();
    }

    private void FindPointsOfInterests()
    {
        pointsOfInterestList.Clear();

         var listOfPositions = GameObject.FindGameObjectsWithTag(pointOfInterest);
         foreach (var p in listOfPositions)
         {
             pointsOfInterestList.Add(p.transform.position);
         }
    }

    private void SetEyeDelay()
    {
        randomEyeMovement = UnityEngine.Random.Range(eyeTimeMovementMin, eyeTimeMovementMax);
    }

    private void RandomEyeReset(){
        randomEyeReset = UnityEngine.Random.Range(2, 6);
    }

    private void SubEyesMovement()
    {
        if (isCubey && gameObject.activeInHierarchy)
        {
            EyesMovement(GetPositionFromAngle());
        }
    }
    
    private void EyesMovement(Vector3 newTarget)
    {
        MovePupils(newTarget);
        
        Debug.DrawLine(pupilCenterTarget.transform.position, newTarget, Color.yellow, 1f);
    }

    private void MovePupils(Vector3 newTarget)
    {
        Vector3 target = new Vector3(MoveEyesAlongX(newTarget.x), MoveEyesAlongY(newTarget.y), 0);
        Vector3 localTarget = transform.InverseTransformPoint(target);
        localTarget.z = 0;
        pupils.transform.localPosition = Vector3.MoveTowards(pupilCenterTarget.transform.localPosition, localTarget, dist);
    }

    private float MoveEyesAlongX(float posX)
    {
        return Mathf.Clamp(posX, pupilCenterTarget.transform.position.x + eyeXmin, pupilCenterTarget.transform.position.x + eyeXmax);
    }

    private float MoveEyesAlongY(float posY)
    {
        return Mathf.Clamp(posY, pupilCenterTarget.transform.position.y + eyeYmin, pupilCenterTarget.transform.position.y + eyeYmax);
    }

    private int ChooseRandomInterest()
    {
        randomNo = UnityEngine.Random.Range(0, pointsOfInterestList.Count);
        return randomNo;
    }

    private Vector3 RandomlyChoosePoint()
    {
        if (pointsOfInterestList.Count == 0) 
            return defaultPos;
        
        var n = ChooseRandomInterest();
    
        if (pointsOfInterestList.Count > 0 && pointsOfInterestList[n] != null)
        {
            SetEyeDelay();
            return pointsOfInterestList[n];
        }
        return defaultPos;
    }

    private Vector3 GetPositionFromAngle()
    {
        return launchRenderArc.firstPosition;
    }
}

