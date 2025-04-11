using UnityEngine;
using LevelEditorSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: Selecting
    /// </summary>
    public class SelectingState
    {
        private readonly LevelEditorController controller;
        // Removed: e alias, use LevelEditor.Instance directly
        private readonly LevelEditorContext context;

        public SelectingState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered SelectingState");

            // Always re-apply highlight to all selected objects
            foreach (var obj in context.selectedObjects)
            {
                if (obj != null)
                    context.ApplyHighlight(obj, LevelEditor.Instance.highlightMaterial);
            }
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited SelectingState");

            var keys = new List<LevelObject>(context.originalMaterials.Keys);
            foreach (var obj in keys)
            {
                if (obj != null)
                    context.RestoreHighlight(obj);
            }
        }

        public void OnUpdate()
        {
            bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (context.isStickySpawning && context.stickyObject != null)
            {
                controller.ChangeToStickySpawningState();
                return;
            }
            if (LevelEditor.Instance == null)
            {
                Debug.LogError("[FSM] SelectingState: LevelEditor.Instance is null!");
                return;
            }
            if (LevelEditor.CurrentMode != LevelEditor.EditorMode.Edit)
                return;

            if (!context.isSingleObjectDragging && Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorld = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                Vector3Int cellPos = LevelEditor.Instance.grid.WorldToCell(mouseWorld);

                bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

                bool clickedObject = false;
                LevelObject clickedObjRef = null;
                foreach (var obj in LevelEditor.Instance.GetLevelObjects())
                {
                    if (obj == null) continue;
                    Collider2D col = obj.GetComponent<Collider2D>();
                    if (col != null && col.OverlapPoint(mouseWorld))
                    {
                        clickedObject = true;
                        clickedObjRef = obj;
                        break;
                    }
                }

                if (ctrlHeld)
                {
                    if (clickedObject && clickedObjRef != null)
                    {
                        if (context.selectedObjects.Contains(clickedObjRef))
                        {
                            context.selectedObjects.Remove(clickedObjRef);
                            context.RestoreHighlight(clickedObjRef);
                        }
                    }
                    else
                    {
                        if (context.selectedTiles.Contains(cellPos))
                        {
                            context.selectedTiles.Remove(cellPos);
                        }
                    }
                    return;
                }

                // Single selection: click on object with no modifiers
                if (clickedObject && clickedObjRef != null)
                {
                    bool multiSelection = context.selectedObjects.Count > 1 || context.selectedTiles.Count > 0;
                    if (multiSelection && context.selectedObjects.Contains(clickedObjRef))
                    {
                        Debug.Log("[FSM] Transitioning to DraggingSelectedState (clicked selected object with multi-selection)");
                        controller.ChangeToDraggingSelectedState();
                        return;
                    }

                    context.selectedObjects.Clear();
                    context.selectedTiles.Clear();
                    context.selectedObjects.Add(clickedObjRef);
                    context.ApplyHighlight(clickedObjRef, LevelEditor.Instance.highlightMaterial);

                    // Show parameter window for single selection
                    if (LevelEditor.Instance.propertyPanelUI != null)
                        LevelEditor.Instance.propertyPanelUI.ShowProperties(clickedObjRef);

                    // Start dragging immediately on click
                    context.isSingleObjectDragging = true;
                    LevelEditor.Instance.singleObjectDragStartMouseWorldPos = mouseWorld;
                    LevelEditor.Instance.singleObjectOriginalPos = clickedObjRef.transform.position;
                    context.activeEditingObject = clickedObjRef;
                    Debug.Log($"[FSM] ActiveEditingObject set to {clickedObjRef.name}");
                    controller.ChangeToDraggingSelectedState();

                    context.isSelecting = false;
                    return;
                }

                // altHeld already declared above
                if (altHeld && !clickedObject)
                {
                    TileBase clickedTile = LevelEditor.Instance.tilemap.GetTile(cellPos);
                    if (clickedTile != null)
                    {
                        context.currentTile = clickedTile;
                        controller.ChangeToDrawingState();
                        return;
                    }
                }

                // Drag selection (rectangle)
                context.isSelecting = true;
                Vector2 mousePos = Input.mousePosition;
                context.currentDrag = (DragArea?) new DragArea(mousePos, mousePos);

                foreach (var kvp in context.originalMaterials)
                {
                    if (kvp.Key != null)
                        kvp.Key.GetComponent<Renderer>().material = kvp.Value;
                }
                context.originalMaterials.Clear();

                if (LevelEditor.Instance.selectionOverlayTilemap != null)
                    LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();

                context.selectedObjects.Clear();
                context.selectedTiles.Clear();
            }
            else if (context.isSelecting && Input.GetMouseButton(0) && context.currentDrag.HasValue && !context.isSingleObjectDragging && !context.isGroupMoving)
            {
                Vector2 mousePos = Input.mousePosition;
                var drag = context.currentDrag.Value;
                drag.endScreenPos = mousePos;
                context.currentDrag = drag;
            }

            // Check for group drag activation outside selection drag/release logic
            if (!context.isSingleObjectDragging && Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorld = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                Vector3Int cellPos = LevelEditor.Instance.grid.WorldToCell(mouseWorld);

                bool clickedSelectedTile = context.selectedTiles.Contains(cellPos);
                bool clickedSelectedObject = false;
                foreach (var obj in context.selectedObjects)
                {
                    if (obj == null) continue;
                    Collider2D col = obj.GetComponent<Collider2D>();
                    if (col != null && col.OverlapPoint(mouseWorld))
                    {
                        clickedSelectedObject = true;
                        break;
                    }
                }

                if (clickedSelectedTile || clickedSelectedObject)
                {
                    Debug.Log("[FSM] Transitioning to DraggingSelectedState (clicked inside selection)");
                    controller.ChangeToDraggingSelectedState();
                    return;
                }
            }
            else if (context.isSelecting && Input.GetMouseButtonUp(0) && context.currentDrag.HasValue && !context.isSingleObjectDragging && !context.isGroupMoving)
            {
                context.isSelecting = false;

                var drag = context.currentDrag.Value;
                context.currentDrag = null;

                Rect screenRect = drag.GetScreenRect();

                foreach (var obj in LevelEditor.Instance.GetLevelObjects())
                {
                    if (obj == null) continue;
                    var renderer = obj.GetComponent<Renderer>();
                    if (renderer == null) continue;

                    Bounds bounds = renderer.bounds;
                    Vector3 min = Camera.main.WorldToScreenPoint(bounds.min);
                    Vector3 max = Camera.main.WorldToScreenPoint(bounds.max);

                    // Invert Y to match GUI coordinates
                    min.y = Screen.height - min.y;
                    max.y = Screen.height - max.y;

                    Rect spriteScreenRect = Rect.MinMaxRect(
                        Mathf.Min(min.x, max.x),
                        Mathf.Min(min.y, max.y),
                        Mathf.Max(min.x, max.x),
                        Mathf.Max(min.y, max.y)
                    );

                    if (spriteScreenRect.Overlaps(screenRect, true))
                    {
                        context.selectedObjects.Add(obj);

                        if (renderer != null && LevelEditor.Instance.highlightMaterial != null)
                        {
                            context.ApplyHighlight(obj, LevelEditor.Instance.highlightMaterial);
                        }
                    }
                }

                BoundsInt cellBounds = drag.GetCellBounds(LevelEditor.Instance.grid, LevelEditor.Instance.GetTilemap());
                foreach (var pos in cellBounds.allPositionsWithin)
                {
                    if (LevelEditor.Instance.GetTilemap().HasTile(pos))
                    {
                        context.selectedTiles.Add(pos);

                        if (LevelEditor.Instance.selectionOverlayTilemap != null)
                        {
                            LevelEditor.Instance.selectionOverlayTilemap.SetTile(pos, LevelEditor.Instance.GetTilemap().GetTile(pos));
                            LevelEditor.Instance.selectionOverlayTilemap.SetColor(pos, new Color(1f, 1f, 0f, 0.5f));
                        }
                    }
        
                }
            }
        }
    }
}