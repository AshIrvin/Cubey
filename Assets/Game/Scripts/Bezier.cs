using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    [SerializeField]
    private Transform[] controlPoints;
    [SerializeField]
    private LineRenderer lineRenderer;
    
    private int curveCount = 0;    
    private int layerOrder = 0;

    private const int SEGMENT_COUNT = 50;

    private void Start()
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int)controlPoints.Length / 3;
    }
    private void Update()
    {
        DrawCurve();
    }
    
    private void DrawCurve()
    {
        for (int curve = 0; curve < curveCount; curve++)
        {
            for (int segment = 1; segment <= SEGMENT_COUNT; segment++)
            {
                SetControlPointPositions(segment, curve);
            }
        }
    }

    private void SetControlPointPositions(int curve, int segment) 
    {
        float t = curve / (float)SEGMENT_COUNT;
        int nodeIndex = segment * 3;

        Vector3 pixel = CalculateCubicBezierPoint(t,
            controlPoints[nodeIndex].position,
            controlPoints[nodeIndex + 1].position,
            controlPoints[nodeIndex + 2].position,
            controlPoints[nodeIndex + 3].position
            );

        var n = (segment * SEGMENT_COUNT) + curve;
        //lineRenderer.SetVertexCount(((j * SEGMENT_COUNT) + i));
        lineRenderer.positionCount = n;
        lineRenderer.SetPosition((segment * SEGMENT_COUNT) + (curve - 1), pixel);
    }

    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;
        
        Vector3 p = uuu * p0; 
        p += 3 * uu * t * p1; 
        p += 3 * u * tt * p2; 
        p += ttt * p3; 
        
        return p;
    }

    private void OnDrawGizmos()
    {
        foreach (Transform marker in controlPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(controlPoints[0].position, marker.position);
        }
    }
}
