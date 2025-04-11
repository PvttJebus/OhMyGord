using UnityEngine;

/// <summary>
/// Base class for dynamic, toggleable, and draggable environmental hazards.
/// </summary>
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D), typeof(SpriteRenderer))]

public class Hazard : Interactable, IParameterEditable
{
    [Header("Hazard Settings")]
    [SerializeField, Min(0.1f)]
    private float initialScale = 1f;

    public bool isActive = true;

    protected Collider2D damageCollider;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D body;

    protected bool isMouseOver = false;
    protected bool isDragging = false;

    private void Awake()
    {
        // Ensure the hazard starts with the correct uniform scale
        transform.localScale = Vector3.one * initialScale;
    }

    protected virtual void Start()
    {
        damageCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0; // Hazards are environment objects, no gravity
        UpdateHazardState();
    }

    /// <summary>
    /// Toggle hazard active state.
    /// </summary>
    public override void OnToggle()
    {
        isActive = !isActive;
        UpdateHazardState();
    }

    /// <summary>
    /// Update collider and visuals based on active state.
    /// </summary>
    protected virtual void UpdateHazardState()
    {
        damageCollider.enabled = isActive;
        spriteRenderer.color = new Color(
            spriteRenderer.color.r,
            spriteRenderer.color.g,
            spriteRenderer.color.b,
            isActive ? 1f : 0.5f
        );
    }

    protected virtual void Update()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        HandleMouseInput();
    }

    /// <summary>
    /// Handles mouse drag to reposition the hazard.
    /// </summary>
    protected virtual void HandleMouseInput()
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
                body.MovePosition(mousePos);
            }
            else
            {
                isDragging = false;
            }
        }
    }

    protected virtual void OnMouseEnter()
    {
        isMouseOver = true;
    }

    protected virtual void OnMouseExit()
    {
        isMouseOver = false;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            // TODO: Replace with your damage or kill logic
            Debug.Log("Player hit by hazard!");
        }
    }
    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        var scaleStr = transform.localScale.x.ToString(CultureInfo.InvariantCulture);
        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "scale",
                type = "float",
                value = scaleStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var param = parameters?.FirstOrDefault(p => p.name == "scale" && p.type == "float");
        if (param != null && float.TryParse(param.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale))
        {
            scale = Mathf.Max(0.1f, scale);
            transform.localScale = Vector3.one * scale;
        }
    }

    #endregion
}