using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    [Serializable]
    public enum GravityDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public GravityDirection gravityDirection;

    private static readonly float _gravity = -9.81f;
    
    private void ChangeGravity()
    {
        switch (gravityDirection)
        {
            case GravityDirection.Up:
                Physics.gravity = new Vector3(0, -_gravity, 0);
                break;
            case GravityDirection.Down:
                Physics.gravity = new Vector3(0, _gravity, 0);
                break;
            case GravityDirection.Left:
                Physics.gravity = new Vector3(-_gravity, 0, 0);
                break;
            case GravityDirection.Right:
                Physics.gravity = new Vector3(_gravity, 0, 0);
                break;
            default:
                Physics.gravity = new Vector3(0, _gravity, 0);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ChangeGravity();
        }
    }

    public static void ResetGravity()
    {
        Physics.gravity = new Vector3(0, _gravity, 0);
    }
}
