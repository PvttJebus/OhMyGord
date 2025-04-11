using UnityEngine;
using LevelEditorSystem;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: Erasing
    /// </summary>
    public class ErasingState
    {
        private readonly LevelEditorController controller;
        private LevelEditor e;
        private readonly LevelEditorContext context;

        public ErasingState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.e = legacyEditor;
            this.context = context;
        }

        public void OnEnter()
        {
            // TODO: Add enter logic for Erasing state
        }

        public void OnExit()
        {
            // TODO: Add exit logic for Erasing state
        }

        public void OnUpdate()
        {
            if (e == null)
            {
                e = LevelEditor.Instance;
                if (e == null)
                {
                    Debug.LogError("[FSM] ErasingState: LevelEditor.Instance is null!");
                    return;
                }
            }
            if (!(LevelEditor.CurrentMode == LevelEditor.EditorMode.Edit &&
                  (Input.GetMouseButton(1) || Input.GetMouseButton(0))))
                return;

            Vector3Int center = e.GetMouseCellPosition();

            int offset = (Mathf.Max(1, e.brushSize) - 1) / 2;
            for (int x = -offset; x <= offset; x++)
            {
                for (int y = -offset; y <= offset; y++)
                {
                    Vector3Int cell = center + new Vector3Int(x, y, 0);
                    e.GetTilemap().SetTile(cell, null);

                    Vector3 worldPos = e.grid.GetCellCenterWorld(cell);
                    foreach (var col in Physics2D.OverlapPointAll(worldPos))
                    {
                        if (col.TryGetComponent(out LevelObject obj))
                        {
                            e.GetLevelObjects().Remove(obj);
                            UnityEngine.Object.Destroy(obj.gameObject);
                        }
                    }
                }
            }
        }
    }
}