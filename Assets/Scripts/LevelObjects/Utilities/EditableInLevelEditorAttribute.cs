using System;
using UnityEngine;

/// <summary>
/// Attribute to mark fields or properties as editable in the in-game level editor UI.
/// Supports optional metadata for display customization.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class EditableInLevelEditorAttribute : PropertyAttribute
{
    public string DisplayName { get; }
    public float Min { get; }
    public float Max { get; }
    public string Tooltip { get; }

    /// <summary>
    /// Marks a property as editable with optional metadata.
    /// </summary>
    /// <param name="displayName">Display name in the UI.</param>
    /// <param name="min">Minimum value (for numeric types).</param>
    /// <param name="max">Maximum value (for numeric types).</param>
    /// <param name="tooltip">Tooltip text.</param>
    public EditableInLevelEditorAttribute(string displayName = null, float min = float.NegativeInfinity, float max = float.PositiveInfinity, string tooltip = null)
    {
        DisplayName = displayName;
        Min = min;
        Max = max;
        Tooltip = tooltip;
    }
}