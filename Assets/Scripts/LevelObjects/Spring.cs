using UnityEngine;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>A spring that bounces the player upwards.</summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Spring : Interactable, IParameterEditable
{
    public float normalBounceForce = 300f;
    public float boostedBounceForce = 600f;
    public float boostDuration = 0.5f;

    private bool isBoosted = false;
    private float boostTimer = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<Collider2D>().isTrigger = true;
        UpdateVisual();
    }

    private void Update()
    {
        if (isBoosted)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                isBoosted = false;
                UpdateVisual();
            }
        }
    }

    public override void OnToggle()
    {
        isBoosted = true;
        boostTimer = boostDuration;
        UpdateVisual();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            if (playerBody != null)
            {
                float force = isBoosted ? boostedBounceForce : normalBounceForce;
                playerBody.velocity = new Vector2(playerBody.velocity.x, 0); // Reset vertical velocity
                playerBody.AddForce(Vector2.up * force);
                isBoosted = false;
                UpdateVisual();
            }
        }
    }

    private void UpdateVisual()
    {
        spriteRenderer.color = isBoosted ? Color.yellow : Color.white;
    }

    #region IParameterEditable Implementation

    public List<LevelData.ParameterData> ExportParameters()
    {
        var normalBounceForceStr = normalBounceForce.ToString(CultureInfo.InvariantCulture);
        var boostedBounceForceStr = boostedBounceForce.ToString(CultureInfo.InvariantCulture);
        var boostDurationStr = boostDuration.ToString(CultureInfo.InvariantCulture);

        return new List<LevelData.ParameterData>
        {
            new LevelData.ParameterData
            {
                name = "normalBounceForce",
                type = "float",
                value = normalBounceForceStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "boostedBounceForce",
                type = "float",
                value = boostedBounceForceStr,
                version = 1
            },
            new LevelData.ParameterData
            {
                name = "boostDuration",
                type = "float",
                value = boostDurationStr,
                version = 1
            }
        };
    }

    public void ImportParameters(List<LevelData.ParameterData> parameters)
    {
        var normalBounceForceParam = parameters?.FirstOrDefault(p => p.name == "normalBounceForce" && p.type == "float");
        if (normalBounceForceParam != null && float.TryParse(normalBounceForceParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedNormalBounceForce))
        {
            normalBounceForce = parsedNormalBounceForce;
        }

        var boostedBounceForceParam = parameters?.FirstOrDefault(p => p.name == "boostedBounceForce" && p.type == "float");
        if (boostedBounceForceParam != null && float.TryParse(boostedBounceForceParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedBoostedBounceForce))
        {
            boostedBounceForce = parsedBoostedBounceForce;
        }

        var boostDurationParam = parameters?.FirstOrDefault(p => p.name == "boostDuration" && p.type == "float");
        if (boostDurationParam != null && float.TryParse(boostDurationParam.value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedBoostDuration))
        {
            boostDuration = parsedBoostDuration;
        }
    }

    #endregion
}