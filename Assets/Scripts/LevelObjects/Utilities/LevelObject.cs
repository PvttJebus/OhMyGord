using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public abstract class LevelObject : MonoBehaviour
{
    [Header("Editing State")]
    public bool editing;
    public int originalPrefabInstanceID;

    [Header("Size Configuration")]
    [SerializeField] private Vector2 _sizeMin = new Vector2(0.25f, 0.25f);
    [SerializeField] private Vector2 _sizeMax = new Vector2(4f, 4f);

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Vector2 _originalSpriteSize;
    private Vector2 _originalColliderSize;
    private Vector2 _currentSizeFactor = Vector2.one;

    #region Initialization
    protected virtual void Awake()
    {
        InitializeComponents();
        CacheOriginalSizes();
    }

    private void InitializeComponents()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();

        if (_spriteRenderer == null)
            Debug.LogError($"Missing SpriteRenderer on {name}", this);

        if (_collider == null)
            Debug.LogError($"Missing Collider2D on {name}", this);
    }

    private void CacheOriginalSizes()
    {
        if (TryGetOriginalPrefabSizes(out var prefabSpriteSize, out var prefabColliderSize))
        {
            _originalSpriteSize = prefabSpriteSize;
            _originalColliderSize = prefabColliderSize;
        }
        else
        {
            _originalSpriteSize = _spriteRenderer.size;
            _originalColliderSize = GetCurrentColliderSize();
        }
    }

    private bool TryGetOriginalPrefabSizes(out Vector2 spriteSize, out Vector2 colliderSize)
    {
        spriteSize = Vector2.zero;
        colliderSize = Vector2.zero;

        if (originalPrefabInstanceID == 0) return false;
        if (LevelEditor.Instance?.objectPalette == null) return false;

        foreach (var prefab in LevelEditor.Instance.objectPalette.ObjectPrefabs)
        {
            if (prefab.GetInstanceID() == originalPrefabInstanceID)
            {
                var prefabSR = prefab.GetComponent<SpriteRenderer>();
                var prefabCol = prefab.GetComponent<Collider2D>();

                if (prefabSR != null) spriteSize = prefabSR.size;
                if (prefabCol != null) colliderSize = GetColliderSize(prefabCol);
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Size Modification
    public void AdjustSize(Vector2 delta)
    {
        if (_spriteRenderer == null || _collider == null) return;

        _currentSizeFactor += delta;
        _currentSizeFactor.x = Mathf.Clamp(_currentSizeFactor.x, _sizeMin.x, _sizeMax.x);
        _currentSizeFactor.y = Mathf.Clamp(_currentSizeFactor.y, _sizeMin.y, _sizeMax.y);

        ApplySize();
    }

    private void ApplySize()
    {
        // Update sprite
        _spriteRenderer.size = new Vector2(
            _originalSpriteSize.x * _currentSizeFactor.x,
            _originalSpriteSize.y * _currentSizeFactor.y
        );

        // Update collider
        SetColliderSize(_originalColliderSize * _currentSizeFactor);
    }
    #endregion

    #region Collider Handling
    private Vector2 GetCurrentColliderSize() => GetColliderSize(_collider);

    private Vector2 GetColliderSize(Collider2D collider)
    {
        return collider switch
        {
            BoxCollider2D box => box.size,
            CapsuleCollider2D capsule => capsule.size,
            CircleCollider2D circle => new Vector2(circle.radius * 2, circle.radius * 2),
            _ => collider.bounds.size
        };
    }

    private void SetColliderSize(Vector2 newSize)
    {
        switch (_collider)
        {
            case BoxCollider2D box:
                box.size = newSize;
                break;

            case CapsuleCollider2D capsule:
                capsule.size = newSize;
                break;

            case CircleCollider2D circle:
                circle.radius = Mathf.Max(newSize.x, newSize.y) / 2;
                break;

            default:
                Debug.LogWarning($"Unsupported collider type: {_collider.GetType().Name}", this);
                break;
        }
    }
    #endregion

    #region Abstract Implementation
    public abstract void Modify();
    #endregion

    #region Editor Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        _sizeMin.x = Mathf.Max(0.1f, _sizeMin.x);
        _sizeMin.y = Mathf.Max(0.1f, _sizeMin.y);
        _sizeMax.x = Mathf.Max(_sizeMin.x, _sizeMax.x);
        _sizeMax.y = Mathf.Max(_sizeMin.y, _sizeMax.y);
    }
#endif
    #endregion
}