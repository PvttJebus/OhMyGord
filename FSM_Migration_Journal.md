# FSM Migration Journal

---

## Day 1: Initial Overview

- The project is mid-migration from a monolithic `LevelEditor` to an FSM architecture.
- The FSM core is clean and generic.
- FSM states cover key editor modes.
- The monolithic `LevelEditor` still contains most input handling and logic.
- FSM states duplicate some of this logic, leading to redundancy.
- Key issues:
  - Dual state management (`EditorState` enum + FSM)
  - Input handling in both places
  - FSM not fully integrated
  - Some FSM states are stubs
  - `LevelEditor` is still a God Object
- Recommendations:
  - Remove `EditorState` enum once FSM is fully integrated
  - Centralize input handling in FSM states
  - Complete FSM state implementations
  - Refactor `LevelEditor` to delegate responsibilities
  - Clarify FSM transitions
  - Add logs to FSM transitions

---

## Day 2: Updated Analysis with Controller Context

- The `LevelEditorController` instantiates and runs the FSM.
- FSM states wrap the old `LevelEditor` instance.
- FSM is operational but the monolithic `LevelEditor.Update()` still runs.
- Migration is gradual; FSM and monolith coexist.
- Redundancies remain in input handling and state management.
- The plan is to fully migrate logic into FSM states and remove monolithic handling.

---

## 10-Day Migration & Refactoring Plan

### Day 2 (Today)
- Review architecture, identify redundancies, and plan migration steps.
- Write this plan into this journal file.

### Day 3
- Deep dive into `DraggingGroupState` and `DraggingSingleState`.
- Identify overlaps with monolithic code.
- Plan to remove redundant drag logic from `LevelEditor`.

### Day 4
- Review `DrawingTilesState` and `ErasingState`.
- Identify overlaps with `LevelEditor.HandleTileEditing()` and erasing logic.
- Plan to remove redundant tile editing code.

### Day 5
- Review `SelectingState`.
- Identify overlaps with selection logic in `LevelEditor`.
- Plan to remove redundant selection code.

### Day 6
- Review `StickySpawningState` and sticky spawn logic in `LevelEditor`.
- Plan to migrate sticky spawning fully into FSM.

### Day 7
- Review `IdleState` and `ReadyToGroupMoveState`.
- Implement missing TODO logic.
- Define clear transitions between states.

### Day 8
- Refactor `LevelEditor`:
  - Remove `EditorState` enum.
  - Remove redundant input handling.
  - Delegate all state logic to FSM.
  - Improve separation of concerns.

### Day 9
- Improve FSM transitions:
  - Explicitly handle all user actions.
  - Add transition logs.
  - Test all editor modes.

### Day 10
- Final cleanup:
  - Improve code readability.
  - Add comments and documentation.
  - Reflect on user experience flow.
  - Prepare final journal entry.

---

## Day 3: Dragging States Review

- FSM states `DraggingGroupState` and `DraggingSingleState` duplicate drag logic in `LevelEditor`.
- Plan to centralize drag logic in FSM and remove from monolith.

---

## Day 4: Drawing & Erasing States Review

- FSM states cover all necessary logic for drawing and erasing.
- Plan to remove redundant monolithic code and rely on FSM.

---

## Day 5: Selecting State Review

- FSM state covers selection logic.
- Plan to remove redundant monolithic code and rely on FSM.

---

## Day 6: Sticky Spawning State Review

- `StickySpawningState` is currently empty.
- Sticky spawning logic remains in `LevelEditor.Update()`.
- Plan:
  - Fully implement sticky spawning in FSM state.
  - Remove sticky spawn logic from monolith.
  - FSM should handle:
    - Initial spawn setup.
    - Object following mouse.
    - Click to finalize placement.
    - Transition back to selecting or idle.

---

## Day 7: Idle & ReadyToGroupMove States Review

- Both states are currently empty.
- Plan:
  - Implement idle as neutral/no interaction state.
  - Implement ready-to-group-move as pre-drag state.
  - Improve FSM flow and clarity.

---

## Day 8: Refactor `LevelEditor` Monolith

- Remove `EditorState` enum and `CurrentState`.
- Remove all input handling from `LevelEditor.Update()`.
- Delegate all input/state logic to FSM states.
- Keep only utility methods and data storage.
- Result: `LevelEditor` becomes a data/model class.

---

## Day 9: Improve FSM Transitions & Testing

- Explicitly define all FSM transitions.
- Add logs for all transitions.
- Test all editor modes:
  - Object selection and dragging
  - Group selection and dragging
  - Tile drawing and erasing
  - Sticky spawning
  - Camera controls
- Adjust transitions based on user input and edge cases.

---

## Day 10: Final Cleanup & Documentation

- Remove any remaining redundant or dead code.
- Document:
  - FSM architecture
  - State responsibilities
  - Transition logic
  - How to add new states
- Reflect on user experience:
  - Clear, predictable mode switching
  - Visual feedback for current mode
  - Smooth transitions
- Prepare final journal entry summarizing the migration.

---

## Final Reflection

This migration will:

- Greatly improve **separation of concerns**.
- Make the codebase **more maintainable**.
- Allow **easier extension** of editor features.
- Improve **readability** and **debuggability**.
- Provide a **clearer user experience** with explicit modes.

---

## End of Migration Plan

Ready to proceed with implementation based on this plan.