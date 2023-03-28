using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class DropRockManager : MonoBehaviour
{
    [SerializeField] private float rockHitReset = 1f;
    
    private Vector3 defaultRockPosition;
    private Rigidbody rockRb;

    private void OnEnable()
    {
        defaultRockPosition = transform.position;
        rockRb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (!collision.collider.CompareTag("Player"))
        // {
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peRockBreakage, transform.position, Quaternion.Euler(0,0,0));
            ToggleObjectImage();
            StartCoroutine(WaitToReset());
        /*}
        else
        {
            Debug.Log("Hit player! Should this reset the player?");
        }*/
    }
    
    private IEnumerator WaitToReset()
    {
        yield return new WaitForSeconds(rockHitReset);
        rockRb.velocity = Vector3.zero;
        transform.position = defaultRockPosition;
        transform.rotation = GetRandomRotation();
        ToggleObjectImage();
    }

    private Quaternion GetRandomRotation()
    {
        var rot = UnityEngine.Random.Range(-90, 90);
        return Quaternion.Euler(0,0, rot);
    }

    private void ToggleObjectImage()
    {
        transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        transform.GetChild(1).gameObject.SetActive(!transform.GetChild(1).gameObject.activeInHierarchy);
    }
}
