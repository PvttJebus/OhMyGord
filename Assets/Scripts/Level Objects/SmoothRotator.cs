using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SmoothRotator : Interactable
{
    private Rigidbody2D body;
    private float rotationStart;
    private bool rotating;

    [Header("Rotation Settings")]
    public float rotationSpeed = 15f;
    public float returnSpeed = 10f;
    public bool returnToStart = true;
    [SerializeField] private float closeEnough = 0.1f;
    [SerializeField] private float decay = 0.9f;

    private float targetAngle;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        rotationStart = body.rotation;
    }

    public override void OnToggle() => rotating = true;

    private void Update()
    {
        if (rotating)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetAngle = Mathf.Atan2(
                mousePosition.y - transform.position.y,
                mousePosition.x - transform.position.x
            ) * Mathf.Rad2Deg;
            if (Input.GetMouseButtonUp(0))
            {
                rotating = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rotating) HandleActiveRotation();
        else if (returnToStart) HandleReturnRotation();

        body.angularVelocity *= decay;
    }

    private void HandleActiveRotation()
    {
        float angleDiff = Mathf.DeltaAngle(body.rotation, targetAngle);
        float altAngleDiff = Mathf.DeltaAngle(body.rotation + 180, targetAngle);

        if (Mathf.Abs(altAngleDiff) < Mathf.Abs(angleDiff))
            angleDiff = altAngleDiff;

        body.AddTorque(Mathf.Abs(angleDiff) > closeEnough
            ? angleDiff * rotationSpeed
            : 0);
    }

    private void HandleReturnRotation()
    {
        float returnAngleDiff = Mathf.DeltaAngle(body.rotation, rotationStart);
        float altReturnAngleDiff = Mathf.DeltaAngle(body.rotation + 180, rotationStart);

        if (Mathf.Abs(altReturnAngleDiff) < Mathf.Abs(returnAngleDiff))
            returnAngleDiff = altReturnAngleDiff;

        if (Mathf.Abs(returnAngleDiff) > closeEnough)
            body.AddTorque(returnAngleDiff * returnSpeed);
    }
}