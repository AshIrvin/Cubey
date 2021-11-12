using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickupManager : MonoBehaviour
{
    // public static PickupManager Instance { get; set; }
    // [SerializeField] private VisualEffects visualEffects;

    [SerializeField] private IntGlobalVariable pickupCountProperty;
    
    
    
    
    public AudioManager audioManager;

    
    public Vector3 startPos;

    public bool butterfly; // increases jumps
    public bool friend; //
    public bool sweet;

    public bool butterflyWing;


    [Header("Timer")]
    public float setTime = 2f;
    public float timer;

    private Vector3 velocity = Vector3.zero;

    [Header("Movement")]
    public Vector3 newPos;
    public Vector3 currentPos;
    public float smoothTime = 0.3F;
    public float dist = 0.1f;
    public float posRange = 0.1f;

    [Header("Random time range")]
    public float timeMin;
    public float timeMax;

    bool moveObject;

    public int PickupCountProperty
    {
        get => pickupCountProperty.CurrentValue;
        set => pickupCountProperty.CurrentValue = value;
    }


    private void Awake()
    {
        // Instance = this;
    }

    private void Start()
    {
        if (audioManager == null)
            audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        //if (Vector3.Distance(startPos, transform.position) > 0.1f && butterfly)
        //{
        //    transform.position = startPos;
        //} else
        //{
            startPos = transform.position;
        //}

    }

    private void Update()
    {
        if (butterfly)
        {
            ButterflyEffect();
        }

        if (butterflyWing)
        {

        }
    }

    private void ButterflyWingFlutter()
    {

    }

    private void ButterflyEffect()
    {
        timer -= Time.deltaTime;
        if (timer < 0.1f)
        {
            moveObject = true;
            MoveObject();
        }
    }

    void MoveObject()
    {
        if (moveObject)
        {
            gameObject.transform.localPosition = Vector3.SmoothDamp(transform.position, RandomPosition(), ref velocity, smoothTime);
            if (Vector3.Distance(currentPos, newPos) < dist)
            {
                timer = RandomTime();
                moveObject = false;
            }
        }
    }

    Vector3 RandomPosition()
    {
        var n = Random.Range(-posRange, posRange);
        var p = Random.Range(-posRange, posRange);
        currentPos = transform.localPosition;
        newPos = currentPos;

        newPos.x += n;
        newPos.y += p;

        return newPos;
    }

    float RandomTime()
    {
        return Random.Range(timeMin, timeMax);
    }

    // if player his a friend, friend vanishes and PE emits
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && sweet)
        {
            audioManager.PlayAudio(audioManager.itemPickup);
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peSweetPickup, transform.position);

            PickupCountProperty--;

            Destroy(gameObject);
        }

        if (other.CompareTag("Player") && friend)
        {
            Destroy(gameObject);
            // PE needed
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peAirBoom, transform.position);

            PickupCountProperty--;
        }

        //if (other.CompareTag("PickUpJump"))
        if (other.CompareTag("Player") && butterfly)
        {
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peSweetPickup, transform.position);

            PickupCountProperty--;
            //GameManager.Instance.AddJump();
            Destroy(gameObject);
        }

        //if (other.CompareTag("Player") && sweet)
        //{
        //    // PE needed
        //    VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peSweetPickup, transform.position);

        //    GameManager.Instance.PickupCount(gameObject);

        //    Destroy(gameObject);
        //}
    }

}
