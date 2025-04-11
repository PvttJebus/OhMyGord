using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
/// <summary>An interactable object that smoothly rotates towards the mouse, then returns to start.</summary>
public class SmoothRotator : Interactable, IParameterEditable
{
    #region Fields

    private Rigidbody2D body;
    private float rotationStart;
    private float clickOffsetAngle;
    private bool rotating;

    [Header("Rotation Settings")]
    public float rotationSpeed = 15f;
    public float returnSpeed = 10f;
    public bool returnToStart = true;
    [SerializeField] private float closeEnough = 0.1f;
    [SerializeField] private float decay = 0.9f;

    private float targetAngle;
    private float absoluteTargetAngle;
    private float lastTargetAngle;

    #endregion

    #region Unity Methods

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        rotationStart = body.rotation;
    }

    /// <summary>Starts rotation towards the mouse.</summary>
    public override void OnToggle()
    {
        rotating = true;

        // Calculate offset between current rotation and click position
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 toClick = mouseWorldPos - (Vector2)transform.position;
        float clickAngle = Mathf.Atan2(toClick.y, toClick.x) * Mathf.Rad2Deg - 90f;

        clickOffsetAngle = Mathf.DeltaAngle(body.rotation, clickAngle);
    }

    private void Update()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        if (rotating)
        {
            // Calculate angle from object to mouse position
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float mouseAngle = Mathf.Atan2(
                mousePosition.y - transform.position.y,
                mousePosition.x - transform.position.x
            ) * Mathf.Rad2Deg - 90f;

            absoluteTargetAngle = mouseAngle - clickOffsetAngle;

            // Prefer continuous rotation direction
            float delta = absoluteTargetAngle - lastTargetAngle;
            if (delta > 180f)
                absoluteTargetAngle -= 360f;
            else if (delta < -180f)
                absoluteTargetAngle += 360f;

            targetAngle = absoluteTargetAngle;
            lastTargetAngle = absoluteTargetAngle;

            // Stop rotating on mouse release
            if (Input.GetMouseButtonUp(0))
            {
                rotating = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        if (rotating)
        {
            HandleActiveRotation();
        }
        else if (returnToStart)
        {
            HandleReturnRotation();
        }

        // Apply damping to slow down rotation
        body.angularVelocity *= decay;
    }

    #endregion

    #region Rotation Methods

    /// <summary>Rotates towards the target angle based on mouse position.</summary>
    private void HandleActiveRotation()
    {
        float angleDiff = absoluteTargetAngle - body.rotation;

        // Apply torque if not close enough
        body.AddTorque(Mathf.Abs(angleDiff) > closeEnough
            ? angleDiff * rotationSpeed
            : 0);
    }

    /// <summary>Rotates back to the initial rotation angle.</summary>
    private void HandleReturnRotation()
    {
        float returnAngleDiff = Mathf.DeltaAngle(body.rotation, rotationStart);
        float altReturnAngleDiff = Mathf.DeltaAngle(body.rotation + 180, rotationStart);

        // Choose the shortest rotation direction
        if (Mathf.Abs(altReturnAngleDiff) < Mathf.Abs(returnAngleDiff))
            returnAngleDiff = altReturnAngleDiff;

        // Apply torque if not close enough
        if (Mathf.Abs(returnAngleDiff) > closeEnough)
            body.AddTorque(returnAngleDiff * returnSpeed);
    }

    #endregion

    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        var rotationSpeedStr = rotationSpeed.ToString(CultureInfo.InvariantCulture);
        var returnSpeedStr = returnSpeed.ToString(CultureInfo.InvariantCulture);
        var returnToStartStr = returnToStart.ToString();
        var closeEnoughStr = closeEnough.ToString(CultureInfo.InvariantCulture);
        var decayStr = decay.ToString(CultureInfo.InvariantCulture);

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
                name = "returnToStart",
                type = "bool",
                value = returnToStartStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "closeEnough",
                type = "float",
                value = closeEnoughStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "decay",
                type = "float",
                value = decayStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var rotationSpeedParam = parameters?.FirstOrDefault(p => p.name == "rotationSpeed" && p.type == "float");
        if (rotationSpeedParam != null && float.TryParse(rotationSpeedParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedRotationSpeed))
        {
            rotationSpeed = parsedRotationSpeed;
        }

        var returnSpeedParam = parameters?.FirstOrDefault(p => p.name == "returnSpeed" && p.type == "float");
        if (returnSpeedParam != null && float.TryParse(returnSpeedParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedReturnSpeed))
        {
            returnSpeed = parsedReturnSpeed;
        }

        var returnToStartParam = parameters?.FirstOrDefault(p => p.name == "returnToStart" && p.type == "bool");
        if (returnToStartParam != null && bool.TryParse(returnToStartParam.value, out bool parsedReturnToStart))
        {
            returnToStart = parsedReturnToStart;
        }

        var closeEnoughParam = parameters?.FirstOrDefault(p => p.name == "closeEnough" && p.type == "float");
        if (closeEnoughParam != null && float.TryParse(closeEnoughParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedCloseEnough))
        {
            closeEnough = parsedCloseEnough;
        }

        var decayParam = parameters?.FirstOrDefault(p => p.name == "decay" && p.type == "float");
        if (decayParam != null && float.TryParse(decayParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedDecay))
        {
            decay = parsedDecay;
        }
    }

    #endregion
}