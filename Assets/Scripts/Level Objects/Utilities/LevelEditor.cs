using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class LevelEditor : MonoBehaviour
{
    public enum EditorState { Spawning, Drawing, Inactive }

    public static LevelEditor Instance { get; private set; }

    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase eraseTile;
    [SerializeField] private Camera editorCamera;
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private bool useGridSnapping = true;
    [SerializeField] private float doubleClickTime = 0.2f;
    [SerializeField][Range(1, 9)] private int brushSize = 1;
    [SerializeField] private float sizeSensitivity = 0.1f;

    private const int MaxBrushSize = 9;
    private const int MinBrushSize = 1;
    private readonly List<LevelObject> levelObjects = new List<LevelObject>();
    private Vector3Int lastCellPosition = new Vector3Int(int.MinValue, int.MinValue, 0);

    public EditorState CurrentState { get; private set; } = EditorState.Inactive;
    private TileBase currentTile;
    private Grid grid;
    private Vector3 lastPanPosition;
    private bool isPanning;
    private bool isDraggingTile;
    private float lastLeftClickTime = -1f;
    private TilesetManager tilesetManager;
    public LevelObjectPalette objectPalette;
    private int currentLevelIndex = -1;
    private bool ignoreNextMouseUp;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        grid = GetComponent<Grid>();
        tilesetManager = GetComponent<TilesetManager>();
        objectPalette = GetComponent<LevelObjectPalette>();
        ValidateComponents();
        brushSize = Mathf.Clamp(brushSize | 1, MinBrushSize, MaxBrushSize);
    }

    void ValidateComponents()
    {
        if (!tilemap) Debug.LogError($"[{nameof(LevelEditor)}] Tilemap missing!", this);
        if (!eraseTile) Debug.LogWarning($"[{nameof(LevelEditor)}] Erase Tile missing!", this);
        if (!editorCamera || !editorCamera.orthographic) Debug.LogError($"[{nameof(LevelEditor)}] Invalid camera!", this);
        if (!tilesetManager || !objectPalette) Debug.LogError($"[{nameof(LevelEditor)}] Manager/Palette missing!", this);
    }

    void Update()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt);

        if (!isOverUI)
        {
            UpdateBrushSize();
            if (Input.GetKeyDown(KeyCode.Return)) SaveLevel(LevelData.GetNextLevelFileName());
            if (Input.GetKeyDown(KeyCode.Tab)) CycleToNextLevel();
        }

        HandleCameraControls(ctrlHeld);

        if (isOverUI || isPanning) return;

        if (altHeld && Input.GetMouseButtonDown(0)) HandleAltClick();
        else if (!ctrlHeld && !altHeld && Input.GetMouseButtonDown(0) && CurrentState != EditorState.Drawing)
            TryStartSpawning();

        if (CurrentState == EditorState.Spawning) HandleObjectSpawning(ctrlHeld);
        else if (CurrentState == EditorState.Drawing) HandleTileEditing();

        if (ignoreNextMouseUp && Input.GetMouseButtonUp(0))
            ignoreNextMouseUp = false;
    }

    void UpdateBrushSize()
    {
        int sizeChange = 0;
        if (Input.GetKeyDown(KeyCode.UpArrow)) sizeChange = 2;
        else if (Input.GetKeyDown(KeyCode.DownArrow)) sizeChange = -2;

        if (sizeChange != 0)
            brushSize = Mathf.Clamp(brushSize + sizeChange, MinBrushSize, MaxBrushSize);
    }

    void HandleCameraControls(bool ctrlHeld)
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && !ctrlHeld)
        {
            float currentSize = editorCamera.orthographicSize;
            float speedFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(minZoom, maxZoom, currentSize));
            editorCamera.orthographicSize = Mathf.Clamp(
                currentSize - scroll * zoomSpeed * speedFactor,
                minZoom,
                maxZoom
            );
        }

        bool panInput = Input.GetMouseButton(2) || (ctrlHeld && Input.GetMouseButton(0));
        if (!isPanning && (Input.GetMouseButtonDown(2) || (ctrlHeld && Input.GetMouseButtonDown(0))))
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
        else if (isPanning && !panInput)
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastPanPosition;
            editorCamera.transform.Translate(
                new Vector3(-delta.x, -delta.y, 0) *
                (editorCamera.orthographicSize * 2f / editorCamera.pixelHeight),
                Space.World
            );
            lastPanPosition = Input.mousePosition;
        }
    }

    void HandleAltClick()
    {
        Vector3 worldPos = GetPreciseMouseWorldPosition();
        foreach (var col in Physics2D.OverlapPointAll(worldPos))
        {
            if (col.TryGetComponent(out LevelObject obj))
            {
                SpawnClone(obj);
                return;
            }
        }

        Vector3Int cell = GetMouseCellPosition();
        TileBase tile = tilemap.GetTile(cell);
        if (tile == null && eraseTile != null)
        {
            SetEraserState();
        }
        else if (tile != null)
        {
            SetDrawingState(tile);
        }
    }

    void TryStartSpawning()
    {
        foreach (var col in Physics2D.OverlapPointAll(GetPreciseMouseWorldPosition()))
        {
            if (col.TryGetComponent(out LevelObject obj) && !obj.editing)
            {
                obj.editing = true;
                SetSpawningState();
                ignoreNextMouseUp = true;
                return;
            }
        }
    }

    void HandleObjectSpawning(bool ctrlHeld)
    {
        foreach (var obj in levelObjects)
        {
            if (obj?.editing ?? false)
            {
                obj.transform.position = GetSnappedMousePosition();

                if (ctrlHeld && Input.GetAxis("Mouse ScrollWheel") is float scroll and not 0)
                {
                    obj.AdjustSize(new Vector2(
                        Input.GetKey(KeyCode.LeftShift) ? 0 : scroll,
                        Input.GetKey(KeyCode.LeftShift) ? scroll : 0
                    ) * sizeSensitivity);
                }

                if (!ignoreNextMouseUp && Input.GetMouseButtonUp(0))
                {
                    obj.editing = false;
                }
            }
        }
    }

    void HandleTileEditing()
    {
        Vector3Int cell = GetMouseCellPosition();
        if (!IsPositionVisible(cell)) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastLeftClickTime < doubleClickTime)
            {
                tilemap.SetTile(cell, null);
                tilemap.FloodFill(cell, currentTile);
                isDraggingTile = false;
                lastLeftClickTime = -1f;
            }
            else
            {
                lastLeftClickTime = Time.time;
                isDraggingTile = true;
                ApplyBrush(cell);
            }
        }
        else if (isDraggingTile && Input.GetMouseButton(0) && cell != lastCellPosition)
        {
            ApplyBrush(cell);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingTile = false;
        }

        lastCellPosition = cell;
    }

    void ApplyBrush(Vector3Int center)
    {
        int offset = (Mathf.Max(1, brushSize) - 1) / 2;
        for (int x = -offset; x <= offset; x++)
        {
            for (int y = -offset; y <= offset; y++)
            {
                tilemap.SetTile(
                    center + new Vector3Int(x, y, 0),
                    currentTile == eraseTile ? null : currentTile
                );
            }
        }
    }

    Vector3Int GetMouseCellPosition()
    {
        return grid.WorldToCell(GetPreciseMouseWorldPosition());
    }

    public Vector3 GetSnappedMousePosition()
    {
        return useGridSnapping
            ? grid.GetCellCenterWorld(GetMouseCellPosition())
            : GetPreciseMouseWorldPosition();
    }

    Vector3 GetPreciseMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = editorCamera.nearClipPlane + 10f;
        return editorCamera.ScreenToWorldPoint(mousePos).WithZ(0);
    }

    bool IsPositionVisible(Vector3Int cell)
    {
        Vector3 viewportPos = editorCamera.WorldToViewportPoint(grid.GetCellCenterWorld(cell));
        return viewportPos.x >= 0f && viewportPos.x <= 1f &&
               viewportPos.y >= 0f && viewportPos.y <= 1f;
    }

    public void RegisterLevelObject(LevelObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to register null LevelObject.");
            return;
        }
        if (levelObjects.Contains(obj))
        {
            Debug.LogWarning($"LevelObject {obj.name} already registered.");
            return;
        }
        levelObjects.Add(obj);
    }

    public void SetSpawningState()
    {
        CurrentState = EditorState.Spawning;
        currentTile = null;
        isDraggingTile = false;
    }

    public void SetDrawingState(TileBase tile)
    {
        CurrentState = EditorState.Drawing;
        currentTile = tile;
        DisableAllEditing();
        isDraggingTile = false;
    }

    public void SetEraserState()
    {
        CurrentState = EditorState.Drawing;
        currentTile = eraseTile;
        DisableAllEditing();
        isDraggingTile = false;
    }

    void DisableAllEditing()
    {
        foreach (var obj in levelObjects)
        {
            if (obj != null)
            {
                obj.editing = false;
            }
        }
    }

    public void SaveLevel(string fileName)
    {
        LevelData.Save(fileName, tilemap, levelObjects, objectPalette.ObjectPrefabs);
    }

    public void LoadLevel(string fileName)
    {
        LevelData.Load(fileName, tilemap, levelObjects, tilesetManager, objectPalette.ObjectPrefabs);
    }

    void SpawnClone(LevelObject original)
    {
        GameObject newObj = Instantiate(original.gameObject, GetSnappedMousePosition(), Quaternion.identity);
        if (newObj.TryGetComponent(out LevelObject clone))
        {
            clone.originalPrefabInstanceID = original.originalPrefabInstanceID;
            RegisterLevelObject(clone);
            clone.editing = true;
            SetSpawningState();
        }
    }

    void CycleToNextLevel()
    {
        string nextLevel = LevelData.GetNextSavedLevel(ref currentLevelIndex);
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
            Debug.Log($"Loaded: {nextLevel}");
        }
        else
        {
            Debug.Log("No saved levels.");
        }
    }
}

public static class Extensions
{
    public static Vector3 WithZ(this Vector3 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static bool InViewport(this Vector3 v)
    {
        return v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f;
    }
}