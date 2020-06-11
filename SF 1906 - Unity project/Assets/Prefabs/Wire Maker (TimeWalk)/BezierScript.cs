using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierScript : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Transform p0;
    public Transform p1;
    public Transform p2;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        DrawQuadraticBezierCurve(p0.position, p1.position, p2.position);
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
