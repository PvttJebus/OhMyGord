using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class Interactable : MonoBehaviour
{
    [HideInInspector] public InteractableGroup group;

    private void OnMouseDown()
    {
        // Check if the left mouse button was clicked
        if (Input.GetMouseButtonDown(0))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        OnToggle();
        ToggleGroup();
    }

    public abstract void OnToggle();

    public void ToggleGroup()
    {
        if (group != null) group.ActivateAll();
    }
}