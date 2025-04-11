using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

/// <summary>
/// Falling spikes hazard that drops down to damage the player.
/// Player 2 can catch or reposition the spikes mid-air to protect Player 1.
/// </summary>
public class FallingSpikes : Hazard
{

    private Vector2 initialPosition;
    private bool hasFallen = false;
 
    [Header("Falling Settings")]
    public float fallDelay = 1.0f; // Delay before falling after triggered
    private float fallTimer = 0f;


    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        body.gravityScale = 0; // Initially no gravity
    }

    protected override void Update()
    {
        base.Update();

        if (hasFallen) return;

        // If waiting to fall after triggered
        if (fallTimer > 0)
        {
            fallTimer -= Time.deltaTime;
            if (fallTimer <= 0)
            {
                StartFalling();
            }
            return;
        }

        // Raycast down to detect player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f);
        Debug.DrawRay(transform.position, Vector2.down * 20f, Color.red);

        if (hit.collider != null && !hasFallen)
        {
            Player player = hit.collider.GetComponent<Player>();
            if (player != null)
            {
                TriggerFall();
            }
        }
    }

    /// <summary>
    /// Call this to trigger the spikes to fall after a delay.
    /// </summary>
    public void TriggerFall()
    {
        if (hasFallen) return;
        fallTimer = fallDelay;
    }

    private void StartFalling()
    {
        hasFallen = true;
        body.gravityScale = 3f; // Enable gravity to fall
    }

    /// <summary>
    /// Reset spikes to initial position and state.
    /// </summary>
    public void ResetSpikes()
    {
        hasFallen = false;
        fallTimer = 0;
        body.gravityScale = 0;
        body.velocity = Vector2.zero;
        transform.position = initialPosition;
    }

    protected override void UpdateHazardState()
    {
        base.UpdateHazardState();
        spriteRenderer.color = new Color(
            spriteRenderer.color.r,
            spriteRenderer.color.g,
            spriteRenderer.color.b,
            isActive ? 1f : 0.3f
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Debug.Log("Player hit by falling spikes!");
            // TODO: Damage or kill player
        }
    }
}