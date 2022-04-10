using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GravityManager : MonoBehaviour
{
    private MapManager mapManager;
    [SerializeField] private GameObject pivotObject;
    [SerializeField] private Quaternion target;
    [SerializeField] private Transform targetDirection;

    private static readonly float _gravity = -9.81f;
    private bool allowLevelRotation = false;
    private GameObject cubePivot;
    private Rigidbody rb;
    private float defaultDrag = 0;
    private GameObject levelObject;

    [SerializeField] private bool dontKill;

    public float turnDuration = 2f;
    public float gravityDrag = 40;
    public float delay = 1.8f;
    public float resetWait = 0.2f;

    private void OnEnable()
    {
        if (mapManager == null)
        {
            mapManager = FindObjectOfType<MapManager>();
        }

        targetDirection = transform.Find("GravityDirection").transform;
        target = Quaternion.Euler(targetDirection.eulerAngles);
    }

    private void Update()
    {
        RotateLevel();
    }

    private void RotateLevel()
    {
        if (cubePivot != null && 
            cubePivot.transform.rotation != target && 
            target != null &&
            allowLevelRotation)
        {
            cubePivot.transform.DOLocalRotate(target.eulerAngles, turnDuration, RotateMode.Fast).SetEase(Ease.OutCubic).onComplete = () =>
            {
                StartCoroutine(WaitBeforeResetingParent());
            };
        }
    }

    private IEnumerator WaitBeforeResetingParent()
    {
        yield return new WaitForSeconds(resetWait);
        levelObject.transform.SetParent(mapManager.LevelParent.transform);
        Destroy(cubePivot);
        if (!dontKill)
        {
            VisualEffects.Instance.PlayEffect(VisualEffects.Instance.peGravityImplode, transform.position);
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (rb == null)
            {
                rb = other.GetComponent<Rigidbody>();
            }

            ChangeGravity(other.transform.position);
        }
    }

    private void ChangeGravity(Vector3 pos)
    {
        if (allowLevelRotation) return;
        
        allowLevelRotation = true; // should go through script once before not allowed
        levelObject = mapManager.LevelGameObject;
        target = Quaternion.Euler(targetDirection.eulerAngles);

        rb.drag = gravityDrag;
        rb.angularDrag = gravityDrag;
        rb.useGravity = false;

        if (cubePivot == null)
        {
            cubePivot = Instantiate(pivotObject, pos, Quaternion.identity);
        }
        
        cubePivot.transform.SetParent(mapManager.LevelParent.transform);
        levelObject.transform.SetParent(cubePivot.transform);
        
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(delay);

        rb.drag = defaultDrag;
        rb.angularDrag = defaultDrag;
        rb.useGravity = true;

        yield return new WaitForSeconds(delay);
        allowLevelRotation = false;
    }

}
