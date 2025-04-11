using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using LevelEditorSystem;


[RequireComponent(typeof(Grid))]
public class LevelEditor : MonoBehaviour
{
    // FSM migration: EditorState enum removed, now handled by FSM states
    public enum EditorMode { Play, Edit }

    public static LevelEditor Instance { get; private set; }

    // Reference to FSM controller to trigger state transitions
    public LevelEditorController controller;

    public static EditorMode CurrentMode { get; private set; } = EditorMode.Edit;

    public EditorHistory History => history;
    public static void SetMode(EditorMode mode)
    {
        CurrentMode = mode;
        if (Instance != null && Instance.editorCanvas != null)
            Instance.editorCanvas.SetActive(mode == EditorMode.Edit);
    }

    public Tilemap GetTilemap() => tilemap;
    public List<LevelObject> GetLevelObjects() => levelObjects;
    public TilesetManager GetTilesetManager() => tilesetManager;
    public GameObject[] GetPrefabList() => objectPalette.ObjectPrefabs;

    [Header("Editor UI Root")]
    public GameObject editorCanvas;
    public Button selectButton;
    public Button eraserButton;
    public Button increaseBrushButton;
    public Button decreaseBrushButton;

    [Header("Groups UI")]
    public Button groupsButton;

    [Header("In-Editor UI")]
    public PropertyPanelUI propertyPanelUI;

    [SerializeField] public Tilemap tilemap;
    public Tilemap selectionOverlayTilemap;
    [SerializeField] public Material highlightMaterial;
    [Header("Group Highlight")]
    [SerializeField] public Material groupHighlightMaterial;

    // FSM migration: originalMaterials removed, now handled by FSM states
    [SerializeField] public Camera editorCamera;
    [SerializeField] public float panSpeed = 20f;
    [SerializeField] public float zoomSpeed = 5f;
    [SerializeField] public float minZoom = 2f;
    [SerializeField] public float maxZoom = 20f;
    [SerializeField] public bool useGridSnapping = true;
    [SerializeField] public float doubleClickTime = 0.2f;
    [SerializeField][Range(1, 9)] public int brushSize = 1;
    [SerializeField] public float sizeSensitivity = 0.1f;

    private const int MaxBrushSize = 9;
    private const int MinBrushSize = 1;

    public readonly List<LevelObject> levelObjects = new();
    // FSM migration: selectedObjects removed, now handled by FSM states
    // FSM migration: selectedTiles removed, now handled by FSM states
    // FSM migration: currentDrag removed, now handled by FSM states
    // FSM migration: groupMoveInProgress removed, now handled by FSM states

    // FSM migration: isSingleObjectDragging removed, now handled by FSM states
    public Vector3 singleObjectDragStartMouseWorldPos;
    public Vector3 singleObjectOriginalPos;
    // FSM migration: activeEditingObject removed, now handled by FSM states

    // FSM migration: isGroupMoving removed, now handled by FSM states
    // Removed: groupMoveStartMousePos now handled by FSM context
    // FSM migration: groupMoveStartMouseWorldPos removed, now handled by FSM states
    // Removed: originalObjectPositions now handled by FSM context
    // FSM migration: originalObjectWorldPositions removed, now handled by FSM states
    // FSM migration: originalTileData removed, now handled by FSM states
    // FSM migration: groupMoveDelta removed, now handled by FSM states

    public Vector3Int lastCellPosition = new(int.MinValue, int.MinValue, 0);
    private EditorHistory history = new();
    public LevelObjectPalette objectPalette;
    public LevelSaveLoadUI saveLoadUI;

    public Grid grid;
    public Vector3 lastPanPosition;
    public bool isPanning;
    // FSM migration: isDraggingTile removed, now handled by FSM states
    // FSM migration: lastLeftClickTime removed, now handled by FSM states
    public TilesetManager tilesetManager;

    // FSM migration: CurrentState removed, FSM now manages editor interaction state
    // FSM migration: currentTile removed, now handled by FSM states

    // Removed: currentLevelIndex unused
    // Removed: ignoreNextMouseUp now handled by FSM context

    // Removed: sticky spawn flags now handled by FSM context

    // FSM migration: isSelecting removed, now handled by FSM states
    // Removed: lastPlacementTime and placementCooldown now handled by FSM context
    // Removed: floodFillTargetTile now handled by FSM context
    // Removed: selection rectangle now handled by FSM context

    public Camera previewCamera;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Attach or find LevelEditorController on the same GameObject
        if (controller == null)
        {
            controller = GetComponent<LevelEditorController>();
            if (controller == null)
            {
                controller = gameObject.AddComponent<LevelEditorController>();
            }
        }

        grid = GetComponent<Grid>();
        tilesetManager = GetComponent<TilesetManager>();
        objectPalette = GetComponent<LevelObjectPalette>();
        ValidateComponents();
        brushSize = Mathf.Clamp(brushSize | 1, MinBrushSize, MaxBrushSize);

        // Create overlay tilemap programmatically
        var overlayGO = new GameObject("SelectionOverlayTilemap");
        overlayGO.transform.SetParent(transform);
        overlayGO.transform.localPosition = Vector3.zero;

        var overlayGrid = overlayGO.AddComponent<Grid>();
        selectionOverlayTilemap = overlayGO.AddComponent<Tilemap>();
        var renderer = overlayGO.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = 10; // render above main tilemap

        if (saveLoadUI != null)
            saveLoadUI.gameObject.SetActive(false);

        if (selectButton != null)
            selectButton.onClick.AddListener(() => {
                if (controller != null)
                {
                    controller.ChangeToSelectingState();
                }
            });

        if (eraserButton != null)
            eraserButton.onClick.AddListener(() => {
                if (controller != null)
                {
                    controller.ChangeToErasingState();
                }
            });

        if (increaseBrushButton != null)
            increaseBrushButton.onClick.AddListener(IncreaseBrushSize);
if (decreaseBrushButton != null)
    decreaseBrushButton.onClick.AddListener(DecreaseBrushSize);

if (groupsButton != null)
    groupsButton.onClick.AddListener(EnterGroupEditingMode);
            decreaseBrushButton.onClick.AddListener(DecreaseBrushSize);
    }

    public void EnterGroupEditingMode()
    {
        if (controller != null)
        {
            controller.ChangeToGroupEditingState();
        }
    }

    void ValidateComponents()
    {
        if (!tilemap) Debug.LogError($"[{nameof(LevelEditor)}] Tilemap missing!", this);
        if (!editorCamera || !editorCamera.orthographic) Debug.LogError($"[{nameof(LevelEditor)}] Invalid camera!", this);
        if (!tilesetManager || !objectPalette) Debug.LogError($"[{nameof(LevelEditor)}] Manager/Palette missing!", this);
    }

    void Update()
    {
        // FSM migration: original erasing logic disabled

        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool altHeld = Input.GetKey(KeyCode.LeftAlt);

        // FSM migration: sticky spawn logic removed, now handled by FSM states



        // FSM migration: original selection logic disabled

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (CurrentMode == EditorMode.Edit)
            {
                SetMode(EditorMode.Play);
                Debug.Log("Switched to Play Mode");
            }
            else
            {
                SetMode(EditorMode.Edit);
                Debug.Log("Switched to Edit Mode");
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (saveLoadUI != null)
            {
                bool isActive = saveLoadUI.gameObject.activeSelf;
                saveLoadUI.gameObject.SetActive(!isActive);
            }
        }

        if (CurrentMode == EditorMode.Play)
            return;

        if (ctrlHeld)
        {
            if (Input.GetKeyDown(KeyCode.Z) && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                history.Undo(this);
            else if (Input.GetKeyDown(KeyCode.Y))
                history.Redo(this);
            else if (Input.GetKeyDown(KeyCode.Z) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                history.Redo(this);
        }

        if (!isOverUI)
            UpdateBrushSize();

        HandleCameraControls(ctrlHeld);

        if (isOverUI || isPanning) return;

        // FSM migration: selection, spawning, and tile editing input removed, now handled by FSM states

        // Removed: ignoreNextMouseUp logic now handled by FSM context
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
        if (scroll != 0f)
        {
            float currentSize = editorCamera.orthographicSize;
            float speedFactor = Mathf.Lerp(0.5f, 1.5f, Mathf.InverseLerp(minZoom, maxZoom, currentSize));
            editorCamera.orthographicSize = Mathf.Clamp(
                currentSize - scroll * zoomSpeed * speedFactor,
                minZoom,
                maxZoom
            );
        }

        bool panInput = Input.GetMouseButton(2);
        if (!isPanning && Input.GetMouseButtonDown(2))
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
            Vector3 delta = (Input.mousePosition - lastPanPosition) * panSpeed;
            editorCamera.transform.Translate(
                new Vector3(-delta.x, -delta.y, 0) *
                (editorCamera.orthographicSize * 2f / editorCamera.pixelHeight),
                Space.World
            );
            lastPanPosition = Input.mousePosition;
        }

        // WASD camera movement
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) move.y += 1;
        if (Input.GetKey(KeyCode.S)) move.y -= 1;
        if (Input.GetKey(KeyCode.A)) move.x -= 1;
        if (Input.GetKey(KeyCode.D)) move.x += 1;

        if (move != Vector3.zero)
        {
            float speed = panSpeed * Time.deltaTime;
            editorCamera.transform.Translate(move * speed, Space.World);
        }
    }

    // Removed: HandleAltClick now handled by FSM states

    // FSM migration: TryStartSpawning removed, now handled by FSM states

    // Removed HandleObjectSpawning sticky logic.
    // Object movement now handled via click-and-drag in Update()

    // FSM migration: HandleTileEditing removed, now handled by FSM DrawingTilesState and ErasingState

    /// <summary>
    /// Paints a brush of the given tile centered at the specified cell.
    /// </summary>
    /// <param name="center">Center cell position</param>
    /// <param name="tile">Tile to paint</param>
    public void ApplyBrush(Vector3Int center, TileBase tile)
    {
        int offset = (Mathf.Max(1, brushSize) - 1) / 2;
        for (int x = -offset; x <= offset; x++)
        {
            for (int y = -offset; y <= offset; y++)
            {
                tilemap.SetTile(
                    center + new Vector3Int(x, y, 0),
                    tile
                );
            }
        }
    }

    public Vector3Int GetMouseCellPosition()
    {
        return grid.WorldToCell(GetPreciseMouseWorldPosition());
    }

    public Vector3 GetSnappedMousePosition()
    {
        return useGridSnapping
            ? grid.GetCellCenterWorld(GetMouseCellPosition())
            : GetPreciseMouseWorldPosition();
    }

    public Vector3 GetPreciseMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = editorCamera.nearClipPlane + 10f;
        return editorCamera.ScreenToWorldPoint(mousePos).WithZ(0);
    }

    public bool IsPositionVisible(Vector3Int cell)
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

    // FSM migration: SetSpawningState, SetDrawingState, SetEraserState, and OnSelectButtonClicked removed.
    // These are now handled by FSM states and transitions via LevelEditorController.

    void DisableAllEditing()
    {
        foreach (var obj in levelObjects)
        {
            if (obj != null)
            {
                obj.editing = false;
            }
        }

        if (propertyPanelUI != null)
            propertyPanelUI.Clear();
    }

    public void SaveLevel(string fileName)
    {
        LevelData.Save(fileName, tilemap, levelObjects, objectPalette.ObjectPrefabs);
        LevelData.SaveLevelPreview(fileName, previewCamera);
    }

    public void LoadLevel(string fileName)
    {
        LevelData.Load(fileName, tilemap, levelObjects, tilesetManager, objectPalette.ObjectPrefabs);
    }

    private static Texture2D _dragRectTexture;

    void OnGUI()
    {
        if (controller != null && controller.context != null && controller.context.currentDrag.HasValue)
        {
            if (_dragRectTexture == null)
            {
                _dragRectTexture = new Texture2D(1, 1);
                _dragRectTexture.SetPixel(0, 0, new Color(0f, 0.5f, 1f, 0.25f)); // semi-transparent blue
                _dragRectTexture.Apply();
            }

            Rect rect = controller.context.currentDrag.Value.GetScreenRect();
            GUI.DrawTexture(rect, _dragRectTexture);
            GUI.color = Color.white; // reset GUI color
        }
    }

    // FSM migration: SpawnClone removed, now handled by FSM states


    public void IncreaseBrushSize()
    {
        brushSize = Mathf.Clamp(brushSize + 1, MinBrushSize, MaxBrushSize);
    }

    public void DecreaseBrushSize()
    {
        brushSize = Mathf.Clamp(brushSize - 1, MinBrushSize, MaxBrushSize);
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

public struct DragArea
{
    public Vector2 startScreenPos;
    public Vector2 endScreenPos;

    public DragArea(Vector2 start, Vector2 end)
    {
        startScreenPos = start;
        endScreenPos = end;
    }

    /// <summary>
    /// Gets the screen-space rectangle of the drag area, corrected for GUI coordinates.
    /// </summary>
    public Rect GetScreenRect()
    {
        float y1 = Screen.height - startScreenPos.y;
        float y2 = Screen.height - endScreenPos.y;

        Vector2 p1 = new Vector2(startScreenPos.x, y1);
        Vector2 p2 = new Vector2(endScreenPos.x, y2);

        Vector2 min = Vector2.Min(p1, p2);
        Vector2 max = Vector2.Max(p1, p2);

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    public Bounds GetWorldBounds(Camera camera, float zPlane = 0f)
    {
        Vector3 p1 = camera.ScreenToWorldPoint(new Vector3(startScreenPos.x, startScreenPos.y, camera.nearClipPlane + 10f));
        Vector3 p2 = camera.ScreenToWorldPoint(new Vector3(endScreenPos.x, endScreenPos.y, camera.nearClipPlane + 10f));

        p1.z = zPlane;
        p2.z = zPlane;

        Vector3 min = Vector3.Min(p1, p2);
        Vector3 max = Vector3.Max(p1, p2);

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public BoundsInt GetCellBounds(Grid grid, Tilemap tilemap)
    {
        Bounds worldBounds = GetWorldBounds(Camera.main);
        Vector3Int minCell = grid.WorldToCell(worldBounds.min);
        Vector3Int maxCell = grid.WorldToCell(worldBounds.max);

        Vector3Int min = Vector3Int.Min(minCell, maxCell);
        Vector3Int max = Vector3Int.Max(minCell, maxCell);

        return new BoundsInt(min, max - min + Vector3Int.one);
    }
}
