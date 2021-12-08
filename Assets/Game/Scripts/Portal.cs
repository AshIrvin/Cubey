using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Portal : MonoBehaviour
{
    [SerializeField] private VisualEffects visualEffects;
    [SerializeField] private AutoRotation autoRotation;
    
    [SerializeField] private GameObject portalExit;
    [SerializeField] private float delay = 0.3f;
    [SerializeField] private BoolGlobalVariable usePortal;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    
    public Vector3 defaultScale;
    public GameObject objectToTeleport;

    public bool scaleObject;
    public float scaleSpeed;
    public float duration;
    
    private bool setDefaultScale;
    private SpriteRenderer objectColour;
    
    public Color transparent;
    public Color defaultColour;

    public float time;
    
    private void Awake()
    {
        if (visualEffects == null) visualEffects = GameObject.Find("VisualEffectsManager").GetComponent<VisualEffects>();
        
        transparent = new Color(1,1,1,0);
    }

    private void OnEnable()
    {
        usePortal.CurrentValue = true;
    }

    private void Update()
    {
        if (scaleObject)
        {
            ScaleObject(true);
            
            if (objectToTeleport.transform.localScale.y < 0.05f)
            {
                scaleObject = false;
                TeleportObject();
            }
        }
        else
        {
            if (objectToTeleport != null && objectToTeleport.transform.localScale != defaultScale)
            {
                if (objectColour != null)
                {
                    // time -= Time.deltaTime * duration;
                    objectColour.color = Color.Lerp(objectColour.color, defaultColour, Single.Epsilon); // i += Time.deltaTime * rate;
                    // objectColour.color = defaultColour;
                    // RestartTime();
                }
                ScaleObject(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && usePortal.CurrentValue)
        {
            objectToTeleport = other.gameObject;
            if (!setDefaultScale)
            {
                autoRotation = objectToTeleport.CompareTag("Player") 
                    ? objectToTeleport.GetComponent<AutoRotation>() : null;
                
                objectColour = objectToTeleport.CompareTag("Player")
                    ? objectToTeleport.transform.Find("BodySprite").GetComponent<SpriteRenderer>() : null;
                
                if (objectColour != null) 
                    defaultColour = objectColour.color;
                
                setDefaultScale = true;
                defaultScale = objectToTeleport.transform.localScale;
            }

            scaleObject = true;
            visualEffects.PlayEffect(visualEffects.pePortalEffects, objectToTeleport.transform.position);
        }
    }

    private IEnumerator DelayPortalActivation()
    {
        yield return new WaitForSeconds(delay);
        usePortal.CurrentValue = true;
    }
    
    private void ScaleObject(bool shrink)
    {
        var scale = shrink ? Vector3.zero : defaultScale;
        objectToTeleport.transform.localScale = Vector3.Slerp(objectToTeleport.transform.localScale, scale, scaleSpeed);
        if (objectColour != null)
        {
            // time -= Time.deltaTime * duration;
            objectColour.color = Color.Lerp(objectColour.color, transparent, Single.Epsilon); // i += Time.deltaTime * rate;
            // RestartTime();
            // objectColour.color = transparent;
        }
    }

    public int startTime = 2;
    
    private void RestartTime()
    {
        if (time < 0)
            time = startTime;
    }
    
    private void TeleportObject()
    {
        usePortal.CurrentValue = false;
        objectToTeleport.transform.position = portalExit.transform.position;
        objectToTeleport.transform.rotation = Quaternion.Euler(0,0,Random.Range(0, 360));
        StartCoroutine(DelayPortalActivation());
        visualEffects.PlayEffect(visualEffects.pePortalEffectsExit, objectToTeleport.transform.position);
        
        if (autoRotation != null)
            autoRotation.AddRotation(Random.Range(-4, 4));
    }
}
