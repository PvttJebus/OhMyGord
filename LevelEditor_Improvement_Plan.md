# LevelEditor Improvement Plan

---

## 1. Refactor `Update()` into Smaller Methods

- **Current:** Monolithic `Update()` handles erasing, group move, selection, spawning, camera, mode switching.
- **Goal:** Improve readability, maintainability, and testability.
- **Actions:**
  - Extract:
    - `HandleErasing()`
    - `HandleGroupMove()`
    - `HandleSelection()`
    - `HandleSpawning()`
    - `HandleCameraControls()`
    - `HandleModeSwitching()`
  - Call these from `Update()` in order.

---

## 2. Explicit Single Object Edit Mode

- **Current:** No clear mode for editing existing objects.
- **Goal:** Support intuitive click+drag to move, rotate, resize single objects.
- **Actions:**
  - Add `EditorState.EditingSingle`
  - Enter on explicit selection or edit command
  - Exit on mouse release or confirm
  - Disable automatic movement unless dragging
  - Support rotation, scaling, property editing

---

## 3. Clarify State Management

- **Current:** `editing` flag overloaded, `isGroupMoving` separate, implicit transitions.
- **Goal:** Clear, explicit states and transitions.
- **Actions:**
  - Use `EditorState` for high-level mode:
    - `Selecting`
    - `Drawing`
    - `Erasing`
    - `EditingSingle`
    - `Spawning`
  - Use flags (`isGroupMoving`, `isSelecting`) for transient actions
  - Document transitions clearly

---

## 4. Drag Thresholds for Selection and Move

- **Current:** Immediate start of selection or move on click.
- **Goal:** Prevent accidental drags, improve UX.
- **Actions:**
  - On mouse down, record position
  - On drag, start action only if distance > threshold (e.g., 5 pixels)
  - Otherwise treat as click/tap

---

## 5. Improve Undo/Redo System

- **Current:** Saves before/after moves, but coarse-grained.
- **Goal:** More granular, flexible undo/redo.
- **Actions:**
  - Use command pattern for actions
  - Support undoing:
    - Single object moves
    - Group moves
    - Tile edits
    - Property changes
  - Batch related actions as needed

---

## 6. Dependency Injection and UI Management

- **Current:** Many serialized fields, manual setup.
- **Goal:** Easier setup, testing, and maintenance.
- **Actions:**
  - Group UI references into a struct or class
  - Use dependency injection or service locator
  - Create overlay tilemap programmatically (already done)

---

## 7. Optional: Visual Feedback and Polish

- **Add:**
  - Drag thresholds
  - Animated transitions
  - Ghost previews
  - Snap indicators
  - Sound effects or haptics

---

## Summary

This plan will significantly improve the LevelEditor by clarifying responsibilities, improving UX, and making the codebase more maintainable and extensible.