using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindManager : MonoBehaviour {

    public float windForce;
    private Rigidbody playerRb;

    public float smoothTime = 0.3F;     // how it takes to get there
    public float distanceToFlow = 2f;   // distance to go
    public float offset;

    //public Transform target;
    private Vector3 velocity = Vector3.zero;
    public Vector3 targetPosition, startPos;

    public bool moveRight;

    private void Start()
    {
        var pos = transform.position;

        startPos = transform.position;

        targetPosition = new Vector3(pos.x + distanceToFlow, pos.y, pos.z);

    }

    void TreeWindMotion()
    {
        // Define a target position above and behind the target transform

        // Smoothly move the camera towards that target position

        //print("transform.position.x: " + transform.position.x + ", targetPosition.x: " + targetPosition.x + ", startPos: " + startPos);

        //moveRight = (transform.position.x < startPos.x);

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

        //moveRight = true ? (transform.position.x < (startPos.x + offset)) : (transform.position.x > (targetPosition.x - offset));

        //if (movingPlatform && moveHor && !fixedObject)
        //{
        //    if (moveRight)
        //    {
        //        transform.position += (velocity * Time.deltaTime);
        //    }
        //    else
        //        transform.position -= (velocity * Time.deltaTime);

        //    // checks against start pos on how far to move
        //    if (transform.position.x > (startPos.x + horPlatformDistance))
        //    {
        //        moveRight = false;
        //    }
        //    else if (transform.position.x < startPos.x)
        //    {
        //        moveRight = true;
        //    }

        //}
    }

    private void LateUpdate()
    {
        TreeWindMotion();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("WIND WIND WIND IS OFF");
            // when player enters trigger, they get thrown in that direction
            //playerRb = other.gameObject.GetComponent<Rigidbody>();
            //playerRb.AddForce(transform.right * windForce, ForceMode.Impulse);

        }
    }


}
