using UnityEngine;
using LevelEditorSystem;
using System.Collections.Generic;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: StickySpawning
    /// </summary>
    public class StickySpawningState
    {
        private readonly LevelEditorController controller;
        private LevelEditor legacyEditor;
        private readonly LevelEditorContext context;

        public StickySpawningState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.legacyEditor = legacyEditor;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered StickySpawningState");

            if (context.stickyObject == null)
            {
                Debug.LogWarning("[FSM] StickySpawningState.OnEnter: stickyObject is null, skipping highlight");
                return;
            }

            context.ApplyHighlight(context.stickyObject, LevelEditor.Instance.highlightMaterial);

            // If mouse is already pressed, wait for release to finalize placement
            context.waitingForStickyClickRelease = Input.GetMouseButton(0);
            context.suppressNextStickyClick = true;
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited StickySpawningState");

            var keys = new List<LevelObject>(context.originalMaterials.Keys);
            foreach (var obj in keys)
            {
                if (obj != null)
                    context.RestoreHighlight(obj);
            }
        }

        public void OnUpdate()
        {
            if (legacyEditor == null)
            {
                legacyEditor = LevelEditor.Instance;
                if (legacyEditor == null)
                {
                    Debug.LogError("[FSM] StickySpawningState: LevelEditor.Instance is null!");
                    return;
                }
            }
            if (context.stickyObject == null)
            {
                Debug.LogError("[FSM] StickySpawningState: stickyObject is null!");
                context.isStickySpawning = false;
                controller.ChangeToSelectingState();
                return;
            }
            if (!context.isStickySpawning || context.stickyObject == null)
            {
                controller.ChangeToSelectingState();
                return;
            }

            // Follow mouse
            Vector3 snappedPos = legacyEditor.GetSnappedMousePosition();
            context.stickyObject.transform.position = snappedPos;

            if (context.suppressNextStickyClick)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    context.suppressNextStickyClick = false;
                }
            }
            else
            {
                if (!context.waitingForStickyClickRelease && Input.GetMouseButtonDown(0))
                {
                    context.waitingForStickyClickRelease = true;
                }

                if (context.stickyObject != null && context.waitingForStickyClickRelease && Input.GetMouseButtonUp(0))
                {
                    // Finalize placement
                    context.stickyObject.transform.position = snappedPos;

                    // Update startPosition parameter if supported
                    var editable = context.stickyObject as IParameterEditable;
                    if (editable != null)
                    {
                        var parameters = editable.ExportParameters();
                        var startPosParam = parameters.Find(p => p.name == "startPosition" && p.type == "vector2");
                        if (startPosParam != null)
                        {
                            string newValue = $"{snappedPos.x.ToString(System.Globalization.CultureInfo.InvariantCulture)},{snappedPos.y.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
                            Debug.Log($"[StickySpawningState] Updating startPosition for {context.stickyObject.name} to {newValue}");
                            startPosParam.value = newValue;
                            editable.ImportParameters(parameters);
                            // Log after import
                            var afterParams = editable.ExportParameters();
                            var afterStartPos = afterParams.Find(p => p.name == "startPosition" && p.type == "vector2");
                            Debug.Log($"[StickySpawningState] After ImportParameters, startPosition for {context.stickyObject.name} is now {afterStartPos?.value}");
                        }
                    }

                    bool altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

                    if (altHeld && context.stickyObject != null)
                    {
                        // Restore previous sticky object's material
                        if (context.originalMaterials.TryGetValue(context.stickyObject, out var originalMat))
                        {
                            var renderer = context.stickyObject.GetComponent<Renderer>();
                            if (renderer != null)
                                renderer.material = originalMat;

                            context.originalMaterials.Remove(context.stickyObject);
                        }

                        // Clone the last placed object
                        GameObject cloneObj = Object.Instantiate(context.stickyObject.gameObject, snappedPos, Quaternion.identity);
                        if (cloneObj.TryGetComponent(out LevelObject cloneLevelObj))
                        {
                            cloneLevelObj.originalPrefabInstanceID = context.stickyObject.originalPrefabInstanceID;
                            legacyEditor.RegisterLevelObject(cloneLevelObj);

                            context.stickyObject = cloneLevelObj;
                            context.isStickySpawning = true;
                            context.waitingForStickyClickRelease = false;
                            context.suppressNextStickyClick = true;

                            Debug.Log("[FSM] StickySpawningState: Cloned sticky object");

                            var cloneRenderer = cloneLevelObj.GetComponent<Renderer>();
                            Debug.Log($"[FSM] StickySpawningState: cloneLevelObj={cloneLevelObj}, renderer={cloneRenderer}, highlightMaterial={legacyEditor.highlightMaterial}");

                            if (cloneRenderer != null && legacyEditor.highlightMaterial != null)
                            {
                                context.originalMaterials[cloneLevelObj] = cloneRenderer.material;
                                cloneRenderer.material = legacyEditor.highlightMaterial;
                            }
                        }
                    }
                    else
                    {
                        context.isStickySpawning = false;
                        context.stickyObject = null;
                        context.waitingForStickyClickRelease = false;

                        controller.ChangeToSelectingState();
                    }
                }
            }
        }
    }
}