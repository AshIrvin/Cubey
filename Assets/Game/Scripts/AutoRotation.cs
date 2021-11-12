using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    private Rigidbody rb;
    public float torque;
    [SerializeField] private bool enableRotation;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!enableRotation) return;
        
        rb.AddTorque(transform.forward * torque);
    }
}
