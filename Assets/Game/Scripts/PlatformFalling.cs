using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformFalling : PlatformBase
{
    [SerializeField] private float platformDropTimer = 3;

    private void Awake()
    {
        platformRb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") || collision.collider.CompareTag("Pickup"))
        {
            playerCol = collision.collider;
            StartCoroutine(WaitForPlatform(platformDropTimer));
        } 
    }

    // Player cant jump in the air
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            collision.transform.parent = null;
            playerCol = null;
        }
    }
    
    // Play PE when platform falls
    private IEnumerator WaitForPlatform(float timer)
    {
        Vector3 pos = transform.position;
        pos.y += peOffset;
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pePlatformRockDust, pos);
        yield return new WaitForSeconds(timer);
        platformRb.isKinematic = false;
        yield return new WaitForSeconds(1);
        DestroyObject();
    }

    private void DestroyObject()
    {
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pePlatformExplode1, transform.position);
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pePlatformExplode2, transform.position);
        
        TogglePlatform(false);

        if (playerCol != null)
            playerCol.transform.parent = null;

        StartCoroutine(RespawnPlatform(platformDropTimer));
    }

    private void TogglePlatform(bool enable)
    {
        if (transform.childCount > 0)
        {
            // Todo - xmas falling platform need fixed? GetChild(1)
            var col = transform.GetChild(0).GetComponent<BoxCollider>();
            var sprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
            col.enabled = enable;
            sprite.enabled = enable;
        }
        else
        {
            var col = transform.GetComponent<BoxCollider>();
            var sprite = transform.GetComponent<SpriteRenderer>();
            col.enabled = enable;
            sprite.enabled = enable;
        }
    }

    private IEnumerator RespawnPlatform(float timer)
    {
        yield return new WaitForSeconds(timer);
        // VisualEffects.Instance.pePlatformExplode2.Play();
        VisualEffects.Instance.PlayEffect(VisualEffects.Instance.pePlatformExplode2, transform.position);
        platformRb.isKinematic = true;
        TogglePlatform(true);
    }
    
}
