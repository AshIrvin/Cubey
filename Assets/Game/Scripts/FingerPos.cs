using System;
using Lean.Touch;
using UnityEngine;

public class FingerPos : MonoBehaviour
{
    private LeanForceRigidbodyCustom leanForceRigidbodyCustom;
    
    private static Vector3 fingerPosition;
    private static Vector3 playerPosition;
    private float fingerOffset = 1.2f;
    private LeanFingerLine leanFingerLine;

    public static Vector3 FingerPosition => fingerPosition;
    public static Vector3 FingerPlayerDirection => fingerPosition - playerPosition;
    public static bool belowPlayer;
    public static bool abovePlayer;

    public Vector3 GetPlayerPosition => transform.position;

    public static Action belowCubey;
    public static Action aboveCubey;
    // public static Action<bool> allowedJump;
    
    private void Awake()
    {
        if (leanFingerLine == null)
            leanFingerLine = FindFirstObjectByType<LeanFingerLine>();

        leanForceRigidbodyCustom = GetComponent<LeanForceRigidbodyCustom>();
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
            
            belowPlayer = fingerPosition.y < transform.position.y + fingerOffset;
            abovePlayer = fingerPosition.y > transform.position.y;
            
            if (belowPlayer)
            {
                belowCubey?.Invoke();
                leanForceRigidbodyCustom.CheckGroundBeforeJump();
            }
            else
            {
                aboveCubey?.Invoke();
            }
        }
    }

    private void ResetFinger(LeanFinger finger)
    {
        belowPlayer = false;
        abovePlayer = false;
    }

    public static float GetCameraPlayerDistance()
    {
        return Vector3.Distance(playerPosition, Camera.main.transform.position);
    }   
}
