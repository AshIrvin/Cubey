using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Bezier : MonoBehaviour
{
    public Transform[] controlPoints;
    public LineRenderer lineRenderer;
    
    private int curveCount = 0;    
    private int layerOrder = 0;
    private int SEGMENT_COUNT = 50;

    private void Start()
    {
        if (!lineRenderer)
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
        for (int j = 0; j < curveCount; j++)
        {
            for (int i = 1; i <= SEGMENT_COUNT; i++)
            {
                float t = i / (float)SEGMENT_COUNT;
                int nodeIndex = j * 3;
                
                Vector3 pixel = CalculateCubicBezierPoint(t, 
                    controlPoints [nodeIndex].position, 
                    controlPoints [nodeIndex + 1].position, 
                    controlPoints [nodeIndex + 2].position, 
                    controlPoints [nodeIndex + 3].position
                    );
                
                lineRenderer.SetVertexCount(((j * SEGMENT_COUNT) + i));
                lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
            }
        }
    }

    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // Debug.Log($"p0:{p0}, p1:{p1}, p2:{p2}, p3:{p3}");
        
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
    
    void OnDrawGizmos()
    {
        foreach (Transform marker in controlPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(controlPoints[0].position, marker.position);
        }
    }
}
