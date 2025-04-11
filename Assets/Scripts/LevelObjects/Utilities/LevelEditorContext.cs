using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelEditorSystem
{
    /// <summary>
    /// Holds FSM-owned interaction state data for the Level Editor.
    /// </summary>
    public class LevelEditorContext
    {
        // Selection state
        public bool isSelecting = false;
        public LevelEditorSystem.DragArea? currentDrag = null;
        public List<LevelObject> selectedObjects = new();
        public HashSet<Vector3Int> selectedTiles = new();
        public Dictionary<LevelObject, Material> originalMaterials = new();

        // Dragging single object
        public bool isSingleObjectDragging = false;
        public LevelObject activeEditingObject = null;

        // Dragging group
        public bool isGroupMoving = false;
        public bool groupMoveInProgress = false;
        public Vector3 groupMoveStartMouseWorldPos;
        public Dictionary<LevelObject, Vector3> originalObjectWorldPositions = new();
        public Dictionary<Vector3Int, TileBase> originalTileData = new();
        public Vector3Int groupMoveDelta = Vector3Int.zero;

        // Tile drawing
        public bool isDraggingTile = false;
        public float lastLeftClickTime = -1f;
        public TileBase floodFillTargetTile;
        public TileBase currentTile;

        // Sticky spawning
        public bool isStickySpawning = false;
        public LevelObject stickyObject = null;
        public bool waitingForStickyClickRelease = false;
        public bool suppressNextStickyClick = false;
        public void ApplyHighlight(LevelObject obj, Material highlightMaterial)
        {
            if (obj == null) return;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null || highlightMaterial == null) return;

            if (!originalMaterials.ContainsKey(obj))
                originalMaterials[obj] = renderer.material;

            renderer.material = highlightMaterial;
        }

        public void RestoreHighlight(LevelObject obj)
        {
            if (obj == null) return;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null) return;

            if (originalMaterials.TryGetValue(obj, out var originalMat))
            {
                if (originalMat != null)
                    renderer.material = originalMat;
                else
                    renderer.material = null;

                originalMaterials.Remove(obj);
            }
        }
    }
}