# Rectangle Selection and Drawing Enhancement Plan

## Overview
Enhance the LevelEditor with advanced rectangle selection and rectangle drawing capabilities using a unified, extensible architecture.

---

## 1. Core Abstraction: DragArea

Create a **DragArea** struct/class that encapsulates drag rectangle logic:

- **Fields:**
  - `Vector2 startScreenPos`
  - `Vector2 endScreenPos`
- **Methods:**
  - `Rect GetScreenRect()`
  - `Bounds GetWorldBounds(Camera camera, float zPlane = 0)`
  - `BoundsInt GetCellBounds(Grid grid, Tilemap tilemap)`
  - `void DrawOverlay(Color color)` (optional, for visual feedback)

Reusable for:
- Object selection
- Tile selection
- Rectangle drawing
- Future features (erase, copy-paste, batch edit)

---

## 2. Unified Drag Handling

In `LevelEditor.Update()`:

- On **mouse down**: initialize `DragArea` with start point
- On **mouse drag**: update end point
- On **mouse up**:
  - If Selecting: perform selection within drag area
  - If Drawing: perform rectangle fill within drag area

---

## 3. Rectangle Selection

### Objects
- Convert drag area to viewport/world rect
- Select all objects whose world positions fall inside

### Tiles
- Convert drag area to cell bounds
- Store selected tile positions (`HashSet<Vector3Int>`)
- Optionally highlight selected tiles

---

## 4. Rectangle Drawing

- Convert drag area to cell bounds
- On mouse release, fill all cells in bounds with the current tile
- Save operation in history for undo/redo

---

## 5. Visual Feedback

- During drag, draw a semi-transparent rectangle overlay
- Use UI canvas or GL lines
- Different colors for selection vs. drawing

---

## 6. Extensibility

Supports future features:
- Rectangle erase
- Copy-paste
- Batch edit
- Multi-layer editing

---

## 7. Implementation Roadmap

**Phase 1:** Implement DragArea utility  
**Phase 2:** Refactor object selection to use DragArea, add tile selection  
**Phase 3:** Add rectangle fill drawing  
**Phase 4:** Add visual feedback overlays  
**Phase 5:** Extend to future features

---

## Mermaid Diagram

```mermaid
flowchart TD
    subgraph Input
        A(Mouse Down)
        B(Mouse Drag)
        C(Mouse Up)
    end

    subgraph DragArea
        D[Start Point]
        E[End Point]
        F[Get Screen Rect]
        G[Get World Bounds]
        H[Get Cell Bounds]
    end

    A --> D
    B --> E
    E --> F
    E --> G
    E --> H

    C -->|Selecting| I[Select Objects + Tiles in Bounds]
    C -->|Drawing| J[Fill Tiles in Bounds]
    C -->|Erasing (future)| K[Erase Tiles + Objects in Bounds]
```

---

## Summary
- Centralize drag logic in `DragArea`
- Unify rectangle selection and drawing
- Provide visual feedback
- Design for extensibility