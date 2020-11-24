using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowman : MonoBehaviour
{
    public Rigidbody rb1, rb2, rb3;

    private void Start()
    {
        //var snowman = transform.parent.gameObject;        

        //rb1 = transform.GetChild(0).GetComponent<Rigidbody>();
        //rb2 = transform.GetChild(1).GetComponent<Rigidbody>();
        //rb3 = transform.GetChild(2).GetComponent<Rigidbody>();

    }

    private void OnCollisionEnter(Collision collision)
    {
        //print("snowman was hit by something: " + collision.collider.name);
        //if (collision.collider.CompareTag("Player"))
        //{
        //    rb1.isKinematic = false;
        //    rb2.isKinematic = false;
        //    rb3.isKinematic = false;
        //}
    }

}
