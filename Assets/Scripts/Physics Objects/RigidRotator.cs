using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RigidRotator : MonoBehaviour
{
    private Rigidbody2D _body;
    private Collider2D _platformCollider;
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

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _platformCollider = GetComponent<Collider2D>();
        _mainCamera = Camera.main;
        _body.angularDrag = 1f; // Add damping to prevent overshooting
    }

    private void Start() => SnapToNearestAngle();

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (_platformCollider.OverlapPoint(mousePos))
            {
                ToggleRotationState();
            }
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

    private void ToggleRotationState()
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
            SnapToNearestAngle();
        }
    }

    private void HandleActiveRotation()
    {
        float currentRotation = GetCurrentRotation();
        float rotationDelta = Mathf.DeltaAngle(_rotationStart, currentRotation);

        // Apply smooth torque with automatic direction handling
        float torque = _rotationSpeed * Mathf.Sign(90 - rotationDelta);
        _body.AddTorque(_reversed ? -torque : torque);

        // Check for snap condition using angular distance
        if (Mathf.Abs(rotationDelta) >= 90 - _snapThreshold)
        {
            SnapToNearestAngle();
            ToggleRotationState();
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
            // Smooth return using velocity damping
            _body.angularVelocity = Mathf.SmoothDamp(
                _body.angularVelocity,
                0f,
                ref _angularVelocity,
                0.1f,
                _returnSpeed
            );

            // Apply corrective torque
            float torque = angleDifference * _returnSpeed * 0.01f;
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