using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsBody : MonoBehaviour
{
    [Header("Gravity")]
    public float GravityStrength
    {
        get => _gravityStrength;
        set => _gravityStrength = value;
    }
    [SerializeField] private float _gravityStrength = -9.81f;
    public Vector3 AttractorPosition => WorldSwitchManager.Instance.CurrentWorldPair.world.transform.position;
    [SerializeField] private Vector2 _randomRotation = new Vector2(1f, 5f);
    private Rigidbody _rb;

    // START
    //--------------------------------------------------
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    // GRAVITY
    //--------------------------------------------------
    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        var dir = transform.position - AttractorPosition;
        dir.z = 0;
        dir = dir.normalized;
        var newDir = GravityStrength * Time.fixedDeltaTime * dir;
        _rb.linearVelocity += newDir;
    }

    // HELPERS
    //--------------------------------------------------
    public bool CalculateSpeedForAngle(Vector3 start, Vector3 end, float angleDegrees, out float speed)
    {
        speed = 0f;

        Vector3 gravityDir = (start - AttractorPosition).normalized;
        gravityDir.z = 0f;
        gravityDir.Normalize();

        Vector3 displacement = end - start;
        float y = Vector3.Dot(gravityDir, displacement);
        Vector3 horizontal = displacement - gravityDir * y;
        float d = horizontal.magnitude;

        if (d < Mathf.Epsilon)
            return false;

        float g = Mathf.Abs(GravityStrength);
        float theta = Mathf.Deg2Rad * angleDegrees;
        float tanTheta = Mathf.Tan(theta);
        float cosTheta = Mathf.Cos(theta);

        float denom = 2f * cosTheta * cosTheta * (d * tanTheta - y);
        if (denom <= 0f)
            return false;

        speed = Mathf.Sqrt(g * d * d / denom);
        return true;
    }

    public Vector3 GetVelocityFromAngle(Vector3 start, Vector3 end, float angleDegrees)
    {
        if (!CalculateSpeedForAngle(start, end, angleDegrees, out float speed))
            return Vector3.zero;

        Vector3 gravityDir = (start - AttractorPosition).normalized;
        gravityDir.z = 0f;
        gravityDir.Normalize();

        Vector3 displacement = end - start;
        float y = Vector3.Dot(gravityDir, displacement);
        Vector3 horizontal = displacement - gravityDir * y;
        Vector3 horizontalDir = horizontal.normalized;

        float theta = Mathf.Deg2Rad * angleDegrees;
        Vector3 velocity = horizontalDir * speed * Mathf.Cos(theta) + gravityDir * speed * Mathf.Sin(theta);
        return velocity;
    }
    public void AddImpulse(Vector3 velocity)
    {
        _rb.linearVelocity += velocity;

        Vector3 randomAxis = Random.onUnitSphere;
        float randomSpeed = Random.Range(_randomRotation.x, _randomRotation.y);
        _rb.angularVelocity += randomAxis * randomSpeed;
    }
}