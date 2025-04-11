using UnityEngine;
using LevelEditorSystem;
using System.Collections.Generic;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: DraggingGroup
    /// </summary>
    public class DraggingGroupState
    {
        private readonly LevelEditorController controller;
        // Removed: e alias, use LevelEditor.Instance directly
        private readonly LevelEditorContext context;

        public DraggingGroupState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered DraggingGroupState");

            if (LevelEditor.Instance == null)
            {
                Debug.LogError("[FSM] DraggingGroupState: LevelEditor.Instance is null!");
                return;
            }
            if (context == null)
            {
                Debug.LogError("[FSM] DraggingGroupState: context is null!");
                return;
            }

            context.isGroupMoving = true;
            context.groupMoveInProgress = true; // Start dragging immediately

            context.originalTileData.Clear();
            context.originalObjectWorldPositions.Clear();

            context.groupMoveStartMouseWorldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();

            // Save original tile data
            foreach (var pos in context.selectedTiles)
            {
                var tile = LevelEditor.Instance.tilemap.GetTile(pos);
                if (tile != null)
                {
                    context.originalTileData[pos] = tile;
                }
            }

            // Save original object positions
            foreach (var obj in context.selectedObjects)
            {
                if (obj != null)
                {
                    context.originalObjectWorldPositions[obj] = obj.transform.position;
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
                            }
                        }
                    }
                }
            }
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited DraggingGroupState");
            // Do not restore highlights here; SelectingState manages selection highlights
        }

        public void OnUpdate()
        {
            Debug.Log($"[FSM] DraggingGroupState Update - isGroupMoving: {context.isGroupMoving}, groupMoveInProgress: {context.groupMoveInProgress}");
            // Debug.Log("[FSM] DraggingGroupState Update");

            if (!context.isGroupMoving)
                return;

            // Re-apply highlight to all selected objects during drag
            foreach (var obj in context.selectedObjects)
            {
                if (obj != null)
                    context.ApplyHighlight(obj, LevelEditor.Instance.highlightMaterial);
            }

            if (!context.groupMoveInProgress && Input.GetMouseButtonDown(0))
            {
                context.groupMoveInProgress = true;
                Debug.Log("[FSM] DraggingGroupState - Drag started");
            }

            if (context.groupMoveInProgress && Input.GetMouseButton(0))
            {
                Vector3 currentMouseWorldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                Vector3 worldDelta = currentMouseWorldPos - context.groupMoveStartMouseWorldPos;

                if (LevelEditor.Instance.selectionOverlayTilemap != null)
                {
                    LevelEditor.Instance.selectionOverlayTilemap.transform.localPosition = worldDelta;
                }

                // Hide original tiles during drag
                foreach (var kvp in context.originalTileData)
                {
                    LevelEditor.Instance.tilemap.SetTile(kvp.Key, null);
                }

                foreach (var kvp in context.originalObjectWorldPositions)
                {
                    if (kvp.Key != null)
                        kvp.Key.transform.position = kvp.Value + worldDelta;
                }
            }
            else if (context.groupMoveInProgress && !Input.GetMouseButton(0))
            {
                context.groupMoveInProgress = false;
                context.isGroupMoving = false;

                controller.ChangeToSelectingState();

                Vector3 finalWorldDelta = LevelEditor.Instance.GetPreciseMouseWorldPosition() - context.groupMoveStartMouseWorldPos;
                Vector3Int finalCellDelta = LevelEditor.Instance.grid.WorldToCell(finalWorldDelta);
                context.groupMoveDelta = finalCellDelta;

                foreach (var kvp in context.originalTileData)
                {
                    LevelEditor.Instance.tilemap.SetTile(kvp.Key, null);
                }
                foreach (var kvp in context.originalTileData)
                {
                    Vector3Int newPos = kvp.Key + context.groupMoveDelta;
                    LevelEditor.Instance.tilemap.SetTile(newPos, kvp.Value);
                }
                context.selectedTiles.Clear();
                foreach (var kvp in context.originalTileData)
                {
                    context.selectedTiles.Add(kvp.Key + context.groupMoveDelta);
                }

                if (LevelEditor.Instance.selectionOverlayTilemap != null)
                {
                    LevelEditor.Instance.selectionOverlayTilemap.transform.localPosition = Vector3.zero;
                    LevelEditor.Instance.selectionOverlayTilemap.ClearAllTiles();
                }

                foreach (var kvp in context.originalObjectWorldPositions)
                {
                    if (kvp.Key != null)
                    {
                        Vector3 freePos = kvp.Value + (LevelEditor.Instance.GetPreciseMouseWorldPosition() - context.groupMoveStartMouseWorldPos);
                        Vector3Int snappedCell = LevelEditor.Instance.grid.WorldToCell(freePos);
                        Vector3 snappedPos = LevelEditor.Instance.grid.GetCellCenterWorld(snappedCell);
                        kvp.Key.transform.position = snappedPos;
                    }
                }
            }
        }
    }
}