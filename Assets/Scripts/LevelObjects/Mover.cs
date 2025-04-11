using UnityEngine;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
/// <summary>
/// An interactable object that moves towards the mouse when toggled, and optionally returns to start.
/// </summary>

[AddComponentMenu("LevelObjects/Mover")]
public class Mover : Interactable, IParameterEditable
{
    #region Fields

    [SerializeField, Min(0.1f)]
    private float initialScale = 1f;

    private Rigidbody2D body;
    private Vector2 startPosition;
    private bool moving;

    [Header("Movement Settings")]

    /// <summary>
    /// Force applied when moving towards the mouse.
    /// </summary>
    public float moveSpeed = 9000f;
    /// <summary>
    /// Force applied when returning to the start position.
    /// </summary>
    public float moveSpeedReturn = 5000f;
    /// <summary>
    /// Maximum speed magnitude.
    /// </summary>
    public float maxSpeed = 30000f;
    /// <summary>
    /// Whether the mover returns to its start position when not active.
    /// </summary>
    
    public float throwForce = 500f; // Tune as needed
    
    public bool returnToStart = true;

    /// <summary>
    /// Direction of movement constraint.
    /// </summary>
    public enum MoveDirection { Horizontal, Vertical }
    [SerializeField]
    private MoveDirection moveDirection = MoveDirection.Horizontal;
    [SerializeField] private float closeEnough = 0.1f;

    [Header("Audio")]
    /// <summary>
    /// Audio source for movement sounds.
    /// </summary>
    public AudioSource moverAudio;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Ensure the mover starts with the correct uniform scale
        transform.localScale = Vector3.one * initialScale;
        body = GetComponent<Rigidbody2D>();
        ApplyMovementConstraint();
    }

    private void OnValidate()
    {
        // Reapply constraint if body is already initialized (i.e., not during prefab instantiation)
        if (body != null)
        {
            ApplyMovementConstraint();
        }
    }

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = body.position;
    }


    /// <summary>
    /// Called when this object is toggled (e.g., clicked).
    /// </summary>
    public override void OnToggle()
    {
        moving = true;
        moverAudio.Play();
    }

    private bool isDragging = false;
    private bool isMouseOver = false;
    private Vector2 lastMousePosition;

    private void Update()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        HandleMouseInput();

        if (moving && !isDragging)
        {
            // Stop moving when mouse button is released
            if (Input.GetMouseButtonUp(0))
            {
                moving = false;
                moverAudio.Stop();
            }
        }
    }

    private void HandleMouseInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && isMouseOver)
        {
            isDragging = true;
            body.velocity = Vector2.zero;
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 delta = mousePos - (Vector2)transform.position;
                if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play)
                {
                    startPosition = body.position;
                }
                lastMousePosition = mousePos;
            }
            else
            {
                // On release, throw in drag direction
                isDragging = false;
                Vector2 throwDirection = (mousePos - lastMousePosition).normalized;
                
                body.AddForce(throwDirection * throwForce);
            }
        }
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }

    private void FixedUpdate()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        if (moving)
        {
            ApplyMovementForce();
        }
        else if (returnToStart)
        {
            ReturnToStart();
        }

        // Apply damping to gradually slow down
        body.velocity *= 0.9f;
    }

    #endregion

    #region Movement Methods

    /// <summary>
    /// Applies force towards the mouse position.
    /// </summary>
    private void ApplyMovementForce()
    {
        if (body.velocity.magnitude >= maxSpeed) return;

        Vector2 mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - body.position).normalized;
        body.AddForce(direction * moveSpeed);
    }

    /// <summary>
    /// Returns the mover to its starting position if far enough away.
    /// </summary>
    private void ReturnToStart()
    {
        if ((startPosition - body.position).magnitude > closeEnough)
        {
            body.AddForce((startPosition - body.position).normalized * moveSpeedReturn);
        }
    }

    #endregion
    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        var scaleStr = transform.localScale.x.ToString(CultureInfo.InvariantCulture);
        var startPosStr = $"{startPosition.x.ToString(CultureInfo.InvariantCulture)},{startPosition.y.ToString(CultureInfo.InvariantCulture)}";
        var returnSpeedStr = moveSpeedReturn.ToString(CultureInfo.InvariantCulture);
        var massStr = (body != null ? body.mass : 1f).ToString(CultureInfo.InvariantCulture);

        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "scale",
                type = "float",
                value = scaleStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "startPosition",
                type = "vector2",
                value = startPosStr,
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
                name = "mass",
                type = "float",
                value = massStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        Debug.Log($"[Mover] ImportParameters called for {name}. Incoming parameters:");
        foreach (var p in parameters)
        {
            Debug.Log($"[Mover] Param: {p.name} ({p.type}) = {p.value}");
        }
        Debug.Log($"[Mover] startPosition BEFORE import: {startPosition}");

        var scaleParam = parameters?.FirstOrDefault(p => p.name == "scale" && p.type == "float");
        if (scaleParam != null && float.TryParse(scaleParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale))
        {
            scale = Mathf.Max(0.1f, scale);
            transform.localScale = Vector3.one * scale;
        }

        var startPosParam = parameters?.FirstOrDefault(p => p.name == "startPosition" && p.type == "vector2");
        if (startPosParam != null)
        {
            var parts = startPosParam.value.Split(',');
            if (parts.Length == 2 &&
                float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                Debug.Log($"[Mover] Parsed new startPosition: {x}, {y}");
                startPosition = new Vector2(x, y);
                if (body != null)
                {
                    body.position = startPosition;
                    transform.position = startPosition;
                }
                Debug.Log($"[Mover] startPosition set to: {startPosition}");
            }
            else
            {
                Debug.LogWarning($"[Mover] Failed to parse startPosition: {startPosParam.value}");
            }
        }

        var returnSpeedParam = parameters?.FirstOrDefault(p => p.name == "returnSpeed" && p.type == "float");
        if (returnSpeedParam != null && float.TryParse(returnSpeedParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float returnSpeed))
        {
            moveSpeedReturn = returnSpeed;
        }

        var massParam = parameters?.FirstOrDefault(p => p.name == "mass" && p.type == "float");
        if (massParam != null && float.TryParse(massParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float massValue))
        {
            if (body != null)
            {
                body.mass = Mathf.Max(0.01f, massValue);
            }
        }

        Debug.Log($"[Mover] startPosition AFTER import: {startPosition}");
    }

    private void ApplyMovementConstraint()
    {
        if (body == null) return;
        body.constraints &= ~(RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY);
        body.constraints |= moveDirection switch
        {
            MoveDirection.Horizontal => RigidbodyConstraints2D.FreezePositionY,
            MoveDirection.Vertical => RigidbodyConstraints2D.FreezePositionX,
            _ => RigidbodyConstraints2D.None
        };
    }

    #endregion
}