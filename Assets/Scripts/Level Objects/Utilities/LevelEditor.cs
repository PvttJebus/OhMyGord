using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class LevelEditor : MonoBehaviour
{
    public enum EditorState { Spawning, Drawing, Inactive }

    public static LevelEditor Instance { get; private set; }

    [Header("Core Components")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase eraseTile;
    [SerializeField] private Camera editorCamera;

    [Header("Settings")]
    [SerializeField] private bool useGridSnapping = true;

    [Header("Camera Controls")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Drawing Settings")]
    [SerializeField] private float doubleClickTime = 0.2f;
    [Tooltip("Initial brush size (odd numbers: 1, 3, 5...). Adjustable via Up/Down Arrows.")]
    [SerializeField][Range(1, 9)] private int brushSize = 1;
    private const int MaxBrushSize = 9;
    private const int MinBrushSize = 1;

    public EditorState CurrentState { get; private set; } = EditorState.Inactive;
    private TileBase currentTile;
    private List<LevelObject> levelObjects = new List<LevelObject>();
    private Grid grid;
    private Vector3Int lastCellPosition = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);

    private Vector3 lastPanPosition;
    private bool isPanning = false;
    private float lastLeftClickTime = -1f;
    private bool isDraggingTile = false;

    private TilesetManager tilesetManager;
    private LevelObjectPalette objectPalette;

    private int currentLevelIndex = -1;

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

        if (brushSize % 2 == 0) brushSize++;
        brushSize = Mathf.Clamp(brushSize, MinBrushSize, MaxBrushSize);
    }

    void ValidateComponents()
    {
        if (!tilemap) Debug.LogError($"[{nameof(LevelEditor)}] Tilemap reference missing!", this);
        if (!eraseTile) Debug.LogWarning($"[{nameof(LevelEditor)}] Erase Tile reference missing!", this);
        if (!editorCamera) Debug.LogError($"[{nameof(LevelEditor)}] Editor camera reference missing!", this);
        if (!editorCamera.orthographic) Debug.LogError($"[{nameof(LevelEditor)}] Editor camera must be orthographic!", this);
        if (!tilesetManager) Debug.LogError($"[{nameof(LevelEditor)}] TilesetManager component missing!", this);
        if (!objectPalette) Debug.LogError($"[{nameof(LevelEditor)}] LevelObjectPalette component missing!", this);
    }

    void Update()
    {
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();

        if (!isOverUI)
        {
            int sizeChange = 0;
            if (Input.GetKeyDown(KeyCode.UpArrow)) sizeChange = 2;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) sizeChange = -2;
            if (sizeChange != 0)
            {
                int newSize = brushSize + sizeChange;
                brushSize = Mathf.Clamp(newSize, MinBrushSize, MaxBrushSize);
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                string fileName = LevelData.GetNextLevelFileName();
                SaveLevel(fileName);
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                CycleToNextLevel();
            }
        }

        if (isOverUI)
        {
            HandleCameraControls();
            return;
        }

        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt);

        if (!ctrlHeld && altHeld && Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = GetPreciseMouseWorldPosition();
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out LevelObject obj))
                {
                    SpawnClone(obj);
                    return;
                }
            }
            Vector3Int cellPosition = GetMouseCellPosition();
            TileBase clickedTile = tilemap.GetTile(cellPosition);
            if (clickedTile == null && eraseTile != null)
            {
                SetEraserState();
            }
            else if (clickedTile != null)
            {
                SetDrawingState(clickedTile);
            }
            return;
        }

        if (!ctrlHeld && !altHeld && Input.GetMouseButtonDown(0) && CurrentState != EditorState.Drawing)
        {
            Vector3 worldPos = GetPreciseMouseWorldPosition();
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos);
            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out LevelObject obj) && !obj.editing)
                {
                    obj.editing = true;
                    SetSpawningState();
                    return;
                }
            }
        }

        HandleCameraControls();

        switch (CurrentState)
        {
            case EditorState.Spawning:
                HandleObjectSpawning();
                break;
            case EditorState.Drawing:
                HandleTileEditing();
                break;
        }
    }

    void HandleCameraControls()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float currentSize = editorCamera.orthographicSize;
            float speedFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(minZoom, maxZoom, currentSize));
            float adjustedZoomSpeed = zoomSpeed * speedFactor;
            float newSize = currentSize - scroll * adjustedZoomSpeed;
            editorCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }

        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool panInputDown = Input.GetMouseButtonDown(2) || (ctrlHeld && Input.GetMouseButtonDown(0));
        bool panInputHeld = Input.GetMouseButton(2) || (ctrlHeld && Input.GetMouseButton(0));

        if (panInputDown && !isPanning)
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
        else if (isPanning && !panInputHeld)
        {
            isPanning = false;
        }

        if (isPanning)
        {
            Vector3 delta = Input.mousePosition - lastPanPosition;
            float orthoFactor = (editorCamera.orthographicSize * 2f) / editorCamera.pixelHeight;
            Vector3 worldMove = new Vector3(-delta.x * orthoFactor, -delta.y * orthoFactor, 0);
            editorCamera.transform.Translate(worldMove, Space.World);
            lastPanPosition = Input.mousePosition;
        }
    }

    void SpawnClone(LevelObject original)
    {
        Vector3 spawnPos = GetSnappedMousePosition();
        GameObject newObj = Instantiate(original.gameObject, spawnPos, Quaternion.identity);
        if (newObj.TryGetComponent(out LevelObject levelObj))
        {
            levelObj.originalPrefabInstanceID = original.originalPrefabInstanceID;
            RegisterLevelObject(levelObj);
            DisableAllEditing();
            levelObj.editing = true;
            SetSpawningState();
        }
    }

    void HandleObjectSpawning()
    {
        if (isPanning) return;

        foreach (var obj in levelObjects)
        {
            if (obj != null && obj.editing)
            {
                obj.transform.position = GetSnappedMousePosition();
                if (Input.GetMouseButtonDown(0))
                {
                    obj.editing = false;
                }
            }
        }
    }

    void HandleTileEditing()
    {
        if (isPanning) { isDraggingTile = false; return; }

        Vector3Int currentCell = GetMouseCellPosition();

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastLeftClickTime < doubleClickTime)
            {
                if (IsPositionVisible(currentCell))
                {
                    tilemap.SetTile(currentCell, null);
                    tilemap.FloodFill(currentCell, currentTile);
                    isDraggingTile = false;
                    lastLeftClickTime = -1f;
                }
            }
            else
            {
                lastLeftClickTime = Time.time;
                isDraggingTile = true;
                if (IsPositionVisible(currentCell))
                {
                    ApplyBrush(currentCell);
                    lastCellPosition = currentCell;
                }
            }
        }
        else if (isDraggingTile && Input.GetMouseButton(0))
        {
            if (currentCell != lastCellPosition && IsPositionVisible(currentCell))
            {
                ApplyBrush(currentCell);
                lastCellPosition = currentCell;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDraggingTile = false;
        }
    }

    void ApplyBrush(Vector3Int centerCell)
    {
        int currentBrushSize = Mathf.Max(1, brushSize);
        int offset = (currentBrushSize - 1) / 2;

        for (int x = -offset; x <= offset; x++)
        {
            for (int y = -offset; y <= offset; y++)
            {
                Vector3Int targetCell = centerCell + new Vector3Int(x, y, 0);
                PaintTile(targetCell);
            }
        }
    }

    void PaintTile(Vector3Int cellPosition)
    {
        if (currentTile == eraseTile)
        {
            tilemap.SetTile(cellPosition, null);
        }
        else
        {
            tilemap.SetTile(cellPosition, currentTile);
        }
    }

    Vector3Int GetMouseCellPosition()
    {
        return grid.WorldToCell(GetPreciseMouseWorldPosition());
    }

    public Vector3 GetSnappedMousePosition()
    {
        Vector3 worldPos = GetPreciseMouseWorldPosition();
        return useGridSnapping ? grid.GetCellCenterWorld(grid.WorldToCell(worldPos)) : worldPos;
    }

    Vector3 GetPreciseMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = editorCamera.nearClipPlane + 10f;
        Vector3 worldPos = editorCamera.ScreenToWorldPoint(mousePos);
        return new Vector3(worldPos.x, worldPos.y, 0f);
    }

    bool IsPositionVisible(Vector3Int cellPosition)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(cellPosition);
        Vector3 viewportPos = editorCamera.WorldToViewportPoint(worldPos);
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
            if (obj != null) obj.editing = false;
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

    private void CycleToNextLevel()
    {
        string nextLevel = LevelData.GetNextSavedLevel(ref currentLevelIndex);
        if (nextLevel != null)
        {
            LoadLevel(nextLevel);
            Debug.Log($"Loaded level: {nextLevel}");
        }
        else
        {
            Debug.Log("No saved levels to cycle through.");
        }
    }
}