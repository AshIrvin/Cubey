using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

public class FingerPos : MonoBehaviour
{
    private static Vector3 fingerPosition;
    private static Vector3 playerPosition;
    public static Vector3 FingerPosition => fingerPosition;
    public static Vector3 FingerPlayerDirection => fingerPosition - playerPosition;
    public static bool belowPlayer;
    public static bool screenTouched;
    private float fingerOffset = 1.2f;
    
    private LeanFingerLine leanFingerLine;
    [SerializeField] private GameObject screenPosGo2;

    public Vector3 GetPlayerPosition => transform.position;

    private void Awake()
    {
        if (leanFingerLine == null)
            leanFingerLine = FindObjectOfType<LeanFingerLine>();
    }

    private void OnEnable()
    {
        LeanTouch.OnFingerSet += GetFingerPosition;
        LeanTouch.OnFingerUp += ResetFinger;
    }

    private void OnDisable()
    {
        LeanTouch.OnFingerSet -= GetFingerPosition;
        LeanTouch.OnFingerUp -= ResetFinger;
    }
    
    private void GetFingerPosition(LeanFinger finger)
    {
        if (Input.GetMouseButton(0))
        {
            playerPosition = GetPlayerPosition;
            fingerPosition = leanFingerLine.ScreenDepth.Convert(finger.ScreenPosition, Camera.main, gameObject);
            
            if (screenPosGo2 != null)
            {
                screenPosGo2.transform.position = fingerPosition;
            }
            
            belowPlayer = fingerPosition.y < transform.position.y + fingerOffset;
        }
    }

    private void ResetFinger(LeanFinger finger)
    {
        belowPlayer = false;
    }

    public static float GetCameraPlayerDistance()
    {
        return Vector3.Distance(playerPosition, Camera.main.transform.position);
    }
}
