using UnityEngine;

/// <summary>
/// A simple throwable ball for testing hold and throw mechanics.
/// </summary>
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

[AddComponentMenu("LevelObjects/Ball")]
public class Ball : ThrowableObject, IParameterEditable
{
    [SerializeField]
    [Min(0.1f)]
    private float initialScale = 1f;

    private void Awake()
    {
        // Ensure the ball starts with the correct uniform scale
        transform.localScale = Vector3.one * initialScale;
    }

    // IParameterEditable implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        // Serialize uniform scale as a single float string
        var scaleStr = transform.localScale.x.ToString(CultureInfo.InvariantCulture);
        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "scale",
                type = "float",
                value = scaleStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var param = parameters?.FirstOrDefault(p => p.name == "scale" && p.type == "float");
        if (param != null && float.TryParse(param.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float scale))
        {
            scale = Mathf.Max(0.1f, scale); // Prevent zero or negative scale
            transform.localScale = Vector3.one * scale;
        }
    }
}