using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A pressure switch activated by any LevelObject, including Player and ThrowableObjects.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PressureSwitch : MonoBehaviour, IParameterEditable
{
    public bool isActive = false;
    private SpriteRenderer spriteRenderer;
    private int objectsOnSwitch = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<LevelObject>() != null)
        {
            objectsOnSwitch++;
            if (!isActive)
            {
                isActive = true;
                UpdateVisual();
                // TODO: Trigger connected objects
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<LevelObject>() != null)
        {
            objectsOnSwitch = Mathf.Max(0, objectsOnSwitch - 1);
            if (objectsOnSwitch == 0)
            {
                isActive = false;
                UpdateVisual();
                // TODO: Trigger connected objects
            }
        }
    }

    private void UpdateVisual()
    {
        spriteRenderer.color = isActive ? Color.green : Color.red;
    }

    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "isActive",
                type = "bool",
                value = isActive.ToString(),
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var param = parameters?.FirstOrDefault(p => p.name == "isActive" && p.type == "bool");
        if (param != null && bool.TryParse(param.value, out bool active))
        {
            isActive = active;
            UpdateVisual();
        }
    }

    #endregion
}