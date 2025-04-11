using UnityEngine;
using LevelEditorSystem;

namespace LevelEditorSystem.States
{
    /// <summary>
    /// FSM State: ReadyToGroupMove
    /// </summary>
    public class ReadyToGroupMoveState
    {
        private readonly LevelEditorController controller;
        private readonly LevelEditor legacyEditor;
        private readonly LevelEditorContext context;

        public ReadyToGroupMoveState(LevelEditorController controller, LevelEditor legacyEditor, LevelEditorContext context)
        {
            this.controller = controller;
            this.legacyEditor = legacyEditor;
            this.context = context;
        }

        public void OnEnter()
        {
            // TODO: Add enter logic for ReadyToGroupMove state
        }

        public void OnExit()
        {
            // TODO: Add exit logic for ReadyToGroupMove state
        }

        public void OnUpdate()
        {
            // TODO: Add update logic for ReadyToGroupMove state
        }
    }
}