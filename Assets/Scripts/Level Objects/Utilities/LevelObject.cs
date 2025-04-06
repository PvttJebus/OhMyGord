using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class LevelObject : MonoBehaviour
{
    public bool editing;
    public int originalPrefabInstanceID;
    public float currentScale = 1f;

    [Header("Scaling Constraints")]
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private Vector2 baseSpriteSize;
    private Vector2 baseColliderSize;
    private SpriteRenderer spriteRenderer;
    private Collider2D objCollider;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        objCollider = GetComponent<Collider2D>();
    }

    public void InitializeFromPrefab(GameObject prefab)
    {
        originalPrefabInstanceID = prefab.GetInstanceID();
        CacheBaseDimensions(prefab);
        ApplyScale(currentScale);
    }

    public void AdjustScale(float delta)
    {
        currentScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);
        ApplyScale(currentScale);
    }

    void CacheBaseDimensions(GameObject prefab)
    {
        var prefabSprite = prefab.GetComponent<SpriteRenderer>();
        var prefabCollider = prefab.GetComponent<Collider2D>();

        baseSpriteSize = prefabSprite.sprite.bounds.size;
        baseColliderSize = GetColliderBaseSize(prefabCollider);
    }

    void ApplyScale(float scale)
    {
        // Update sprite
        spriteRenderer.size = baseSpriteSize * scale;

        // Update collider
        UpdateColliderSize(scale);
    }

    Vector2 GetColliderBaseSize(Collider2D collider) => collider switch
    {
        BoxCollider2D box => box.size,
        CircleCollider2D circle => Vector2.one * circle.radius,
        CapsuleCollider2D capsule => capsule.size,
        _ => Vector2.zero
    };

    void UpdateColliderSize(float scale)
    {
        switch (objCollider)
        {
            case BoxCollider2D box:
                box.size = baseColliderSize * scale;
                break;
            case CircleCollider2D circle:
                circle.radius = baseColliderSize.x * scale;
                break;
            case CapsuleCollider2D capsule:
                capsule.size = baseColliderSize * scale;
                break;
        }
    }

    public abstract void Modify();
}