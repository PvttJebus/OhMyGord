using UnityEngine;

[RequireComponent(typeof(Collider2D))]
/// <summary>
/// Abstract base class for all interactable objects, supporting toggle and group activation.
/// </summary>
public abstract class Interactable : LevelObject
{
    #region Fields

    /// <summary>
    /// Optional group this interactable belongs to.
    /// </summary>
    [HideInInspector] public InteractableGroup group;

    #endregion

    #region Unity Methods

    private void OnMouseDown()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Toggle();
        }
    }

    #endregion


    #region Toggle Methods

    /// <summary>
    /// Toggles this interactable or its group if part of one.
    /// </summary>
    public void Toggle()
    {
        if (group != null)
        {
            group.ActivateAll();
        }
        else
        {
            OnToggle();
        }
    }

    /// <summary>
    /// Called when this interactable is toggled. Must be implemented by subclasses.
    /// </summary>
    public abstract void OnToggle();

    #endregion

    #region Editor Methods

    /// <summary>
    /// Called when modifying this object in the level editor.
    /// </summary>
    public override void Modify()
    {
       
    }

    #endregion
}