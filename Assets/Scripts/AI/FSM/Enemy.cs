using UnityEngine;

public class Enemy : MonoBehaviour
{
    //This is the file I feel like is working smart not hard. 
    //This was the original enemy file that housed all the states in one, so I went in and adjusted it so I could use it as a reference file for each of the states.
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
        
    }

    //After the changes I'm certain this isn't needed, but I kept it just in case.
    private void Update()
    {
        HandleMouseInput();

    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && isMouseOver)
        {
            isDragging = true;
            body.velocity = Vector2.zero; 
            animator.SetTrigger("toControl");
        }

        
        if (Input.GetMouseButtonUp(0) && isDragging == true)
        {
            isDragging = false;
            
            animator.SetTrigger("toFalling");
        }
    }

    //I lifted everything that was relevant to general patrol and moved it into this function. 
    public void PatrolMovement()
    {
        
        bool hitWall = CheckFrontWall();

       
        bool edgeCheck = Physics2D.Raycast(body.position + new Vector2(0.7f * moveDir, 0),
                                           Vector2.down, 2f);

        Debug.DrawRay(body.position + new Vector2(1.2f * moveDir, 0),
                      Vector2.down * 2, Color.red);

        bool shouldTurn = false;

        
        if (hitWall && turnCooldown <= 0)
        {
            shouldTurn = true;
        }
        
        else if (!edgeCheck)
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

    //Same here fut for following
    public bool IsFalling()
    {
        float rayLength = 0.2f;
        var hit = Physics2D.Raycast(body.position, Vector2.down, rayLength);
        return (hit.collider == null);  
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

    private void OnMouseEnter() { isMouseOver = true; }
    private void OnMouseExit() { isMouseOver = false; }
}