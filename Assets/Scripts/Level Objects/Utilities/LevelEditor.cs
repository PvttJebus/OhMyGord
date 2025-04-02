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
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase eraseTile;
    [SerializeField] Camera editorCamera;

    [Header("Settings")]
    [SerializeField] float gridSnapSize = 1f;
    [SerializeField] bool useGridSnapping = true;
    [SerializeField] bool showGridDebug = true;
    [SerializeField] Color gridDebugColor = Color.cyan;

    public EditorState CurrentState { get; private set; } = EditorState.Inactive;
    private TileBase currentTile;
    private List<LevelObject> levelObjects = new List<LevelObject>();
    private Grid grid;
    private Vector3Int lastCellPosition;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        grid = GetComponent<Grid>();
        ValidateComponents();
    }

    void ValidateComponents()
    {
        if (!tilemap) Debug.LogError("Tilemap reference missing!", this);
        if (!editorCamera) Debug.LogError("Editor camera reference missing!", this);
        if (editorCamera.orthographic == false) Debug.LogError("Camera must be orthographic!", this);
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        switch (CurrentState)
        {
            case EditorState.Spawning:
                HandleObjectSpawning();
                break;

            case EditorState.Drawing:
                HandleTileEditing();
                break;
        }

        if (showGridDebug)
            DrawGridDebug();
    }

    void HandleObjectSpawning()
    {
        foreach (var obj in levelObjects)
        {
            if (!obj.editing) continue;

            obj.transform.position = GetSnappedMousePosition();

            if (Input.GetMouseButtonDown(0))
                obj.editing = false;
        }
    }

    void HandleTileEditing()
    {
        var currentCell = GetMouseCellPosition();

        if (Input.GetMouseButton(0) && IsPositionVisible(currentCell))
        {
            PaintTile(currentCell);
            lastCellPosition = currentCell;
        }
    }

    void PaintTile(Vector3Int cellPosition)
    {
        if (currentTile == null) return;

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
        return useGridSnapping ?
            grid.GetCellCenterWorld(grid.WorldToCell(worldPos)) :
            worldPos;
    }

    Vector3 GetPreciseMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(editorCamera.transform.position.z);
        Vector3 worldPos = editorCamera.ScreenToWorldPoint(mousePos);
        return new Vector3(worldPos.x, worldPos.y, 0);
    }

    bool IsPositionVisible(Vector3Int cellPosition)
    {
        Vector3 worldPos = grid.CellToWorld(cellPosition);
        Vector3 viewportPos = editorCamera.WorldToViewportPoint(worldPos);
        return viewportPos.x >= 0 && viewportPos.x <= 1 &&
               viewportPos.y >= 0 && viewportPos.y <= 1;
    }

    void DrawGridDebug()
    {
        var cell = GetMouseCellPosition();
        var center = grid.GetCellCenterWorld(cell);
        Debug.DrawLine(center + Vector3.left * 0.45f, center + Vector3.right * 0.45f, gridDebugColor);
        Debug.DrawLine(center + Vector3.up * 0.45f, center + Vector3.down * 0.45f, gridDebugColor);
    }

    public void RegisterLevelObject(LevelObject obj)
    {
        if (!levelObjects.Contains(obj))
            levelObjects.Add(obj);
    }

    public void SetSpawningState()
    {
        CurrentState = EditorState.Spawning;
        currentTile = null;
    }

    public void SetDrawingState(TileBase tile)
    {
        CurrentState = EditorState.Drawing;
        currentTile = tile;
        DisableAllEditing();
    }

    public void SetEraserState()
    {
        CurrentState = EditorState.Drawing;
        currentTile = eraseTile;
        DisableAllEditing();
    }

    void DisableAllEditing()
    {
        levelObjects.ForEach(obj => obj.editing = false);
    }
}