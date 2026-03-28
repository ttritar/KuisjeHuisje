using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrajectoryVisualizer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _timestep = 0.05f;
    [SerializeField] private int _segments = 30;

    private LineRenderer _lineRenderer;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // VISUALIZATION
    //--------------------------------------------------
    public void DrawTrajectory(Vector3 startPos, Vector3 vel, Vector3 gravAttr, float gravStrength)
    {
        _lineRenderer.transform.position = startPos;
        _lineRenderer.positionCount = _segments;

        Vector3 pos = startPos;
        Vector3 velocity = vel;
        for (int i = 0; i < _segments; i++)
        {
            _lineRenderer.SetPosition(i, pos);
            Vector3 gravityDir = (pos - gravAttr);
            gravityDir.z = 0f;
            gravityDir = gravityDir.normalized;
            velocity += gravityDir * gravStrength * _timestep;
            pos += velocity * _timestep;
        }
    }
}
