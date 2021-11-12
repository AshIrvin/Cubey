using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyesLookingManager : MonoBehaviour
{
    [SerializeField] private Vector3 defaultPos;
    [SerializeField] private Vector3 pupilPosition;
    
    [SerializeField] private bool showPupilMovement;
    
    [SerializeField] private float eyeXmin;
    [SerializeField] private float eyeXmax;
    [SerializeField] private float eyeYmin;
    [SerializeField] private float eyeYmax;
    [SerializeField] private float eyeTimeMovementMin;
    [SerializeField] private float eyeTimeMovementMax;

    [SerializeField] private GameObject playerTarget;
    [SerializeField] private GameObject pupils;
    [SerializeField] private GameObject pupilCenterTarget;
    [SerializeField] private GameObject[] pointsOfInterestList;
    [SerializeField] private float dist;

    private Vector3 _target;
    private Vector2 currentTarget;
    private Vector2 newTarget;
    private Vector2 resetEyes;
    private int randomNo;
    private float randomEyeMovement;
    private float randomEyeReset;
    private bool eyesMoved;
    private string pointOfInterest;
    private GameObject randomPoint;
    private GameObject target;

    private Camera cam;

    private void Start()
    {
        if (cam == null)
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();

        if (gameObject.CompareTag("Player"))
        {
            pointOfInterest = "PointOfInterest";
        } else {
            pointOfInterest = "Player";
        }

        if (pupils == null)
            pupils = transform.Find("Pupils").gameObject;

        if (pupilCenterTarget == null)
            pupilCenterTarget = transform.Find("Sphere").gameObject;

        FindPointsOfInterests();
        SetEyeDelay();
        // EyesMovement(pointsOfInterestList[0].gameObject.transform.position);

        if (!gameObject.CompareTag("Player") && playerTarget == null)
            playerTarget = GameObject.FindWithTag("Player").gameObject;

        target = RandomlyChoosePoint();
        
        if (target != null)
            EyesMovement(target.transform.position);

        showPupilMovement = false;
    }

    private void Update()
    {
        randomEyeMovement -= Time.deltaTime;

        if (randomEyeMovement < 0)
        {
            if (gameObject.CompareTag("Player")) 
            {
                RandomlyChoosePoint();
            }
            else
            {
                if (playerTarget == null)
                    playerTarget = GameObject.FindWithTag("Player").gameObject;
                if (target != null)
                    EyesMovement(playerTarget.transform.position);
            }

            SetEyeDelay();
        }

        randomEyeReset -= Time.deltaTime;
        
        // set eyes to default pos looking at player
        if (randomEyeReset < 0 )
        {
            pupils.transform.localPosition = defaultPos;
            RandomEyeReset();
            SetEyeDelay();
        }

        // for testing
        if (showPupilMovement)
            pupilPosition = pupils.transform.localPosition;
    }

    // Look for points of interest active when starting level
    private void FindPointsOfInterests()
    {
        if (GameObject.FindWithTag("PointOfInterest").activeSelf)
            pointsOfInterestList = GameObject.FindGameObjectsWithTag("PointOfInterest");
        else
            print("ERROR - no points of interest in level");
    }

    private void SetEyeDelay(){
        randomEyeMovement = UnityEngine.Random.Range(eyeTimeMovementMin, eyeTimeMovementMax);
        RandomlyChoosePoint();
    }

    private void RandomEyeReset(){
        randomEyeReset = UnityEngine.Random.Range(4, 6);
    }

    /* resets to default position after n secs
     * chooses a point of interest
     * eyes move to direction of interest     
    */
    private void EyesMovement(Vector3 newTarget)
    {
        /*
        // reset to localPos before continuing
        pupils.transform.localPosition = defaultPos;

        var targetPos = Vector3.zero;
        if (targetPointsOfInterestList)
        {
            targetPos = targetPointsOfInterestList.transform.position;
            targetPos.z = pupils.transform.localPosition.z;
        }

        // gets distance between clamped target and face
        float dist = Vector3.Distance(pupilCenterTarget.transform.position, targetPos);

        dist = Mathf.Clamp(dist, eyeXmin, eyeXmax);
        dist = Mathf.Clamp(dist, eyeYmin, eyeYmax);

        //
        // float distX = pupilCenterTarget.transform.position.x - targetPos.x;
        // float distY = pupilCenterTarget.transform.position.y - targetPos.y;
        // distX = Mathf.Clamp(distX, eyeXmin, eyeXmax);
        // distY = Mathf.Clamp(distY, eyeYmin, eyeYmax);
        //
        
        pupils.transform.position = Vector3.MoveTowards(pupilCenterTarget.transform.position, targetPos, dist);
        */

        MovePupils(newTarget);
        
        Debug.DrawLine(pupilCenterTarget.transform.position, newTarget, Color.yellow, 1f);
    }

    private void MovePupils(Vector3 newTarget)
    {
        Vector3 target = new Vector3(MoveEyesAlongX(newTarget.x), MoveEyesAlongY(newTarget.y), 0);
        pupils.transform.position = Vector3.MoveTowards(pupilCenterTarget.transform.position, target, dist);
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
        return randomNo = UnityEngine.Random.Range(0, pointsOfInterestList.Length);
    }

    private GameObject RandomlyChoosePoint()
    {
        return pointsOfInterestList[ChooseRandomInterest()];

        // if (target != null)
            // EyesMovement(target.transform.position);
        
        // return pointsOfInterestList[ChooseRandomInterest()];
    }
}

