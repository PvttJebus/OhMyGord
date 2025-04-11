# Multi-Selection Move and Shader Visualization Plan

---

## Overview
Enable moving multiple selected tiles and objects simultaneously by dragging any selected item, with shader-based visual feedback for selection.

---

## 1. Shader-Based Selection Visualization

### Objects
- Apply an outline shader or highlight material to selected objects
- Swap material on selection, revert on deselection
- Alternatively, use a shader with a highlight toggle property

### Tiles
- Overlay a semi-transparent highlight tile on selected cells
- Or, use a shader/material on the tilemap renderer to highlight selected tiles
- Consider using a secondary tilemap layer for highlights

---

## 2. Detect Drag Start on Selection

- On mouse down:
  - Check if click is on a **selected object** or **selected tile**
  - If yes, enter **group move mode**
  - Store:
    - Initial mouse position
    - Initial positions of all selected objects
    - Initial positions of all selected tiles

---

## 3. Move Selection During Drag

- On mouse drag:
  - Calculate mouse delta from initial position
  - For each selected object:
    - Update position by delta
  - For tiles:
    - Option 1: Show ghost tiles at offset positions
    - Option 2: Temporarily clear and redraw tiles at offset positions (more complex)
- Provide visual feedback during drag

---

## 4. Finalize Move on Mouse Up

- For objects:
  - Commit new positions
- For tiles:
  - Remove original tiles
  - Place tiles at new positions
- Save operation to history for undo/redo

---

## 5. Additional Considerations

- **Snapping:** Apply grid snapping during move
- **Cancel:** Support canceling move with escape key
- **Undo:** Integrate with existing undo system
- **Performance:** Optimize tile redraws to avoid flicker

---

## 6. Implementation Roadmap

1. Add shader/material highlight for selection
2. Detect drag start on selected item
3. Implement group move logic for objects
4. Implement group move logic for tiles (ghost or redraw)
5. Finalize move and update data
6. Integrate undo/redo
7. Polish and optimize

---

## Summary
This plan enables intuitive multi-selection movement with clear visual feedback, improving the level editor's usability and power.