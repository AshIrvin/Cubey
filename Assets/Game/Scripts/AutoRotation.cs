using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    [SerializeField] 
    private bool enableRotation;

    private Rigidbody rigidBody;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        AddRotation(2);
    }

    public void AddRotation(float torque)
    {
        if (rigidBody == null || !enableRotation)
            return;

        rigidBody.AddTorque(transform.forward * torque);
    }
}
