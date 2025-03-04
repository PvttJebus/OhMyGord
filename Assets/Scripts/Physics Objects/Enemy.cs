using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private int gapCheckFrames = 20;

    private int moveDir = -1;
    private Rigidbody2D body;
    private int turnCooldown = 0;
    private Vector3 originalScale;
    private int edgeMissCount = 0;
    private Animator animator;
    private bool isMouseOver;
    private bool isDragging;

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

        if (!isDragging)
        {
            HandleMovement();
        }
    }

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

    private void HandleMovement()
    {
        Vector2 direction = new Vector2(moveDir, 0);
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

        if (Mathf.Abs(body.velocity.x) < moveSpeed)
        {
            body.AddForce(new Vector2(moveForce * moveDir, 0));
        }
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
        body.velocity = new Vector2(0, body.velocity.y);
        moveDir *= -1;
        turnCooldown = 20;
        edgeMissCount = 0;

        transform.localScale = new Vector3(originalScale.x * moveDir, originalScale.y, originalScale.z);
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
    }
}