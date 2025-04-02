// Door.cs
using UnityEngine;

public class Door : Interactable
{
    public bool isOpen = false;

    private Collider2D doorCollider;
    private SpriteRenderer doorRenderer;

    protected void Start()
    {
        doorCollider = GetComponent<Collider2D>();
        doorRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorState();
    }

    public override void OnToggle()
    {
        isOpen = !isOpen;
        UpdateDoorState();
    }

    private void UpdateDoorState()
    {
        doorCollider.enabled = !isOpen;
        doorRenderer.color = new Color(
            doorRenderer.color.r,
            doorRenderer.color.g,
            doorRenderer.color.b,
            isOpen ? 0.5f : 1f
        );
    }
}