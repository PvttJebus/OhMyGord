# Optimized FSM Migration Plan for LevelEditor.cs

---

## Overview

This plan details **surgical, chunked, idempotent steps** to migrate `LevelEditor.cs` from a monolithic design to a clean FSM-driven architecture. It is optimized for execution by an LLM with a large context window.

---

## Step 1: Preparation

- Backup `LevelEditor.cs`.
- Create a new branch or snapshot.

---

## Step 2: Remove `EditorState` Enum and References

- Delete the `EditorState` enum.
- Remove the `CurrentState` property.
- Remove methods:
  - `SetSpawningState`
  - `SetDrawingState`
  - `SetEraserState`
  - `OnSelectButtonClicked`
- Remove all calls to these methods.

---

## Step 3: Remove Input Handling from `Update()`

- Delete sticky spawn logic (lines 165-197).
- Delete selection logic (lines 247-250).
- Delete tile editing logic (lines 253-254).
- Keep only:
  - Mode switching (lines 204-215)
  - Undo/redo (lines 230-238)
  - Brush size updates (lines 240-242)
  - Camera controls (lines 243-244)
- Remove any other input handling.

---

## Step 4: Rewire UI Buttons

- In `Awake()`, remove button listeners that call monolithic methods.
- Instead, have UI buttons **trigger FSM transitions** via `LevelEditorController`.

---

## Step 5: Migrate Sticky Spawning

- Fully implement `StickySpawningState`.
- Remove sticky spawn flags:
  - `isStickySpawning`
  - `stickyObject`
  - `waitingForStickyClickRelease`
  - `suppressNextStickyClick`
- Remove sticky spawn logic from monolith.

---

## Step 6: Migrate Tile Editing

- Fully implement `DrawingTilesState` and `ErasingState`.
- Remove `HandleTileEditing()` method.
- Remove related flags:
  - `isDraggingTile`
  - `lastLeftClickTime`
  - `floodFillTargetTile`
  - `currentTile`

---

## Step 7: Migrate Selection

- Fully implement `SelectingState`.
- Remove:
  - `TryStartSpawning()`
  - `HandleAltClick()`
- Remove related flags:
  - `isSelecting`
  - `currentDrag`
  - `selectedObjects`
  - `selectedTiles`
  - `originalMaterials`

---

## Step 8: Migrate Dragging

- Fully implement `DraggingSingleState` and `DraggingGroupState`.
- Remove related flags:
  - `isSingleObjectDragging`
  - `groupMoveInProgress`
  - `isGroupMoving`
  - `activeEditingObject`
  - `groupMoveStartMouseWorldPos`
  - `originalObjectWorldPositions`
  - `originalTileData`
  - `groupMoveDelta`

---

## Step 9: Clean Up Fields

- Remove all state flags now owned by FSM states.
- Keep data/model fields:
  - `tilemap`
  - `levelObjects`
  - `tilesetManager`
  - `objectPalette`
  - `grid`
  - `editorCamera`
  - `previewCamera`
  - `history`
  - `saveLoadUI`
  - Utility methods (`ApplyBrush`, `GetMouseCellPosition`, etc.)

---

## Step 10: Document and Test

- Add comments indicating FSM ownership of logic.
- Document utility methods.
- Test FSM transitions after each chunk.
- Ensure undo/redo and camera controls remain functional.

---

## Additional Notes

- **Chunk Edits:** Group removals and rewires logically.
- **Idempotent:** Each step can be safely re-run or skipped if already done.
- **Explicit FSM Integration:** Clearly mark FSM transition points.
- **Minimal Coupling:** FSM states should manipulate data/model, not own it.
- **Progressive Testing:** Validate after each step.

---

## End of Plan

This plan will transform `LevelEditor.cs` into a **clean data/model class** with all interaction logic handled by FSM states, improving maintainability, readability, and extensibility.