using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEditor.Rendering;

public class GravityManager : MonoBehaviour
{
    private MapManager mapManager;
    [SerializeField] private GameObject pivotObject;
    [SerializeField] private Quaternion target;
    [SerializeField] private Transform targetDirection;
    [SerializeField] private bool dontKill;

    private static readonly float _gravity = -9.81f;
    private bool allowLevelRotation = false;
    private GameObject cubePivot;
    private Rigidbody rb;
    private float defaultDrag = 0;
    private GameObject levelObject;
    private LevelManager levelManager;


    public float turnDuration = 2f;
    public float gravityDrag = 40;
    public float delay = 1.8f;
    public float resetWait = 0.2f;

    [SerializeField] private int currentAngle;

    private void Start()
    {
        levelManager = LevelManager.Instance;
    }

    private void OnEnable()
    {
        if (mapManager == null)
        {
            mapManager = FindFirstObjectByType<MapManager>();
        }

        targetDirection = transform.Find("GravityDirection").transform;
        target = Quaternion.Euler(targetDirection.eulerAngles);

        currentAngle = 0;
    }

    private IEnumerator WaitBeforeResetingParent()
    {
        yield return new WaitForSeconds(resetWait);
        levelObject.transform.SetParent(levelManager.LevelParent.transform);
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
        levelObject = levelManager.LevelGameObject;
        target = Quaternion.Euler(targetDirection.eulerAngles);

        rb.drag = gravityDrag;
        rb.angularDrag = gravityDrag;
        rb.useGravity = false;

        if (cubePivot == null)
        {
            cubePivot = Instantiate(pivotObject, pos, Quaternion.identity);
            cubePivot.transform.Rotate(targetDirection.eulerAngles);
            Logger.Instance.ShowDebugLog("cubePivot rotate: " + cubePivot.transform.eulerAngles);
        }
        
        cubePivot.transform.SetParent(levelManager.LevelParent.transform);
        levelObject.transform.SetParent(cubePivot.transform);
        
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        Vector3 newAngle = new Vector3(0,0, currentAngle - (int)target.eulerAngles.z);

        if (newAngle.z > 180 && newAngle.z < 360)
        {
            newAngle.z -= 360;
        }
        else if (newAngle.z < -180 && newAngle.z > -360)
        {
            newAngle.z += 360;
        }
        
        cubePivot.transform.DORotate(newAngle, turnDuration, RotateMode.WorldAxisAdd).SetEase(Ease.OutCubic).onComplete = () =>
        {
            StartCoroutine(WaitBeforeResetingParent());
            currentAngle = (int)newAngle.z;
        };
        
        yield return new WaitForSeconds(delay);

        rb.drag = defaultDrag;
        rb.angularDrag = defaultDrag;
        rb.useGravity = true;

        yield return new WaitForSeconds(delay);
        allowLevelRotation = false;
    }

}
