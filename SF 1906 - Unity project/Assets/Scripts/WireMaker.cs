using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WireMaker : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform midPoint;
    // private Vector3 midPoint;
    public Transform endPoint;
    public float wireSag = 0.0f;
    
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        //get our midway point
            // TODO: Calculate a mid-point halfway between the end points, decrease .y by wireSag
            // Vector3 midPointPosition = (startPoint.position + endPoint.position) / 2f;
            // Debug.Log("midPointPosition.y = " + midPointPosition.y);
            //midPointPosition -= Vector3.up * wireSag;
            //midPoint.position = midPointPosition;
    }

    void Update()
    {
        DrawQuadraticBezierCurve(startPoint.position, midPoint.position, endPoint.position);
    }

    void DrawQuadraticBezierCurve(Vector3 point0, Vector3 point1, Vector3 point2)
    {
        lineRenderer.positionCount = 200;
        float t = 0f;
        Vector3 B = new Vector3(0, 0, 0);
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            B = (1 - t) * (1 - t) * point0 + 2 * (1 - t) * t * point1 + t * t * point2;
            lineRenderer.SetPosition(i, B);
            t += (1 / (float)lineRenderer.positionCount);
        }
    }

}
