using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Mover : Interactable
{
    private Rigidbody2D body;
    private Vector2 startPosition;
    private bool moving;

    [Header("Movement Settings")]
    public float moveSpeed = 15000f;
    public float moveSpeedReturn = 5000f;
    public float maxSpeed = 70000f;
    public bool returnToStart = true;
    [SerializeField] private float closeEnough = 0.1f;

    [Header("Audio")]
    public AudioSource moverAudio;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        startPosition = body.position;
    }

    public override void OnToggle()
    {
        moving = true;
        moverAudio.Play();
    }

    private void Update()
    {
        if (moving)
        {
            if (Input.GetMouseButtonUp(0))
            {
                moving = false;
                moverAudio.Stop();
            }
        }
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            ApplyMovementForce();
        }
        else if (returnToStart) ReturnToStart();

        body.velocity *= 0.9f;
    }

    private void ApplyMovementForce()
    {
        if (body.velocity.magnitude >= maxSpeed) return;

        Vector2 mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - body.position).normalized;
        body.AddForce(direction * moveSpeed);
    }

    private void ReturnToStart()
    {
        if ((startPosition - body.position).magnitude > closeEnough)
            body.AddForce((startPosition - body.position).normalized * moveSpeedReturn);
    }
}