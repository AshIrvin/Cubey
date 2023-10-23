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

    private GameObject rock01;
    private GameObject rock02;

    private void Start()
    {
        rock01 = transform.GetChild(0).gameObject;
        rock02 = transform.GetChild(1).gameObject;
        rockRb = GetComponent<Rigidbody>();
        defaultRockPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peRockBreakage, transform.position, Quaternion.Euler(0,0,0));
        ToggleObjectImage(false);
        StartCoroutine(WaitToReset());
    }

    private IEnumerator WaitToReset()
    {
        yield return new WaitForSeconds(rockHitReset);
        rockRb.velocity = Vector3.zero;
        transform.position = defaultRockPosition;
        ToggleObjectImage(true);
        // transform.rotation = GetRandomRotation();
    }

    private Quaternion GetRandomRotation()
    {
        var rot = UnityEngine.Random.Range(-90, 90);
        return Quaternion.Euler(0,0, rot);
    }

    private void ToggleObjectImage(bool state)
    {
        // transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        // transform.GetChild(1).gameObject.SetActive(!transform.GetChild(1).gameObject.activeInHierarchy);
        
        rock01.SetActive(state);
        rock02.SetActive(state);
    }
}
