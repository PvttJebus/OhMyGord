using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int gapCheckFrames = 20;

    private int moveDir = -1;
    public Rigidbody2D body;
    private int turnCooldown = 0;
    private Vector3 originalScale;
    private int edgeMissCount = 0;
    public Animator animator;
    private bool isMouseOver;
    public bool isDragging;

    private void Start()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        animator.Play("EnemyWalk");
    }

    private void Update()
    {
        HandleMouseInput();

    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && isMouseOver)
        {
            isDragging = true;
            body.velocity = Vector2.zero; // Stop movement immediately
            // Fire trigger in animator to switch to ControlState
            animator.SetTrigger("toControl");
        }

        // If we are in "control" state, you'll move the enemy in ControlState.cs
        // but we can still detect the mouse release here:
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            // Probably we want to set "toFalling" so the enemy transitions to FallingState
            // or maybe we check if it’s over ground vs. falling, etc.
            animator.SetTrigger("toFalling");
        }
    }

    /// <summary>
    /// Called by PatrolState to handle walking. 
    /// (Originally from your Enemy.HandleMovement())
    /// </summary>
    public void PatrolMovement()
    {
        // 1) Raycast in front to detect walls
        bool hitWall = CheckFrontWall();

        // 2) Raycast downward to detect ground
        bool edgeCheck = Physics2D.Raycast(body.position + new Vector2(0.7f * moveDir, 0),
                                           Vector2.down, 2f);

        Debug.DrawRay(body.position + new Vector2(1.2f * moveDir, 0),
                      Vector2.down * 2, Color.red);

        bool shouldTurn = false;

        // If we detect a wall and no cooldown left
        if (hitWall && turnCooldown <= 0)
        {
            shouldTurn = true;
        }
        // If no ground under us, increment edgeMiss
        else if (!edgeCheck)
        {
            edgeMissCount++;
            if (edgeMissCount >= gapCheckFrames && turnCooldown <= 0)
            {
                shouldTurn = true;
            }
        }
        // Otherwise we reset edgeMiss if we see ground
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

        // Apply horizontal force, but clamp velocity
        if (Mathf.Abs(body.velocity.x) < moveSpeed)
        {
            body.AddForce(new Vector2(moveForce * moveDir, 0));
        }
    }

    public bool IsFalling()
    {
        float rayLength = 0.2f;
        var hit = Physics2D.Raycast(body.position, Vector2.down, rayLength);
        return (hit.collider == null);  // no ground => falling
    }

    private bool CheckFrontWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            body.position + new Vector2(moveDir * 0.7f, 0),
            new Vector2(moveDir, 0),
            0.1f
        );
        return hit.collider != null;
    }

    private void TurnAround()
    {
        // Zero out horizontal velocity
        body.velocity = new Vector2(0, body.velocity.y);

        // Flip direction
        moveDir *= -1;
        turnCooldown = 20;
        edgeMissCount = 0;

        // Flip sprite
        transform.localScale = new Vector3(originalScale.x * moveDir, originalScale.y, originalScale.z);
    }

    private void OnMouseEnter() { isMouseOver = true; }
    private void OnMouseExit() { isMouseOver = false; }
}