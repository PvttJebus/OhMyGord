using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCalculator : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public Vector3 lastPosition;
    public Vector3 velocity;
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void FixedUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);

        velocity = transform.position - lastPosition;
        lastPosition = transform.position;
        Debug.Log(velocity);

        if (velocity.x > 0.005f)
        {
            animator.Play("PlayerWalk");
            spriteRenderer.flipX = false;
        }
        else if (velocity.x < -0.005f)
        {
            animator.Play("PlayerWalk");
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.Play("PlayerIdle");
        }
    }
}
