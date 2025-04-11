using UnityEngine;
using LevelEditorSystem;
using LevelEditorSystem.States;
using Utilities;
using LevelEditorSystem;
using LevelEditorSystem.States;

namespace LevelEditorSystem
{
    /// <summary>
    /// New controller to gradually migrate LevelEditor logic into FSM-based architecture.
    /// </summary>
    public class LevelEditorController : MonoBehaviour
    {
        private LevelEditor legacyEditor;

        public LevelEditorContext context;

        private FiniteStateMachine<EditorFSMState> fsm;

        // State instances
        private IdleState idleState;
        private SelectingState selectingState;
        private DraggingSelectedState draggingSelectedState;
        private DrawingTilesState drawingTilesState;
        private ErasingState erasingState;
        private StickySpawningState stickySpawningState;
        private GroupEditingState groupEditingState;

        private void Awake()
        {
            fsm = new FiniteStateMachine<EditorFSMState>();

            if (legacyEditor == null)
            {
                legacyEditor = GetComponent<LevelEditor>();
                if (legacyEditor == null)
                {
                    legacyEditor = gameObject.AddComponent<LevelEditor>();
                }
            }

            context = new LevelEditorContext();

            // Instantiate states with references and shared context
            idleState = new IdleState(this, LevelEditor.Instance, context);
            selectingState = new SelectingState(this, LevelEditor.Instance, context);
            draggingSelectedState = new DraggingSelectedState(this, LevelEditor.Instance, context);
            drawingTilesState = new DrawingTilesState(this, LevelEditor.Instance, context);
            erasingState = new ErasingState(this, LevelEditor.Instance, context);
            stickySpawningState = new StickySpawningState(this, LevelEditor.Instance, context);
            groupEditingState = new GroupEditingState(this, context);

            // Register states with FSM
            fsm.AddState(EditorFSMState.Idle, idleState.OnEnter, idleState.OnExit, idleState.OnUpdate);
            fsm.AddState(EditorFSMState.Selecting, selectingState.OnEnter, selectingState.OnExit, selectingState.OnUpdate);
            fsm.AddState(EditorFSMState.DraggingSelected, draggingSelectedState.OnEnter, draggingSelectedState.OnExit, draggingSelectedState.OnUpdate);
            fsm.AddState(EditorFSMState.DrawingTiles, drawingTilesState.OnEnter, drawingTilesState.OnExit, drawingTilesState.OnUpdate);
            fsm.AddState(EditorFSMState.ErasingTiles, erasingState.OnEnter, erasingState.OnExit, erasingState.OnUpdate);
            fsm.AddState(EditorFSMState.StickySpawning, stickySpawningState.OnEnter, stickySpawningState.OnExit, stickySpawningState.OnUpdate);
            fsm.AddState(EditorFSMState.GroupEditing, groupEditingState.OnEnter, groupEditingState.OnExit, groupEditingState.OnUpdate);

            fsm.ChangeState(EditorFSMState.Selecting);
        }

        private void Update()
        {
            fsm.Update();
        }

        public void ChangeToSelectingState()
        {
            fsm.ChangeState(EditorFSMState.Selecting);
        }

        // Removed: ChangeToDraggingGroupState and ChangeToDraggingSingleState

        public void ChangeToDrawingState()
        {
            fsm.ChangeState(EditorFSMState.DrawingTiles);
        }

        public void ChangeToErasingState()
        {
            fsm.ChangeState(EditorFSMState.ErasingTiles);
        }

        public void ChangeToDraggingSelectedState()
        {
            fsm.ChangeState(EditorFSMState.DraggingSelected);
        }

        public void ChangeToStickySpawningState()
        {
            fsm.ChangeState(EditorFSMState.StickySpawning);
        }
        public void ChangeToGroupEditingState()
        {
            fsm.ChangeState(EditorFSMState.GroupEditing);
        }
        public GroupEditingState GetGroupEditingState()
        {
            return groupEditingState;
        }
    }
}