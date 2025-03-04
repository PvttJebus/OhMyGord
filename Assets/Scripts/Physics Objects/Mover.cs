using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Mover : MonoBehaviour
{
    Rigidbody2D body;
    Collider2D collider;
    public float moveSpeed = 15000;
    public float moveSpeedReturn = 5000;
    public float maxSpeed = 70000;
    public  bool returnToStart = true;
    float closeEnough = 0.1f;
    bool moving = false;
    Vector2 startPosition;
    public AudioSource moverAudio;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        startPosition = body.position;
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton((int)MouseButton.Left))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics2D.Raycast(ray.origin, ray.direction, 100).collider == collider)
            {
                moving = true;
                if (!moverAudio.isPlaying) 
                {
                    moverAudio.Play();                
                }

            }

            
        }
        else moving = false;

        if (moving)
        {
            if (body.velocity.magnitude < maxSpeed)
            {
                var mouseToPlatform = (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position);
                body.AddRelativeForce(mouseToPlatform.normalized * moveSpeed);
            }
        }
        else if (returnToStart && (startPosition - body.position).magnitude > closeEnough)
        { 
                body.AddForce((startPosition - body.position).normalized * moveSpeedReturn);
                moverAudio.Stop();
        }
        else
        {
            body.velocity *= 0.9f;
            
        }

    }
}
