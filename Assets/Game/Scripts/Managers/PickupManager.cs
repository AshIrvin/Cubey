using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PickupManager : MonoBehaviour
{
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Vector3 startPos;

    [SerializeField] private bool butterfly; // increases jumps
    [SerializeField] private bool friend; //
    [SerializeField] private bool sweet;

    // [SerializeField] private bool butterflyWing;


    [Header("Timer")]
    public float setTime = 2f;
    public float timer;

    private Vector3 velocity = Vector3.zero;

    [Header("Movement")]
    [SerializeField] private Vector3 newPos;
    [SerializeField] private Vector3 currentPos;
    [SerializeField] private float smoothTime = 0.3F;
    [SerializeField] private float dist = 0.1f;
    [SerializeField] private float positionRange = 0.1f;

    [Header("Random time range")]
    [SerializeField] private float timeMin;
    [SerializeField] private float timeMax;

    private bool moveObject;

    public int PickupCountProperty
    {
        get => pickupCountProperty.CurrentValue;
        set => pickupCountProperty.CurrentValue = value;
    }

    private void Start()
    {
        if (audioManager == null)
            audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();

        startPos = transform.position;
    }

    private void Update()
    {
        if (butterfly)
        {
            ButterflyEffect();
        }
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

    private void MoveObject()
    {
        if (moveObject)
        {
            gameObject.transform.position = Vector3.SmoothDamp(transform.position, RandomPosition(), ref velocity, smoothTime);
            if (Vector3.Distance(currentPos, newPos) < dist)
            {
                timer = RandomTime();
                moveObject = false;
            }
        }
    }

    private Vector3 RandomPosition()
    {
        var n = Random.Range(-positionRange, positionRange);
        var p = Random.Range(-positionRange, positionRange);
        currentPos = transform.position;
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
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peAirBoom, transform.position);

            PickupCountProperty--;
        }

        if (other.CompareTag("Player") && butterfly)
        {
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peSweetPickup, transform.position);

            PickupCountProperty--;
            Destroy(gameObject);
        }
    }
}
