using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

/// <summary>
/// Controls enemy patrolling, edge detection, wall detection, and drag interactions.
/// </summary>
[AddComponentMenu("LevelObjects/Enemy")]

public class Enemy : MonoBehaviour, IParameterEditable
{
    #region Fields

    /// <summary>
    /// Uniform scale for the enemy (2D only).
    /// </summary>
    [SerializeField, Min(0.1f)]
    private float initialScale = 1f;

    /// <summary>
    /// Force applied for horizontal movement.
    /// </summary>
    [SerializeField] private float moveForce = 5f;

    /// <summary>
    /// Maximum horizontal speed.
    /// </summary>
    [SerializeField] private float moveSpeed = 2f;

    /// <summary>
    /// Number of frames to check for edge before turning.
    /// </summary>
    [SerializeField] private int gapCheckFrames = 20;

    // Internal state
    private int moveDir = -1;
    private Rigidbody2D body;
    private int turnCooldown = 0;
    private Vector3 originalScale;
    private int edgeMissCount = 0;
    private Animator animator;
    private bool isMouseOver = false;
    private bool isDragging = false;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Ensure the enemy starts with the correct uniform scale
        transform.localScale = Vector3.one * initialScale;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        animator.Play("EnemyWalk");
    }

    private void Update()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        HandleMouseInput();

        if (!isDragging)
        {
            HandleMovement();
        }
    }

    #endregion

    #region Drag and Mouse

    /// <summary>
    /// Handles mouse drag to reposition the enemy.
    /// </summary>
    private void HandleMouseInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && isMouseOver)
        {
            isDragging = true;
            body.velocity = Vector2.zero; // Stop current movement
        }

        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                // Move using Rigidbody to maintain physics interaction
                body.MovePosition(mousePos);
            }
            else
            {
                isDragging = false;
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

    #endregion

    #region Movement and AI

    /// <summary>
    /// Handles patrolling movement, edge detection, and wall detection.
    /// </summary>
    private void HandleMovement()
    {
        // Check for ground ahead to avoid falling off edges
        RaycastHit2D edgeCheck = Physics2D.Raycast(
            body.position + new Vector2(0.7f * moveDir, 0),
            Vector2.down,
            2f
        );

        Debug.DrawRay(body.position + new Vector2(1.2f * moveDir, 0), Vector2.down * 2, Color.red);

        bool hitWall = CheckFrontWall();
        bool shouldTurn = false;

        if (hitWall && turnCooldown <= 0)
        {
            shouldTurn = true;
        }
        else if (edgeCheck.collider == null)
        {
            edgeMissCount++;
            if (edgeMissCount >= gapCheckFrames && turnCooldown <= 0)
            {
                shouldTurn = true;
            }
        }
        else
        {
            edgeMissCount = 0;
        }

        if (shouldTurn)
        {
            TurnAround();
        }
        else if (turnCooldown > 0)
        {
            turnCooldown--;
        }

        // Apply movement force if below max speed
        if (Mathf.Abs(body.velocity.x) < moveSpeed)
        {
            body.AddForce(new Vector2(moveForce * moveDir, 0));
        }
    }

    /// <summary>
    /// Checks for a wall directly in front of the enemy.
    /// </summary>
    /// <returns>True if wall detected, false otherwise.</returns>
    private bool CheckFrontWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            body.position + new Vector2(moveDir * 0.7f, 0),
            new Vector2(moveDir, 0),
            0.1f
        );
        return hit.collider != null;
    }

    /// <summary>
    /// Reverses enemy direction and resets cooldowns.
    /// </summary>
    private void TurnAround()
    {
        body.velocity = new Vector2(0, body.velocity.y);
        moveDir *= -1;
        turnCooldown = 20;
        edgeMissCount = 0;

        transform.localScale = new Vector3(originalScale.x * moveDir, originalScale.y, originalScale.z);
    }

    #endregion

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