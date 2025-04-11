using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
/// <summary>An interactable object that rotates with physics torque, snapping to 90-degree increments.</summary>
public class RigidRotator : Interactable, IParameterEditable
{
    #region Fields

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

    #endregion

    #region Unity Methods

    private void Start()
    {
        _body = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _body.angularDrag = 1f;
        SnapToNearestAngle();
    }

    /// <summary>Toggles rotation mode on or off.</summary>
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
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

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

    #endregion

    #region Rotation Methods

    /// <summary>Applies torque to rotate the object until it reaches 90 degrees from start.</summary>
    private void HandleActiveRotation()
    {
        float currentRotation = GetCurrentRotation();
        float rotationDelta = Mathf.DeltaAngle(_rotationStart, currentRotation);

        // Apply torque in the direction needed to reach 90 degrees
        float torque = _rotationSpeed * Mathf.Sign(90 - rotationDelta);
        _body.AddTorque(_reversed ? -torque : torque);

        // Snap and stop rotating when close enough to 90 degrees
        if (Mathf.Abs(rotationDelta) >= 90 - _snapThreshold)
        {
            SnapToNearestAngle();
            Toggle();
        }
    }

    /// <summary>Applies torque to return the object to the nearest 90-degree angle.</summary>
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

    /// <summary>Gets the current rotation, accounting for reversed mode.</summary>
    private float GetCurrentRotation() =>
        _reversed ? (360 - _body.rotation) % 360 : _body.rotation % 360;

    /// <summary>Snaps the rotation to the nearest 90-degree angle and stops spinning.</summary>
    private void SnapToNearestAngle()
    {
        _body.MoveRotation(GetClosestRightAngle());
        _body.angularVelocity = 0f;
    }

    /// <summary>Clamps the angular velocity to prevent excessive spinning.</summary>
    private void ClampAngularVelocity() =>
        _body.angularVelocity = Mathf.Clamp(_body.angularVelocity, -_maxSpeed, _maxSpeed);

    /// <summary>Calculates the closest 90-degree angle to the current rotation.</summary>
    /// <returns>Angle in degrees.</returns>
    private float GetClosestRightAngle()
    {
        float current = GetCurrentRotation();
        float normalized = Mathf.Repeat(current, 360f);
        float rounded = Mathf.Round(normalized / 90f) * 90f;
        return Mathf.Repeat(rounded, 360f);
    }

    #endregion

    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        var rotationSpeedStr = _rotationSpeed.ToString(CultureInfo.InvariantCulture);
        var returnSpeedStr = _returnSpeed.ToString(CultureInfo.InvariantCulture);
        var reversedStr = _reversed.ToString();
        var snapThresholdStr = _snapThreshold.ToString(CultureInfo.InvariantCulture);

        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "rotationSpeed",
                type = "float",
                value = rotationSpeedStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "returnSpeed",
                type = "float",
                value = returnSpeedStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "reversed",
                type = "bool",
                value = reversedStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "snapThreshold",
                type = "float",
                value = snapThresholdStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var rotationSpeedParam = parameters?.FirstOrDefault(p => p.name == "rotationSpeed" && p.type == "float");
        if (rotationSpeedParam != null && float.TryParse(rotationSpeedParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float rotationSpeed))
        {
            _rotationSpeed = rotationSpeed;
        }

        var returnSpeedParam = parameters?.FirstOrDefault(p => p.name == "returnSpeed" && p.type == "float");
        if (returnSpeedParam != null && float.TryParse(returnSpeedParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float returnSpeed))
        {
            _returnSpeed = returnSpeed;
        }

        var reversedParam = parameters?.FirstOrDefault(p => p.name == "reversed" && p.type == "bool");
        if (reversedParam != null && bool.TryParse(reversedParam.value, out bool reversed))
        {
            _reversed = reversed;
        }

        var snapThresholdParam = parameters?.FirstOrDefault(p => p.name == "snapThreshold" && p.type == "float");
        if (snapThresholdParam != null && float.TryParse(snapThresholdParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float snapThreshold))
        {
            _snapThreshold = Mathf.Clamp(snapThreshold, 0.1f, 1.5f);
        }
    }

    #endregion
}