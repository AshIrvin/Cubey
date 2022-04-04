using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour {

    [SerializeField] private float windForce;
    private Rigidbody playerRb;

    [SerializeField] private float smoothTime = 0.3F;     // how it takes to get there
    [SerializeField] private float distanceToFlow = 2f;   // distance to go
    [SerializeField] private float offset;

    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 startPos;

    private bool moveRight;
    
    [Header("Wind objects")]
    [SerializeField] private bool shrubbery;
    [SerializeField] private bool windZone;

    private void Start()
    {
        var pos = transform.position;

        startPos = transform.position;

        targetPosition = new Vector3(pos.x + distanceToFlow, pos.y, pos.z);
    }
    
    private void LateUpdate()
    {
        if (shrubbery)
            TreeWindMotion();
    }
    
    private void TreeWindMotion()
    {
        if (moveRight)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            if (transform.position.x >= (targetPosition.x - offset))
                moveRight = false;
        }
        else 
        {
            transform.position = Vector3.SmoothDamp(transform.position, startPos, ref velocity, smoothTime);
            if (transform.position.x <= (startPos.x + offset))
                moveRight = true;
        }
    }

    // wind zones for canyon
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && windZone)
        {
            if (playerRb == null)
                playerRb = other.gameObject.GetComponent<Rigidbody>();

            playerRb.AddForce(transform.right * windForce, ForceMode.Force);
        }
    }


}
