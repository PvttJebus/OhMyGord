using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    Rigidbody2D body;
    BoxCollider2D collider;
    float returnSpeed = 90;
    bool rotating = false;
    float rotationStart;
    float rotationLength = 70;
    float rotationSpeed = 300;
    float maxSpeed = 100;
    float closeEnough = 0.9f;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (body.rotation >= 360 - closeEnough)
        {
            body.rotation = 0;
        }
        if (body.rotation < 0)
        {
            body.rotation += 360;
        }
        if (Input.GetMouseButtonDown((int) MouseButton.Left))
        { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
             if (Physics2D.Raycast(ray.origin, ray.direction, 100).collider == collider)
            {
                rotating = true;
                rotationStart = GetClosestRightAngle();
                if (rotationStart >= 270)
                {
                    rotationStart -= 360;
                }
            }
        }

        if (rotating)
        {
            var rotationDistance = body.rotation - rotationStart;
            if (rotationDistance < rotationLength || rotationDistance > 270)
            {
                body.AddTorque(rotationSpeed);
            }
            if (rotationDistance >= 90 && rotationDistance <= 180)
            {
                rotating = false;
            }
        }
        else 
        {
            if (Mathf.Abs(GetClosestRightAngle() - body.rotation) < 5)
            {
                body.rotation = GetClosestRightAngle();
                body.angularVelocity = 0;  
            }
            else if (body.rotation > 270) body.AddTorque(returnSpeed);
            else
            {
                body.AddTorque((GetClosestRightAngle() - body.rotation) * returnSpeed);
            }
        }
    }

    float GetClosestRightAngle()
    {
        if (body.rotation < 45)
        {
            return 0;
        }
        else if (body.rotation < 135)
        {
            return 90;
        }
        else if (body.rotation < 225)
        {
            return 180;
        }
        else if (body.rotation < 315)
        {
            return 270;
        }
        else
        {
            return 0;
        }
    }
}
