// InteractableGroup.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Groups multiple Interactable objects to toggle them together.
/// </summary>
public class InteractableGroup : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private List<Interactable> interactables = new List<Interactable>();

    #endregion

    #region Unity Methods

    // No longer depends on transform children; membership is managed explicitly.
    private void Start()
    {
        // Optionally: validate group on start
        RemoveNullMembers();
    }

    #endregion

    #region Group Methods

    /// <summary>
    /// Toggles all interactables in this group.
    /// </summary>
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

    /// <summary>
    /// Adds an interactable to the group.
    /// </summary>
    public void AddMember(Interactable interactable)
    {
        if (interactable != null && !interactables.Contains(interactable))
        {
            interactables.Add(interactable);
            interactable.group = this;
        }
    }

    /// <summary>
    /// Removes an interactable from the group.
    /// </summary>
    public void RemoveMember(Interactable interactable)
    {
        if (interactable != null && interactables.Contains(interactable))
        {
            interactables.Remove(interactable);
            if (interactable.group == this)
                interactable.group = null;
        }
    }

    /// <summary>
    /// Removes any null members from the group.
    /// </summary>
    public void RemoveNullMembers()
    {
        interactables.RemoveAll(i => i == null);
    }

    /// <summary>
    /// Validates that all children have Interactable components.
    /// </summary>
    // No longer validates based on transform children.

    #endregion
}