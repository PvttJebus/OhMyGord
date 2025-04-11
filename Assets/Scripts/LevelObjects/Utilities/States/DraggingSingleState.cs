using UnityEngine;
using LevelEditorSystem;
using System.Collections.Generic;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: DraggingSingle
    /// </summary>
    public class DraggingSingleState
    {
        private readonly LevelEditorController controller;
        // Removed: e alias, use LevelEditor.Instance directly
        private readonly LevelEditorContext context;

        public DraggingSingleState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            if (context.activeEditingObject == null)
            {
                Debug.LogWarning("[FSM] DraggingSingleState: activeEditingObject is null on enter, reverting to SelectingState");
                controller.ChangeToSelectingState();
                return;
            }

            Debug.Log("[FSM] Entered DraggingSingleState");

            var renderer = context.activeEditingObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("[FSM] DraggingSingleState: activeEditingObject has no Renderer, reverting to SelectingState");
                controller.ChangeToSelectingState();
                return;
            }

            if (LevelEditor.Instance.highlightMaterial != null)
            {
                context.originalMaterials[context.activeEditingObject] = renderer.material;
                renderer.material = LevelEditor.Instance.highlightMaterial;
            }
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited DraggingSingleState");

            var keys = new List<LevelObject>(context.originalMaterials.Keys);
            foreach (var obj in keys)
            {
                if (obj != null)
                    context.RestoreHighlight(obj);
            }
        }

        public void OnUpdate()
        {
            if (LevelEditor.Instance == null)
            {
                Debug.LogError("[FSM] DraggingSingleState: LevelEditor.Instance is null!");
                return;
            }
            if (context.activeEditingObject == null)
            {
                Debug.LogError("[FSM] DraggingSingleState: activeEditingObject is null!");
                context.isSingleObjectDragging = false;
                controller.ChangeToSelectingState();
                return;
            }
            // Debug.Log("[FSM] DraggingSingleState Update");

            if (context.activeEditingObject == null)
                return;

            if (Input.GetMouseButton(0))
            {
                Vector3 currentMouseWorldPos = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                Vector3 worldDelta = currentMouseWorldPos - LevelEditor.Instance.singleObjectDragStartMouseWorldPos;
                context.activeEditingObject.transform.position = LevelEditor.Instance.singleObjectOriginalPos + worldDelta;
            }
            else
            {
                context.isSingleObjectDragging = false;

                Vector3 finalWorldPos = context.activeEditingObject.transform.position;
                Vector3Int snappedCell = LevelEditor.Instance.grid.WorldToCell(finalWorldPos);
                Vector3 snappedPos = LevelEditor.Instance.grid.GetCellCenterWorld(snappedCell);
                context.activeEditingObject.transform.position = snappedPos;

                context.activeEditingObject = null;
            }
        }
    }
}