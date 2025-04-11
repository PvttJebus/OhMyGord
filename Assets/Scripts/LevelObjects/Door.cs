// Door.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>A toggleable door that changes collision and transparency when opened or closed.</summary>
public class Door : Interactable, IParameterEditable
{
    #region Fields

    /// <summary>Whether the door is currently open.</summary>
    public bool isOpen = false;

    private Collider2D doorCollider;
    private SpriteRenderer doorRenderer;

    #endregion

    #region Unity Methods

    /// <summary>Initialize door components and set initial state.</summary>
    protected void Start()
    {
        doorCollider = GetComponent<Collider2D>();
        doorRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorState();
    }

    #endregion

    #region Interactable Overrides

    /// <summary>Toggle the door open or closed.</summary>
    public override void OnToggle()
    {
        isOpen = !isOpen;
        UpdateDoorState();
    }

    #endregion

    #region Private Methods

    /// <summary>Updates collider and transparency based on open state.</summary>
    private void UpdateDoorState()
    {
        // Disable collider when open, enable when closed
        doorCollider.enabled = !isOpen;

        // Change transparency to indicate open/closed visually
        doorRenderer.color = new Color(
            doorRenderer.color.r,
            doorRenderer.color.g,
            doorRenderer.color.b,
            isOpen ? 0.5f : 1f
        );
    }

    #endregion

    #region IParameterEditable Implementation

    /// <summary>Export all editable parameters to a list of ParameterData for serialization.</summary>
    public List<LevelData.ParameterData> ExportParameters()
    {
        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "isOpen",
                type = "bool",
                value = isOpen.ToString(),
                version = 1
            }
        };
    }

    /// <summary>Import editable parameters from a list of ParameterData (deserialization).</summary>
    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var param = parameters?.FirstOrDefault(p => p.name == "isOpen" && p.type == "bool");
        if (param != null && bool.TryParse(param.value, out bool open))
        {
            isOpen = open;
            UpdateDoorState();
        }
    }

    #endregion
}