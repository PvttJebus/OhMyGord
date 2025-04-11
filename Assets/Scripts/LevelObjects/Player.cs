using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement, jumping, and animation.
/// </summary>
public class Player : LevelObject
{
    #region Fields

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private Ray2D ray;

    private Vector2 maxSpeed = new Vector2(7, 3);
    private float jumpCooldownTime = 0.2f; // Cooldown duration after landing
    private float speedRatio = 1.5f;
    private float stopSpeedRatio = 0.1f;
    private float decay = 0.97f;
    private float jumpForce = 90f;

    private bool wasGrounded = false;
    private float jumpCooldownTimer = 0f;

    [Header("Hold and Throw")]
    public Transform holdPoint;
    public float pickupRange = 2f;
    public float throwForce = 500f;
    private ThrowableObject heldObject;

    #endregion

    #region Unity Methods

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ray = new Ray2D(transform.position, Vector2.down);
    }

    private void FixedUpdate()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        // Apply horizontal velocity decay for smooth stopping
        body.velocity = new Vector2(body.velocity.x * decay, body.velocity.y);

        // Calculate horizontal input force
        Vector2 runForce;
        if (Math.Sign(0) == -Math.Sign(body.velocity.x))
        {
            runForce = Vector2.right * Input.GetAxis("Horizontal") * maxSpeed.x * stopSpeedRatio;
        }
        else
        {
            runForce = Vector2.right * Input.GetAxis("Horizontal") * maxSpeed.x * speedRatio;
        }
        body.AddForce(runForce);

        // Optional vertical bias force (e.g., ladders or vertical movement)
        Vector2 verticalBiasForce = Vector2.up * Input.GetAxis("Vertical") * maxSpeed.y * speedRatio;
        body.AddForce(verticalBiasForce);

        bool isGrounded = grounded();

        // Manage jump cooldown after landing
        if (isGrounded)
        {
            if (!wasGrounded)
            {
                jumpCooldownTimer = jumpCooldownTime;
            }
            else
            {
                jumpCooldownTimer -= Time.deltaTime;
                if (jumpCooldownTimer < 0) jumpCooldownTimer = 0;
            }
        }
        wasGrounded = isGrounded;

        // Jump if grounded and cooldown expired
        if (Input.GetButton("Jump") && isGrounded && jumpCooldownTimer <= 0)
        {
            Debug.Log("Jump");
            body.AddForce(Vector2.up * jumpForce);
        }

        // Handle animation and sprite flipping
        if (body.velocity.x > 0.05f)
        {
            animator.Play("PlayerWalk");
            spriteRenderer.flipX = false;
        }
        else if (body.velocity.x < -0.05f)
        {
            animator.Play("PlayerWalk");
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.Play("PlayerIdle");
        }

        // Prevent sprite rotation from physics
        spriteRenderer.transform.rotation = Quaternion.identity;
        // Handle hold and throw input
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                TryPickup();
            }
            else
            {
                DropHeldObject();
            }
        }

        if (heldObject != null && Input.GetKeyDown(KeyCode.F))
        {
            ThrowHeldObject();
        }
    }

    #endregion

    #region Custom Methods

    /// <summary>
    /// Checks if the player is grounded using a downward raycast.
    /// </summary>
    /// <returns>True if grounded, false otherwise.</returns>
    private bool grounded()
    {
        var hit = Physics2D.Raycast(body.position + Vector2.down * 0.61f, Vector2.down, 0.001f);
        return hit.collider != null;
    }

    /// <summary>
    /// Called when modifying the player in the level editor.
    /// </summary>
    public override void Modify()
    {
    }

    #endregion

    #region Hold and Throw Methods

    private void TryPickup()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);
        foreach (var hit in hits)
        {
            ThrowableObject throwable = hit.GetComponent<ThrowableObject>();
            if (throwable != null)
            {
                heldObject = throwable;
                heldObject.Catch();
                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                heldObject.GetComponent<Rigidbody2D>().gravityScale = 0;
                break;
            }
        }
    }

    private void DropHeldObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        heldObject.Release(Vector2.zero);
        heldObject = null;
    }

    private void ThrowHeldObject()
    {
        if (heldObject == null) return;

        heldObject.transform.SetParent(null);
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        heldObject.Release(direction * throwForce);
        heldObject = null;
    }

    #endregion
}
