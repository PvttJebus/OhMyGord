using UnityEngine;
using UnityEngine.Tilemaps;

namespace LevelEditorSystem
{
    /// <summary>
    /// Utility struct for drag rectangle selection.
    /// </summary>
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
}