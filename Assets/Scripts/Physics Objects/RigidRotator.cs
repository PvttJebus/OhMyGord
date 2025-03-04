using UnityEngine;

public class RigidRotator : MonoBehaviour
{
    Rigidbody2D body;
    Collider2D platformCollider;
    bool rotating = false;
    float rotation;
    float rotationStart;

    float rotationLength = 70;
    public float rotationSpeed = 9000;
    public float maxSpeed = 10000;
    public float returnSpeed = 7000;
    public bool reversed = false;
    float closeEnough = 0.9f;
    public AudioSource rotationAudio;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();
        SnapToNearestAngle();
    }

    void FixedUpdate()
    {
        HandleRotationInput();
        UpdateRotationState();
        ClampAngularVelocity();
    }

    void HandleRotationInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (platformCollider.OverlapPoint(mousePos))
            {
                if (rotating)
                {
                    rotating = false;
                    rotationAudio.Stop();
                }
                else
                {
                    rotating = true;
                    rotationStart = GetCurrentRotation();
                    if (rotationStart >= 270) rotationStart -= 360;
                    rotationAudio.Play();
                }
            }
        }
    }

    void UpdateRotationState()
    {
        rotation = GetCurrentRotation();

        if (rotating)
        {
            HandleActiveRotation();
        }
        else
        {
            HandleReturnRotation();
        }
    }

    float GetCurrentRotation()
    {
        if (reversed)
        {
            return (360 - body.rotation) % 360;
        }
        else
        {
            return body.rotation % 360;
        }
    }

    void HandleActiveRotation()
    {
        float rotationDistance = Mathf.Abs(rotation - rotationStart);

        if (rotationDistance < rotationLength || rotationDistance > 270)
        {
            if (reversed)
            {
                body.AddTorque(-rotationSpeed);
            }
            else
            {
                body.AddTorque(rotationSpeed);
            }
        }

        if (rotationDistance >= 90 - closeEnough && rotationDistance <= 270)
        {
            SnapToNearestAngle();
            rotating = false;
        }
    }

    void HandleReturnRotation()
    {
        float targetAngle = GetClosestRightAngle();
        float currentAngle = GetCurrentRotation();
        float difference = Mathf.DeltaAngle(currentAngle, targetAngle);

        if (Mathf.Abs(difference) < closeEnough)
        {
            SnapToNearestAngle();
        }
        else
        {
            if (reversed)
            {
                body.AddTorque(Mathf.Sign(difference) * returnSpeed * -1);
            }
            else
            {
                body.AddTorque(Mathf.Sign(difference) * returnSpeed);
            }
        }

        body.angularVelocity *= difference / 90;
    }

    void SnapToNearestAngle()
    {
        body.rotation = GetClosestRightAngle();
        body.angularVelocity = 0;
    }

    void ClampAngularVelocity()
    {
        body.angularVelocity = Mathf.Clamp(
            body.angularVelocity,
            -maxSpeed,
            maxSpeed
        );
    }

    float GetClosestRightAngle()
    {
        float currentRotation = body.rotation % 360;

        if (reversed)
        {
            float reversedRotation = (360 - currentRotation) % 360;

            if (reversedRotation < 45 || reversedRotation >= 315) return 360;
            if (reversedRotation < 135) return 270;
            if (reversedRotation < 225) return 180;
            return 90;
        }

        if (currentRotation < 45 || currentRotation >= 315) return 0;
        if (currentRotation < 135) return 90;
        if (currentRotation < 225) return 180;
        return 270;
    }
}