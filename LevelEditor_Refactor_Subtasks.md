# LevelEditor Refactor Subtasks

---

## Subtask 1: Remove Sticky "Editing" Behavior for New Objects

- **Goal:** Newly spawned objects do not stick to the mouse automatically.
- **Actions:**
  - When spawning a new prefab, place it immediately at the snapped position.
  - Do **not** set `editing = true` on the new object.
  - Require the user to **click and drag** to move the new object after placement.
  - Remove any code that moves new objects automatically after spawn.

---

## Subtask 2: Fully Separate `editing` Flag from Move Logic

- **Goal:** The `editing` flag is only used for property editing (rotation, scaling, etc.), not for movement.
- **Actions:**
  - Remove all movement logic conditioned on `editing`.
  - Use explicit drag detection (mouse down + drag) to move objects.
  - Keep `editing` for UI property panels, rotation, scaling, etc.

---

## Subtask 3: Implement Click-and-Drag to Move Any Object

- **Goal:** Moving any object (new or existing) requires click-and-drag.
- **Actions:**
  - On mouse down over an object, start drag mode.
  - While dragging, update position relative to initial offset.
  - On mouse up, snap to grid and finalize.
  - Works for both new and existing objects.

---

## Subtask 4: Prevent Rectangle Selection During Object Drag

- **Goal:** Dragging an object does not start or update a selection rectangle.
- **Actions:**
  - When dragging an object (single or group), set `isSelecting = false`.
  - Skip rectangle selection logic during drag.
  - Only allow rectangle selection when **not** dragging objects.

---

## Subtask 5: Test and Polish

- **Test:**
  - New objects spawn at snapped position, do not stick to mouse.
  - Moving any object requires click-and-drag.
  - Group move works as before.
  - Rectangle selection does not interfere with dragging.
- **Polish:**
  - Add drag threshold if needed.
  - Add visual feedback during drag.
  - Update undo/redo as needed.

---

## Summary

This sequence will eliminate sticky object behavior, clarify editing vs. moving, and improve the overall editing experience.