using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using UnityEngine;
using Random = UnityEngine.Random;

public class Portal : MonoBehaviour
{
    [Header("Scripts")]
    [SerializeField] private VisualEffects visualEffects;
    [SerializeField] private AutoRotation autoRotation;
    
    [Header("Config")]
    [SerializeField] private GameObject portalExit;
    [SerializeField] private float portalActDelay = 0.3f;
    // [SerializeField] private float portalForce = 1f;
    [SerializeField] private BoolGlobalVariable usePortal;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    [SerializeField] private bool usePortals;
    
    [Header("Force through Portals")]
    [SerializeField] private float minVelocityForce = 2f;
    [SerializeField] private float maxVelocityForce = 8f;
    [SerializeField] private float cubeyForce;

    [Header("Other")]
    public GameObject objectToTeleport;

    [Header("Object colour to change")]
    private SpriteRenderer objectColour;
    public Color transparent;
    public Color defaultColour;

    public float portalForceMultiply = 0.5f;
    
    private void Awake()
    {
        if (visualEffects == null) 
            visualEffects = FindObjectOfType<VisualEffects>();
        
        
        transparent = new Color(1,1,1,0);
    }

    private void OnEnable()
    {
        usePortal.CurrentValue = true;
        usePortals = true;
    }

    private void OnDisable()
    {
        usePortals = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        HitPortal(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HitPortal(other);
    }

    private void HitPortal(Collider other)
    {
        if (other.CompareTag("Player") && usePortal.CurrentValue)
        {
            objectToTeleport = other.gameObject;

            if (autoRotation == null)
                autoRotation = objectToTeleport.GetComponent<AutoRotation>();
                
            objectColour = objectToTeleport.transform.Find("BodySprite")?.GetComponent<SpriteRenderer>();
            if (objectColour != null)
                defaultColour = objectColour.color;
            
            TeleportObject();
        }
    }

    private IEnumerator DelayPortalActivation()
    {
        yield return new WaitForSeconds(portalActDelay);
        usePortal.CurrentValue = true;
        objectToTeleport = null;
    }
    
    private void TeleportObject()
    {
        usePortal.CurrentValue = false;
        
        var rb = objectToTeleport.GetComponent<Rigidbody>();
        LeanForceRigidbodyCustom leanForce = objectToTeleport.GetComponent<LeanForceRigidbodyCustom>();
        
        visualEffects.PlayEffect(visualEffects.pePortalEffects, objectToTeleport.transform.position, portalExit.transform.rotation);

        objectToTeleport.transform.localScale = Vector3.zero;
        objectToTeleport.transform.position = portalExit.transform.position;
        objectToTeleport.transform.rotation = Quaternion.Euler(0,0,Random.Range(0, 360));
        rb.isKinematic = true;
        
        StartCoroutine(DelayTeleport(leanForce, rb));
    }

    private float delayTeleportTime = 0.7f;
    
    private IEnumerator DelayTeleport(LeanForceRigidbodyCustom leanForce, Rigidbody rb)
    {
        yield return new WaitForSeconds(delayTeleportTime);
        
        objectToTeleport.transform.localScale = Vector3.one;
        rb.isKinematic = false;
        
        /*if (objectColour != null)
        {
            objectColour.color = Color.Lerp(objectColour.color, transparent, Single.Epsilon); // i += Time.deltaTime * rate;
        }*/
        
        visualEffects.PlayEffect(visualEffects.pePortalEffectsExit, objectToTeleport.transform.position, portalExit.transform.rotation);

        if (autoRotation != null)
            autoRotation.AddRotation(Random.Range(-4, 4));
        
        cubeyForce = 0f;
        if (rb != null)
        {
            if (leanForce != null)
            {
                cubeyForce = Mathf.Max(rb.velocity.magnitude, minVelocityForce);
                cubeyForce = Mathf.Min(cubeyForce, maxVelocityForce);
                rb.AddForce(portalExit.transform.up * cubeyForce, ForceMode.Impulse); // 6
            }
            else
            {
                rb.AddForce(portalExit.transform.up * portalForceMultiply, ForceMode.Impulse);
            }
        }
    
        StartCoroutine(DelayPortalActivation());
    }

}
