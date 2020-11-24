using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Touch;

[RequireComponent(typeof(LineRenderer))]
public class LaunchRenderArc : MonoBehaviour
{
    private LineRenderer lr;
    public LeanForceRigidbody leanForce;

    [Header("Floats")]
    public float velocity;
    public float angle;
    public float leanAngle;
    public float extraPower = 1;
    public float mouseOffset;
    public float maxVelocity = 10;
    public float arcOffset = 0f;
    public float distance = 1;

    [Header("Other")]
    public int resolution = 15;
    public Vector3 groundOffset;
    public Vector3 mousePos;
    public Vector3 leanEndPos;
    public Vector3 direction;

    private float g;
    private float radianAngle;

    public bool renderArcAllowed;

    public GameObject cubey;
    private List<LeanTouch> lt;

    [Header("Sprites")]
    public Sprite spriteImage;
    public GameObject[] spr;
    public GameObject spriteGrp;


    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        g = Mathf.Abs(Physics2D.gravity.y);
        cubey = transform.parent.gameObject;
        leanForce = transform.GetComponentInParent<LeanForceRigidbody>();
    }


    private void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && renderArcAllowed)
            RenderArc();
            
    }

    private void RenderArc()
    {
        if (!lr.enabled)
            lr.enabled = false;

        if (!spriteGrp.activeSelf)
            spriteGrp.SetActive(true);

        lr.positionCount = resolution + 1;
        //lr.SetPositions(CalculateArcArray());
        SpawnArcSprites(CalculateArcArray());

        leanAngle = leanForce.angle;

    }

    private void SpawnArcSprites(Vector3[] positions)
    {
        for (int i = 0; i < positions.Length-1; i++)
        {
            //var s = Instantiate(spr, spriteGrp.transform);
            spr[i].transform.position = positions[i];

        }

    }

    public void RemoveArc()
    {
        lr.positionCount = 0;
        spriteGrp.SetActive(false);
    }

    private Vector3 GetMousePoint()
    {
        Ray castPoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        mousePos = castPoint.origin + (castPoint.direction * distance);

        //mousePos = point;

        return mousePos;
    }

    private float CalculateMouseAngle()
    {
        var startPosition = cubey.transform.position;

        mousePos = GetMousePoint();
        velocity = Vector3.Distance(mousePos, startPosition);
        velocity *= extraPower;
        velocity = Mathf.Min(maxVelocity, velocity);
        velocity = Mathf.Max(0, velocity);

        direction = mousePos - startPosition;
        //direction.z = 0;

        if (direction.sqrMagnitude < 0.1f)
        {
            return 0;
        }

        float angle2 = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //var angle2 = leanForce.angle;

        if (angle2 < 0) angle2 += 360;


        return angle2;
    }



    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[resolution + 1];

        angle = CalculateMouseAngle();

        radianAngle = Mathf.Deg2Rad * angle;
        //radianAngle -= arcOffset;


        float maxDistance = (velocity * velocity * Mathf.Sin(2 * radianAngle)) / g;

        arcArray[0] = cubey.transform.position + groundOffset;

        for (int i = 1; i <= resolution; i++)
        {
            float t = (float)i / (float)resolution;
            arcArray[i] = CalculateArcPoint(t, maxDistance) + arcArray[0]; 
        }

        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(radianAngle) - ((g * x * x) / (2 * velocity * velocity * Mathf.Cos(radianAngle) * Mathf.Cos(radianAngle)));
        return new Vector3(x, y);

    }

}
