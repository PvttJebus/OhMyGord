using UnityEngine;
using LevelEditorSystem;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: Idle
    /// </summary>
    public class IdleState
    {
        private readonly LevelEditorController controller;
        private readonly LevelEditor legacyEditor;
        private readonly LevelEditorContext context;

        public IdleState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.legacyEditor = legacyEditor;
            this.context = context;
        }

        public void OnEnter()
        {
            // TODO: Add enter logic for Idle state
        }

        public void OnExit()
        {
            // TODO: Add exit logic for Idle state
        }

        public void OnUpdate()
        {
            // TODO: Add update logic for Idle state
        }
    }
}