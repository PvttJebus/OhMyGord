using System;
using UnityEngine;
using Unity.VisualScripting;

public class SmoothRotator : MonoBehaviour
{
    Rigidbody2D body;
    Collider2D collider;
    bool rotating = false;
    float rotationStart;
    public float rotationSpeed = 1.5f;
    public float returnSpeed = 1.0f;
    public bool returnToStart = true;
    float closeEnough = 0.1f;
    public float decay = 0.9f;

    float targetAngle;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        rotationStart = body.rotation;
    }

    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetAngle = Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) * Mathf.Rad2Deg;

        if (Input.GetMouseButton((int)MouseButton.Left))
        {
            if (collider.OverlapPoint(mousePosition))
            {
                rotating = true;
            }
        }
        else
        {
            rotating = false;
        }
    }

    void FixedUpdate()
    {
        if (rotating)
        {
            float angleDiff = Mathf.DeltaAngle(body.rotation, targetAngle);
            float altAngleDiff = Mathf.DeltaAngle(body.rotation + 180, targetAngle);
            if (Mathf.Abs(altAngleDiff) < Mathf.Abs(angleDiff))
            {
                angleDiff = altAngleDiff;
            }
            if (Mathf.Abs(angleDiff) > closeEnough)
            {
                body.rotation = targetAngle;
            }
            else
            {
                body.AddTorque(angleDiff * rotationSpeed);
            }
        }
        else if (returnToStart)
        {
            float returnAngleDiff = Mathf.DeltaAngle(body.rotation, rotationStart);
            float altReturnAngleDiff = Mathf.DeltaAngle(body.rotation + 180, rotationStart);
            if (Mathf.Abs(returnAngleDiff) > closeEnough || altReturnAngleDiff > closeEnough)
            {
                if (Mathf.Abs(altReturnAngleDiff) < Mathf.Abs(returnAngleDiff))
                {
                    returnAngleDiff = altReturnAngleDiff;
                }
                body.AddTorque(returnAngleDiff * returnSpeed);
            }
        }

        body.angularVelocity *= decay;
    }
}