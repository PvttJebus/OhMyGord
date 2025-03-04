using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    Vector2 maxVelocity = new Vector2(3,3);
    float speedRatio = 0.7f;
    float decay = 0.97f;
    Rigidbody2D body;
    Ray2D ray;

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        ray = new Ray2D(transform.position, Vector2.down);
    }

    // Update is called once per frame
    void Update()
    {
        body.velocity = new Vector2(body.velocity.x * decay, body.velocity.y * decay);
        if (up() && body.velocity.y < maxVelocity.y)
        {    
            body.AddForce(Vector2.up * maxVelocity.y * speedRatio);
        }
        if (down() && body.velocity.y > -maxVelocity.y * speedRatio)
        {
            body.AddForce(Vector2.down * maxVelocity.y * speedRatio);
        }
        if (left() && body.velocity.x > -maxVelocity.x * speedRatio)
        {
            body.AddForce(Vector2.left * maxVelocity.x * speedRatio);
        }   
        if (right() && body.velocity.x < maxVelocity.x * speedRatio)
        {
            body.AddForce(Vector2.right * maxVelocity.x * speedRatio);
        }
    }

    bool grounded()
    {
        return false;
    }

    bool up()
    {
        if (Input.GetKey(KeyCode.W))
        {
            return true;
        }
        return false;
    }

    bool down()
    {
        if (Input.GetKey(KeyCode.S))
        {
            return true;
        }
        return false;
    }

    bool left()
    {
        if (Input.GetKey(KeyCode.A))
        {
            return true;
        }
        return false;
    }

    bool right()
    {
        if (Input.GetKey(KeyCode.D))
        {
            return true;
        }
        return false;
    }
}
