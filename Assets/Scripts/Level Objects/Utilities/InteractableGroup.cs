// InteractableGroup.cs
using System.Collections.Generic;
using UnityEngine;

public class InteractableGroup : MonoBehaviour
{
    [SerializeField] private bool includeInactive = false;

    private List<Interactable> interactables = new List<Interactable>();

    void Start()
    {
        RefreshGroupMembers();
        ValidateGroupStructure();
    }

    public void ActivateAll()
    {
        foreach (var interactable in interactables)
        {
            if (interactable != null)
            {
                interactable.OnToggle();
            }
        }
    }

    [ContextMenu("Refresh Group Members")]
    private void RefreshGroupMembers()
    {
        interactables.Clear();
        foreach (Transform child in transform)
        {
            var interactable = child.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactables.Add(interactable);
                interactable.group = this;
            }
        }
    }

    [ContextMenu("Validate Group Structure")]
    private void ValidateGroupStructure()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Interactable>() == null)
            {
                Debug.LogWarning($"Child {child.name} has no Interactable component!", child);
            }
        }
    }
}