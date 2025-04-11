# Robust Plan for Moving Selected Tiles with Overlay Preview

---

## Overview
Enable intuitive, non-destructive group movement of selected tiles using a **temporary overlay tilemap** for visual feedback during drag.

---

## 1. Initial Drag Start
- On mouse down over a selected tile:
  - Enter **tile move mode**
  - Store:
    - Initial mouse position
    - Original positions of all selected tiles (`HashSet<Vector3Int>`)
    - Original tile data (`Dictionary<Vector3Int, TileBase>`)
  - Optionally, highlight or ghost original tiles

---

## 2. During Drag
- Calculate **cell delta** based on mouse movement and grid snapping
- Update **overlay tilemap**:
  - Clear previous overlay tiles
  - For each selected tile:
    - Place the tile at **original position + delta** on the overlay tilemap
    - Use a semi-transparent or tinted version for feedback
- Do **not** modify the main tilemap yet

---

## 3. On Mouse Up (Finalize Move)
- Remove tiles from **original positions** in main tilemap
- Place tiles at **new positions** (original + delta) in main tilemap
- Clear overlay tilemap
- Update `selectedTiles` to new positions
- Save operation to **history** for undo/redo

---

## 4. On Cancel (Escape key or right-click)
- Clear overlay tilemap
- Do **not** modify main tilemap
- Exit move mode

---

## 5. Undo/Redo Support
- Save:
  - Original positions and tiles
  - New positions and tiles
- On undo:
  - Restore tiles to original positions
- On redo:
  - Move tiles to new positions

---

## 6. Additional Considerations
- **Snapping:** Apply grid snapping to delta
- **Overlap:** Handle overlapping with existing tiles (overwrite, merge, or block)
- **Performance:** Optimize overlay updates
- **Multi-layer:** Extend to multiple tilemaps if needed

---

## Summary
Use a **temporary overlay tilemap** to preview tile movement during drag, only committing changes on finalize, ensuring a robust, user-friendly multi-tile move experience.