using UnityEngine;
using LevelEditorSystem;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: DraggingSelected
    /// Handles both single and multi-object dragging.
    /// </summary>
    public class DraggingSelectedState
    {
        private readonly LevelEditorController controller;
        private readonly LevelEditorContext context;

        private Vector3 dragStartMouseWorldPos;
        private Dictionary<LevelObject, Vector3> originalObjectPositions = new();
        private Dictionary<Vector3Int, TileBase> originalTileData = new();
        private Vector3Int groupMoveDelta = Vector3Int.zero;

        public DraggingSelectedState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered DraggingSelectedState");

            dragStartMouseWorldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();
            originalObjectPositions.Clear();
            originalTileData.Clear();

            // Save original positions and apply highlight
            foreach (var obj in context.selectedObjects)
            {
                if (obj != null)
                {
                    originalObjectPositions[obj] = obj.transform.position;
                    context.ApplyHighlight(obj, LevelEditor.Instance.highlightMaterial);
                }
            }

            // Save original tile data
            foreach (var pos in context.selectedTiles)
            {
                var tile = LevelEditor.Instance.tilemap.GetTile(pos);
                if (tile != null)
                {
                    originalTileData[pos] = tile;
                }
            }

            // Rebuild selection overlay preview
            if (LevelEditor.Instance.selectionOverlayTilemap != null)
            {
                LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();
                foreach (var pos in context.selectedTiles)
                {
                    var tile = LevelEditor.Instance.tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        LevelEditor.Instance.selectionOverlayTilemap.SetTile(pos, tile);
                        LevelEditor.Instance.selectionOverlayTilemap.SetColor(pos, new Color(1f, 1f, 0f, 0.5f));
                    }
                }
            }
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited DraggingSelectedState");
            var keys = new List<LevelObject>(context.originalMaterials.Keys);
            foreach (var obj in keys)
            {
                if (obj != null)
                    context.RestoreHighlight(obj);
            }
            context.isSingleObjectDragging = false;
            context.activeEditingObject = null;
        }

        public void OnUpdate()
        {
            // Re-apply highlight to all selected objects during drag
            foreach (var obj in context.selectedObjects)
            {
                if (obj != null)
                    context.ApplyHighlight(obj, LevelEditor.Instance.highlightMaterial);
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 currentMouseWorldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                Vector3 worldDelta = currentMouseWorldPos - dragStartMouseWorldPos;

                // Hide original tiles during drag
                foreach (var kvp in originalTileData)
                {
                    LevelEditor.Instance.tilemap.SetTile(kvp.Key, null);
                }

                // Move objects
                foreach (var kvp in originalObjectPositions)
                {
                    if (kvp.Key != null)
                        kvp.Key.transform.position = kvp.Value + worldDelta;
                }

                // Move overlay
                if (LevelEditor.Instance.selectionOverlayTilemap != null)
                {
                    LevelEditor.Instance.selectionOverlayTilemap.transform.localPosition = worldDelta;
                }
            }
            else
            {
                // On mouse up, finalize move
                Vector3 finalWorldDelta = LevelEditor.Instance.GetPreciseMouseWorldPosition() - dragStartMouseWorldPos;
                Vector3Int finalCellDelta = LevelEditor.Instance.grid.WorldToCell(finalWorldDelta);
                groupMoveDelta = finalCellDelta;

                // Move tiles
                foreach (var kvp in originalTileData)
                {
                    LevelEditor.Instance.tilemap.SetTile(kvp.Key, null);
                }
                foreach (var kvp in originalTileData)
                {
                    Vector3Int newPos = kvp.Key + groupMoveDelta;
                    LevelEditor.Instance.tilemap.SetTile(newPos, kvp.Value);
                }
                context.selectedTiles.Clear();
                foreach (var kvp in originalTileData)
                {
                    context.selectedTiles.Add(kvp.Key + groupMoveDelta);
                }

                // Reset overlay
                if (LevelEditor.Instance.selectionOverlayTilemap != null)
                {
                    LevelEditor.Instance.selectionOverlayTilemap.transform.localPosition = Vector3.zero;
                    LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();
                }

                // Snap objects to grid
                foreach (var kvp in originalObjectPositions)
                {
                    if (kvp.Key != null)
                    {
                        Vector3 freePos = kvp.Value + (LevelEditor.Instance.GetPreciseMouseWorldPosition() - dragStartMouseWorldPos);
                        Vector3Int snappedCell = LevelEditor.Instance.grid.WorldToCell(freePos);
                        Vector3 snappedPos = LevelEditor.Instance.grid.GetCellCenterWorld(snappedCell);
                        kvp.Key.transform.position = snappedPos;
                    }

                    // Always update startPosition parameter for all selected objects (single or multi)
                    foreach (var obj in context.selectedObjects)
                    {
                        if (obj != null)
                        {
                            var editable = obj as IParameterEditable;
                            if (editable != null)
                            {
                                var parameters = editable.ExportParameters();
                                var startPosParam = parameters.Find(p => p.name == "startPosition" && p.type == "vector2");
                                if (startPosParam != null)
                                {
                                    // Use the object's current (snapped) position
                                    Vector3 pos = obj.transform.position;
                                    string newValue = $"{pos.x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{pos.y.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                                    Debug.Log($"[DraggingSelectedState] Updating startPosition for {obj.name} to {newValue}");
                                    startPosParam.value = newValue;
                                    editable.ImportParameters(parameters);
                                    // Log after import
                                    var afterParams = editable.ExportParameters();
                                    var afterStartPos = afterParams.Find(p => p.name == "startPosition" && p.type == "vector2");
                                    Debug.Log($"[DraggingSelectedState] After ImportParameters, startPosition for {obj.name} is now {afterStartPos?.value}");
                                }
                            }
                        }
                    }
                }

                controller.ChangeToSelectingState();
            }
        }
    }
}