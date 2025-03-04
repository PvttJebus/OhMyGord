using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 maxSpeed = new Vector2(7, 3);
    public float jumpCooldownTime = 0.2f; // Cooldown duration after landing
    float speedRatio = 1.5f;
    float stopSpeedRatio = 0.1f;
    float decay = 0.97f;
    public float jumpSpeed = 1.3f;
    Rigidbody2D body;
    Ray2D ray;
    Gamepad gamepad;

    private bool wasGrounded = false;
    private float jumpCooldownTimer = 0f;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        gamepad = Gamepad.current;  
        ray = new Ray2D(transform.position, Vector2.down);
    }

    void FixedUpdate()
    {
        body.velocity = new Vector2(body.velocity.x * decay, body.velocity.y * decay);
   

        // Movement input handling
        if (up() && body.velocity.y < maxSpeed.y)
        {
            body.AddForce(Vector2.up * maxSpeed.y * speedRatio);
        }
        if (down() && body.velocity.y > -maxSpeed.y * speedRatio)
        {
            body.AddForce(Vector2.down * maxSpeed.y * speedRatio);
        }
        if (left() && body.velocity.x > -maxSpeed.x * speedRatio)
        {
            body.AddForce(Vector2.left * maxSpeed.x * speedRatio);
        }
        if (right() && body.velocity.x < maxSpeed.x * speedRatio)
        {
            body.AddForce(Vector2.right * maxSpeed.x * speedRatio);
        }

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
        if (jump() && isGrounded && jumpCooldownTimer <= 0)
        {
            body.velocity += Vector2.up * jumpSpeed;
        }
    }
    bool grounded()
    {
        var hit = Physics2D.Raycast(body.position + Vector2.down * 0.61f, Vector2.down, 0.001f);
        return hit.collider != null;
    }

    bool up()
    {
        if (Input.GetKey(KeyCode.W))
        {
            return true;
        }
        if (gamepad != null)
        {
            if (gamepad.dpad.up.isPressed)
            {
                return true;
            }
        }
        return false;
    }

    bool down()
    {
        if (Input.GetKey(KeyCode.S))
        {
            return true;
        }
        if (gamepad != null)
        {
            if (gamepad.dpad.down.isPressed)
            {
                return true;
            }
        }
        return false;
    }

    bool left()
    {
        if (Input.GetKey(KeyCode.A))
        {
            return true;
        }
        if (gamepad != null)
        {
            if (gamepad.dpad.left.isPressed)
            {
                return true;
            }
        }
        return false;
    }

    bool right()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return true;
        }
        if (gamepad != null)
        {
            if (gamepad.dpad.right.isPressed)
            {
                return true;
            }
        }
        return false;
    }

    bool jump()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            return true;
        }
        if (gamepad != null)
        {
            if (gamepad.buttonSouth.isPressed)
            {
                return true;
            }
        }
        return false;
    }
}
