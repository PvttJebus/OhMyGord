// GroupEditingState.cs
using UnityEngine;
using System.Collections.Generic;
using LevelEditorSystem;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: GroupEditing
    /// Handles creation, selection, and assignment of InteractableGroups.
    /// </summary>
    public class GroupEditingState
    {
        private readonly LevelEditorController controller;
        private readonly LevelEditorContext context;

        private InteractableGroup activeGroup;
        private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

        public void SetActiveGroup(InteractableGroup group)
        {
            activeGroup = group;
        }

        public GroupEditingState(LevelEditorController controller, LevelEditorContext context)
        {
            this.controller = controller;
            this.context = context;
        }

        public void OnEnter()
        {
            Debug.Log("[FSM] Entered GroupEditingState");
            // TODO: Show Groups panel in LevelEditor UI

            // Highlight all members of the active group
            originalMaterials.Clear();
            if (activeGroup != null)
            {
                var highlightMat = LevelEditor.Instance.groupHighlightMaterial;
                foreach (var interactable in activeGroup.GetComponentsInChildren<Interactable>())
                {
                    var renderer = interactable.GetComponent<Renderer>();
                    if (renderer != null && highlightMat != null)
                    {
                        originalMaterials[renderer] = renderer.material;
                        renderer.material = highlightMat;
                        context.selectedObjects.Add(interactable); // Add to selection context
                    }
                }
            }
        }

        public void OnExit()
        {
            Debug.Log("[FSM] Exited GroupEditingState");
            // Restore original materials
            foreach (var kvp in originalMaterials)
            {
                if (kvp.Key != null)
                    kvp.Key.material = kvp.Value;
            }
            originalMaterials.Clear();
        }

        public void OnUpdate()
        {
            // Handle assignment of interactables to groups (Ctrl+Click to add/remove)
            if (activeGroup != null && LevelEditor.CurrentMode == LevelEditor.EditorMode.Edit)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mouseWorld = LevelEditor.Instance.GetPreciseMouseWorldPosition();
                    // Check if clicked on an Interactable
                    Interactable clicked = null;
                    foreach (var obj in LevelEditor.Instance.GetLevelObjects())
                    {
                        if (obj == null) continue;
                        Collider2D col = obj.GetComponent<Collider2D>();
                        if (col != null && col.OverlapPoint(mouseWorld))
                        {
                            clicked = obj as Interactable;
                            break;
                        }
                    }

                    if (clicked != null)
                    {
                        // Toggle membership in group
                        bool inGroup = clicked.transform.parent == activeGroup.transform;
                        if (inGroup)
                        {
                            // Remove from group
                            activeGroup.RemoveMember(clicked);
                            context.selectedObjects.Remove(clicked);
                            // Restore material
                            var renderer = clicked.GetComponent<Renderer>();
                            if (renderer != null && originalMaterials.ContainsKey(renderer))
                                renderer.material = originalMaterials[renderer];
                        }
                        else
                        {
                            // Add to group
                            activeGroup.AddMember(clicked);
                            context.selectedObjects.Add(clicked);
                            // Highlight
                            var renderer = clicked.GetComponent<Renderer>();
                            var highlightMat = LevelEditor.Instance.groupHighlightMaterial;
                            if (renderer != null && highlightMat != null)
                            {
                                originalMaterials[renderer] = renderer.material;
                                renderer.material = highlightMat;
                            }
                        }
                    }
                }
            }

            // Escape key exits group editing mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                controller.ChangeToSelectingState();
            }
        }
    }
}