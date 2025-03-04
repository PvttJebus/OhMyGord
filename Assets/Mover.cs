using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mover : MonoBehaviour
{
    Rigidbody2D body;
    Collider2D collider;
    float moveSpeed = 90;
    bool moving = false;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton((int) MouseButton.Left))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(ray.origin, ray.direction, 100).collider == collider)
            {
                moving = true;
            }
        }
        else moving = false;

        if (moving) 
        { 
            var mouseToPlatform = (Vector2) (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
            body.AddForce(mouseToPlatform * Vector2.SqrMagnitude(mouseToPlatform) * moveSpeed); 
        }
    }
}
