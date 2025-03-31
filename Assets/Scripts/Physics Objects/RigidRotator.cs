using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RigidRotator : Interactable
{
    private Rigidbody2D _body;
    private Camera _mainCamera;
    private bool _rotating;
    private float _rotationStart;
    private float _angularVelocity;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationSpeed = 9000f;
    [SerializeField] private float _maxSpeed = 10000f;
    [SerializeField] private float _returnSpeed = 7000f;
    [SerializeField] private bool _reversed = false;
    [SerializeField][Range(0.1f, 1.5f)] private float _snapThreshold = 0.9f;

    [Header("Audio")]
    public AudioSource rotationAudio;

    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _body.angularDrag = 1f;
        SnapToNearestAngle();
    }

    public override void OnToggle()
    {
        _rotating = !_rotating;
        _rotationStart = GetCurrentRotation();

        if (_rotating)
        {
            rotationAudio.Play();
        }
        else
        {
            rotationAudio.Stop();
        }
    }

    private void FixedUpdate()
    {
        if (_rotating)
        {
            HandleActiveRotation();
        }
        else
        {
            HandleReturnRotation();
        }

        ClampAngularVelocity();
    }

    private void HandleActiveRotation()
    {
        float currentRotation = GetCurrentRotation();
        float rotationDelta = Mathf.DeltaAngle(_rotationStart, currentRotation);

        float torque = _rotationSpeed * Mathf.Sign(90 - rotationDelta);
        _body.AddTorque(_reversed ? -torque : torque);

        if (Mathf.Abs(rotationDelta) >= 90 - _snapThreshold)
        {
            SnapToNearestAngle();
            Toggle();
        }
    }

    private void HandleReturnRotation()
    {
        float targetAngle = GetClosestRightAngle();
        float currentAngle = GetCurrentRotation();
        float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

        if (Mathf.Abs(angleDifference) <= _snapThreshold)
        {
            SnapToNearestAngle();
        }
        else
        {
            float torque = angleDifference * _returnSpeed;
            _body.AddTorque(_reversed ? -torque : torque);
        }
    }

    private float GetCurrentRotation() =>
        _reversed ? (360 - _body.rotation) % 360 : _body.rotation % 360;

    private void SnapToNearestAngle()
    {
        _body.MoveRotation(GetClosestRightAngle());
        _body.angularVelocity = 0f;
    }

    private void ClampAngularVelocity() =>
        _body.angularVelocity = Mathf.Clamp(_body.angularVelocity, -_maxSpeed, _maxSpeed);

    private float GetClosestRightAngle()
    {
        float current = GetCurrentRotation();
        float normalized = Mathf.Repeat(current, 360f);
        float rounded = Mathf.Round(normalized / 90f) * 90f;
        return Mathf.Repeat(rounded, 360f);
    }
}