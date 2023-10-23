using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SmoothRandomRotation : MonoBehaviour
{
    [SerializeField] private bool axisX;
    [SerializeField] private bool axisY;
    [SerializeField] private bool axisZ;
    
    private float time;
    private int newAngle;
    private float rotSpeed = 1;
    private Quaternion rot;
    [SerializeField] private bool rotateEnabled;
    
    void Start()
    {
        time = RandomTime();
    }

    void Update()
    {
        time -= Time.deltaTime;

        if (time < 0)
        {
            time = RandomTime();
            newAngle = RandomAngle();
            StartCoroutine(Reset());
        }

        if (rotateEnabled)
        {
            RotateToTarget();
        }
    }

    private void RotateToTarget()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * rotSpeed);
    }

    IEnumerator Reset()
    {
        rotateEnabled = true;
        yield return new WaitForSeconds(1);
        rotateEnabled = false;
        var r = new Vector3(axisX ? newAngle : 0, axisY ? newAngle : 0, axisZ ? newAngle : 0);
        rot = Quaternion.Euler(r);
    }

    private int RandomAngle()
    {
        return Random.Range(-15, -65);
    }
    
    private float RandomTime()
    {
        return Random.Range(2, 5);
    }
}
