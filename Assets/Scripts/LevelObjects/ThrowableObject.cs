using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// Base class for objects that can be caught, held, and thrown by Player 2.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class ThrowableObject : MonoBehaviour, IParameterEditable
{
    protected Rigidbody2D body;
    protected Collider2D col;

    protected bool isCaught = false;
    protected bool isMouseOver = false;
    protected Vector2 lastMousePosition;

    [Header("Throw Settings")]
    public float throwForceMultiplier = 500f;

    protected virtual void Start()
    {
        body = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    protected virtual void Update()
    {
        if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Play) return;

        HandleMouseInput();
    }

    protected virtual void HandleMouseInput()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && isMouseOver)
        {
            Catch();
        }

        if (isCaught)
        {
            if (Input.GetMouseButton(0))
            {
                body.gravityScale = 0;
                body.velocity = Vector2.zero;
                body.MovePosition(mousePos);
                lastMousePosition = mousePos;
            }
            else
            {
                Vector2 throwDir = (mousePos - lastMousePosition).normalized;
                Release(throwDir * throwForceMultiplier);
            }
        }
    }

    /// <summary>
    /// Called when Player 2 grabs the object.
    /// </summary>
    public virtual void Catch()
    {
        isCaught = true;
        body.gravityScale = 0;
        body.velocity = Vector2.zero;
    }

    /// <summary>
    /// Called when Player 2 releases the object, applying throw force.
    /// </summary>
    /// <param name="throwForce">Force vector to apply.</param>
    public virtual void Release(Vector2 throwForce)
    {
        isCaught = false;
        body.gravityScale = 1;
        body.AddForce(throwForce);
    }

    protected virtual void OnMouseEnter()
    {
        isMouseOver = true;
    }

    protected virtual void OnMouseExit()
    {
        isMouseOver = false;
    }

    #region IParameterEditable Implementation

    public virtual List<LevelData.ParameterData> ExportParameters()
    {
        var throwForceMultiplierStr = throwForceMultiplier.ToString(CultureInfo.InvariantCulture);

        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "throwForceMultiplier",
                type = "float",
                value = throwForceMultiplierStr,
                version = 1
            }
        };
    }

    public virtual void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var throwForceMultiplierParam = parameters?.FirstOrDefault(p => p.name == "throwForceMultiplier" && p.type == "float");
        if (throwForceMultiplierParam != null && float.TryParse(throwForceMultiplierParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedThrowForceMultiplier))
        {
            throwForceMultiplier = parsedThrowForceMultiplier;
        }
    }

    #endregion
}