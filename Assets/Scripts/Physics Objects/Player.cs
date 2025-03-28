using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D body;
    Ray2D ray;

    Vector2 maxSpeed = new Vector2(7, 3);
    float jumpCooldownTime = 0.2f; // Cooldown duration after landing
    float speedRatio = 1.5f;
    float stopSpeedRatio = 0.1f;
    float decay = 0.97f;
    float jumpForce = 90f;

    bool wasGrounded = false;
    float jumpCooldownTimer = 0f;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        ray = new Ray2D(transform.position, Vector2.down);
    }

    void FixedUpdate()
    {
        body.velocity = new Vector2(body.velocity.x * decay, body.velocity.y);

        // Movement input handling
        body.AddForce(Vector2.right * Input.GetAxis("Horizontal") * maxSpeed.x * speedRatio);
        body.AddForce(Vector2.up * Input.GetAxis("Vertical") * maxSpeed.y * speedRatio);

        bool isGrounded = grounded();

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

        // Jump only if grounded and cooldown has expired
        if (Input.GetButton("Jump") && isGrounded && jumpCooldownTimer <= 0)
        {
            Debug.Log("Jump");
            body.AddForce(Vector2.up * jumpForce);
        }

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

        spriteRenderer.transform.rotation = Quaternion.identity;

    }
    bool grounded()
    {
        var hit = Physics2D.Raycast(body.position + Vector2.down * 0.61f, Vector2.down, 0.001f);
        return hit.collider != null;
    }
}
