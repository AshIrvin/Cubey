using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EyesLookingManager : MonoBehaviour
{
    public Vector3 defaultPos;
    private Vector3 _target;
    public Vector3 pupilPosition;

    private Vector2 currentTarget, newTarget, resetEyes;

    private int randomNo;

    private bool eyesMoved;
    public bool showPupilMovement;

    public float eyeXmin, eyeXmax, eyeYmin, eyeYmax;
    private float randomEyeMovement, randomEyeReset;
    public float eyeTimeMovementMin, eyeTimeMovementMax;

    private string pointOfInterest;

    public GameObject playerTarget;
    public GameObject pupils, pupilCenterTarget;
    private GameObject randomPoint;
    private GameObject target;
    private GameObject[] pointsOfInterestList;

    private Camera cam;


    void Start()
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

        EyesMovement(pointsOfInterestList[0].gameObject);

        if (!gameObject.CompareTag("Player") && playerTarget == null)
            playerTarget = GameObject.FindWithTag("Player").gameObject;


        target = RandomlyChoosePoint().gameObject;
        if (target != null)
            EyesMovement(target);

        showPupilMovement = false;
    }

    void Update()
    {
        // countdown movement of eyes
        randomEyeMovement -= Time.deltaTime;

        // choose target to point eyes towards
        if (randomEyeMovement < 0)
        {
            if (gameObject.CompareTag("Player")) 
            {
                if (GameObject.FindWithTag("PointOfInterest").activeSelf)
                {
                    target = RandomlyChoosePoint().gameObject;

                    if (target != null)
                        EyesMovement(target);
                }
            }
            else
            {
                if (playerTarget == null)
                    playerTarget = GameObject.FindWithTag("Player").gameObject;
                if (target != null)
                    EyesMovement(playerTarget);
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
    public void FindPointsOfInterests()
    {
        if (GameObject.FindWithTag("PointOfInterest").activeSelf)
            pointsOfInterestList = GameObject.FindGameObjectsWithTag("PointOfInterest");
        else
            print("ERROR - no points of interest in level");
    }

    void SetEyeDelay(){
        randomEyeMovement = UnityEngine.Random.Range(eyeTimeMovementMin, eyeTimeMovementMax);
        FindPointsOfInterests();
    }

    void RandomEyeReset(){
        randomEyeReset = UnityEngine.Random.Range(4, 6);
    }

    /* resets to default position after n secs
     * chooses a point of interest
     * eyes move to direction of interest     
    */
    void EyesMovement(GameObject targetpointsOfInterestList)
    {
        // reset to localPos before continuing
        pupils.transform.localPosition = defaultPos;

        var targetPos = Vector3.zero;
        if (targetpointsOfInterestList)
        {
            targetPos = targetpointsOfInterestList.transform.position;
            targetPos.z = pupils.transform.localPosition.z;
        }

        // gets distance between clamped target and face
        float dist = Vector3.Distance(pupilCenterTarget.transform.position, targetPos);

        dist = Mathf.Clamp(dist, eyeXmin, eyeXmax);
        dist = Mathf.Clamp(dist, eyeYmin, eyeYmax);

        pupils.transform.position = Vector3.MoveTowards(pupilCenterTarget.transform.position, targetPos, dist);

        Debug.DrawLine(pupilCenterTarget.transform.position, targetPos, Color.red, 1f);
    }

    int ChooseRandomInterest()
    {
        return randomNo = UnityEngine.Random.Range(0, pointsOfInterestList.Length);
    }

    public GameObject RandomlyChoosePoint()
    {
        return pointsOfInterestList[ChooseRandomInterest()];
    }
}

