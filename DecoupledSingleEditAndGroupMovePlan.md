# Refined Plan: Decouple Single Object Editing from Group Move

---

## 1. Single Object Placement/Editing

- **Purpose:**
  - Dragging a new prefab into the scene
  - Editing a single existing object (move, rotate, resize)
- **Controlled by:**
  - `editing` flag on the **single active LevelObject**
  - Optionally, a dedicated `EditingSingle` state
- **Behavior:**
  - Object follows mouse until finalized (click or key)
  - Allows rotation, scaling, other edits
- **Finalized by:**
  - Mouse release or explicit confirm
  - Sets `editing = false`

---

## 2. Group Move

- **Purpose:**
  - Move multiple selected objects and tiles simultaneously
- **Controlled by:**
  - `isGroupMoving` flag
  - Mouse button state
- **Behavior:**
  - Starts on click+drag of selection
  - Moves all selected objects and tiles while mouse held
  - Finalizes on mouse release
- **Does NOT use:**
  - `editing` flag on any object
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

## 5. Benefits

- Clear, maintainable separation
- No sticky or conflicting states
- Intuitive UX:
  - **Single object edit**: click to pick up, click to place
  - **Group move**: click+drag to move, release to drop

---

## Summary

This plan ensures clean separation of single object editing and group move, improving maintainability and user experience.