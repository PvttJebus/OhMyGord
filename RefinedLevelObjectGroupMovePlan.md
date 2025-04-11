# Refined Plan for Robust LevelObject Group Move

---

## 1. Data to Store on Drag Start

- Initial mouse **world position**
- For each selected object:
  - Original **world position**
  - Original **rotation**
  - Original **scale** (if scalable)
- Optionally, bounding box of selection for visual feedback

---

## 2. During Drag

- Compute **world delta**:
  ```
  delta = currentMouseWorldPos - initialMouseWorldPos
  ```
- For each selected object:
  - New position = original position + delta
  - Keep rotation and scale unchanged (unless user modifies)
- **Snapping**:
  - Optionally snap new position to grid or other constraints
- **Visual feedback**:
  - Optionally draw bounding box or ghost objects

---

## 3. On Mouse Release (Finalize)

- Commit new positions
- Save original and new positions for **undo/redo**
- Exit group move mode

---

## 4. Cancel (Escape key or right-click)

- Revert all objects to original positions
- Exit group move mode

---

## 5. Undo/Redo

- Store:
  - Original positions
  - New positions
- On undo: revert to original
- On redo: apply new

---

## 6. Integration with LevelObject.cs

- Since LevelObject is a MonoBehaviour with transform, moving is straightforward
- If LevelObject has additional logic (e.g., snapping, constraints), call a method like:
  ```csharp
  levelObject.SetPosition(Vector3 newPos)
  ```
  instead of directly setting transform.position
- Optionally, extend LevelObject with hooks for move start, update, end

---

## 7. Advanced Features (Future)

- **Multi-axis constraints** (X-only, Y-only)
- **Rotation during group move** (e.g., with modifier key)
- **Scaling group** (uniform or per-object)
- **Group pivot point** for rotation/scaling
- **Visual handles** for manipulation

---

## Summary

Use **precise world delta** for smooth, accurate group movement of LevelObjects, preserving rotation and scale, with optional snapping and visual feedback, and full undo/redo support.