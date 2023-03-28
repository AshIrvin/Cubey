using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    // [SerializeField] private float torque = 2;
    [SerializeField] private bool enableRotation;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        AddRotation(2);
    }

    public void AddRotation(float torque)
    {
        if (!enableRotation) return;
        
        if (rb != null)
            rb.AddTorque(transform.forward * torque);
    }
}
