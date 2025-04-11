# Improved Plan: Decouple Single Object Editing from Group Move

---

## 1. Single Object Placement & Editing Mode

- **Purpose:**
  - Dragging a new prefab into the scene
  - Editing a single existing object (move, rotate, resize)
- **Controlled by:**
  - `editing` flag on the **single active LevelObject**
  - Optionally, a dedicated `EditorState.EditingSingle`
- **Behavior:**
  - Object follows mouse until finalized
  - Allows rotation, scaling, other edits
- **Transitions:**
  - **Enter:** When spawning a new object or explicitly editing one
  - **Exit:** On mouse release or explicit confirm
- **Finalization:**
  - Sets `editing = false`
  - Snaps to grid
  - Saves to undo history

---

## 2. Group Move Mode

- **Purpose:**
  - Move multiple selected objects and tiles simultaneously
- **Controlled by:**
  - `isGroupMoving` flag
  - Mouse button state
- **Behavior:**
  - Starts on click+drag of selection
  - Moves all selected objects and tiles while mouse held
  - Provides smooth, freeform movement during drag
- **Transitions:**
  - **Enter:** On click+drag over selection
  - **Exit:** On mouse release
- **Finalization:**
  - Snaps all to grid
  - Saves to undo history
- **Does NOT use:**
  - `editing` flag
  - `HandleObjectSpawning()`

---

## 3. Refactor `HandleObjectSpawning()`

- **Limit to:**
  - New prefab placement
  - Single object edit mode
- **Remove:**
  - Any group move logic
  - Any multi-object handling
- **Use `editing` flag only for:**
  - The single active object being placed or edited
- **Transitions:**
  - Entered explicitly
  - Exited explicitly or on finalize

---

## 4. Group Move Logic

- **Separate from `HandleObjectSpawning()`**
- **Initiated by:**
  - Click+drag on selection
- **Moves:**
  - All selected objects (ignoring `editing`)
  - Overlay tilemap for tiles
- **Finalizes:**
  - On mouse release
  - Snaps positions
  - Clears overlay

---

## 5. Undo/Redo

- Both modes should:
  - Save **before** move starts
  - Save **after** move ends
  - Support full undo/redo of placement and group move

---

## 6. Benefits

- Clear, maintainable separation of concerns
- No sticky or conflicting states
- Intuitive UX:
  - **Single object:** click to pick up, click to place
  - **Group move:** click+drag to move, release to drop
- Easier to extend and debug

---

## Summary

This plan ensures a clean, robust separation between single object editing and group move, improving maintainability, user experience, and future extensibility.