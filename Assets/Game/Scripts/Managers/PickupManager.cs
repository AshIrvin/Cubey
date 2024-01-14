using UnityEngine;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private IntGlobalVariable pickupCountProperty;
    [SerializeField] private bool levelPickup;

    private float timer;
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

    private void Update()
    {
        if (levelPickup)
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

    private float RandomTime()
    {
        return Random.Range(timeMin, timeMax);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (levelPickup)
            {
                AudioManager.Instance.PlayAudio(AudioManager.Instance.itemPickup);
                VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peSweetPickup, transform.position);

                PickupCountProperty--;

                gameObject.SetActive(false);
            }
        }
    }
}
