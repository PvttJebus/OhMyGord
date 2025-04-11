using UnityEngine;
using LevelEditorSystem;
using UnityEngine.Tilemaps;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: DrawingTiles
    /// </summary>
    public class DrawingTilesState
    {
        private readonly LevelEditorController controller;
        // Removed: e alias, use LevelEditor.Instance directly
        private readonly LevelEditorContext context;

        public DrawingTilesState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered DrawingTilesState");
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited DrawingTilesState");
        }

        public void OnUpdate()
        {
            if (LevelEditor.Instance == null)
            {
                Debug.LogError("[FSM] DrawingTilesState: LevelEditor.Instance is null!");
                return;
            }

            bool isDrawing = LevelEditor.CurrentMode == LevelEditor.EditorMode.Edit;

            // Prevent drawing over UI
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                isDrawing = false;
            }

            Vector3Int cell = LevelEditor.Instance.GetMouseCellPosition();

            if (LevelEditor.Instance.selectionOverlayTilemap != null)
            {
                if (!isDrawing)
                {
                    LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();
                }
                else
                {
                    LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();

                    int brushSize = Mathf.Max(1, LevelEditor.Instance.brushSize);
                    int offset = (brushSize - 1);
                    for (int x = -offset; x <= offset; x++)
                    {
                        for (int y = -offset; y <= offset; y++)
                        {
                            Vector3Int pos = cell + new Vector3Int(x, y, 0);
                            LevelEditor.Instance.selectionOverlayTilemap.SetTile(pos, context.currentTile);
                            LevelEditor.Instance.selectionOverlayTilemap.SetColor(pos, new Color(1f, 1f, 1f, 0.5f));
                        }
                    }
                }
            }

            if (!isDrawing)
                return;

            if (!LevelEditor.Instance.IsPositionVisible(cell)) return;

            if (Input.GetMouseButton(1))
            {
                LevelEditor.Instance.tilemap.SetTile(cell, null);

                Vector3 worldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                foreach (var col in Physics2D.OverlapPointAll(worldPos))
                {
                    if (col.TryGetComponent(out LevelObject obj))
                    {
                        LevelEditor.Instance.GetLevelObjects().Remove(obj);
                        UnityEngine.Object.Destroy(obj.gameObject);
                    }
                }

                context.isDraggingTile = false;
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - context.lastLeftClickTime < LevelEditor.Instance.doubleClickTime)
                {
                    LevelEditor.Instance.tilemap.SetTile(cell, context.floodFillTargetTile);
                    LevelEditor.Instance.tilemap.FloodFill(cell, context.currentTile);
                    context.isDraggingTile = false;
                    context.lastLeftClickTime = -1f;
                }
                else
                {
                    context.floodFillTargetTile = LevelEditor.Instance.tilemap.GetTile(cell);

                    context.lastLeftClickTime = Time.time;
                    context.isDraggingTile = true;
                    LevelEditor.Instance.History.SaveState(LevelEditor.Instance);
                    ApplyBrush(cell, context.currentTile);
                }
            }
            else if (context.isDraggingTile && Input.GetMouseButton(0) && cell != LevelEditor.Instance.lastCellPosition)
            {
                ApplyBrush(cell, context.currentTile);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                context.isDraggingTile = false;
            }

            LevelEditor.Instance.lastCellPosition = cell;
        }
        private void ApplyBrush(Vector3Int center, TileBase tile)
        {
            int brushSize = Mathf.Max(1, LevelEditor.Instance.brushSize);
            int offset = (brushSize - 1);
            for (int x = -offset; x <= offset; x++)
            {
                for (int y = -offset; y <= offset; y++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, 0);

                    // Check if any LevelObject overlaps this cell
                    Vector3 worldPos = LevelEditor.Instance.grid.GetCellCenterWorld(pos);
                    bool hasObject = false;
                    foreach (var col in Physics2D.OverlapPointAll(worldPos))
                    {
                        if (col.TryGetComponent(out LevelObject _))
                        {
                            hasObject = true;
                            break;
                        }
                    }

                    if (!hasObject)
                    {
                        LevelEditor.Instance.tilemap.SetTile(pos, tile);
                    }
                }
            }
        }
    }
}