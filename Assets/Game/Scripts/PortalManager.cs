using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    // player enters volume
    // player teleports to exit portal

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Player has entered Portal");

            Vector3 exitPortal = GameObject.Find("PortalExit").gameObject.transform.position;

            other.gameObject.transform.position = exitPortal;
        }
    }

}
