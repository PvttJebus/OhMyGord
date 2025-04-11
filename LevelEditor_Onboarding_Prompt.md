# LevelEditor Project Onboarding Prompt

Welcome to the LevelEditor project! This document will get you up to speed on the architecture, design goals, and key workflows of the FSM-driven LevelEditor system. It is intended for new developers, QA, and LLM coding assistants joining the finalization and polish phase.

---

## Project Overview

- **Type**: 2D Level Editor for Unity, supporting tile and object placement, selection, parameter editing, and undo/redo.
- **Architecture**: Modern, modular, and FSM-driven. All user interaction is managed by a finite state machine (FSM) with explicit states and a shared context.
- **Design Goal**: Match or exceed the UX of industry leaders (Adobe, Figma, Unity) for selection, editing, and feedback.

---

## Key Concepts

### FSM-Driven Interaction

- **LevelEditor**: Pure data/model class. No direct input handling or state management.
- **LevelEditorController**: Owns the FSM, manages state transitions, and holds the shared FSM context.
- **FSM Context**: All interaction state (selection, drag, highlights, sticky spawn, etc.) is stored in a single LevelEditorContext object.
- **FSM States**:
  - **SelectingState**: Handles all selection logic (single, multi, drag), highlights, and transitions.
  - **DraggingSelectedState**: Unified drag state for both single and multi-selection. Handles object/tile movement, highlight, and overlay.
  - **DrawingTilesState**: Handles tile painting with brush preview, brush size, and prevents drawing over objects.
  - **StickySpawningState**: Handles sticky spawn placement, Alt+Click clone, and highlight.
  - **ErasingState**: Handles tile/object erasing with brush.
  - **IdleState**: Placeholder for future expansion.

### Selection & Parameter Editing

- **Single click**: Selects one object, highlights it, and shows its parameters.
- **Ctrl/Shift+Click**: Adds/removes objects from selection, highlights all, and shows only common parameters.
- **Drag rectangle**: Selects all objects whose visible bounds overlap the rectangle, highlights all, and shows only common parameters.
- **Parameter window**: Updates live with selection, supports batch editing, and shows mixed value indicators for differing values.

### Tile Drawing

- **Brush size**: Always odd, increments by 2, fills a square area.
- **Preview**: Overlay matches brush size, only visible in drawing mode.
- **Cannot draw over objects**: Brush skips cells with LevelObjects.

### Camera & UI

- **Camera**: Pans with middle mouse or WASD, zooms with scroll.
- **UI**: All buttons and palette interactions trigger FSM transitions.

### Level Save/Load

- **LevelData**: Now includes a timestamp for sorting.
- **LevelSaveLoadUI**: Sorts levels by timestamp, oldest first.

---

## Design Goals

- **Industry-standard UX**: Selection, dragging, and parameter editing match the best practices of tools like Adobe, Figma, and Unity.
- **Maintainability**: All logic is modular, testable, and easy to extend.
- **Performance**: No redundant per-frame highlight or material changes; all state transitions are efficient.
- **Extensibility**: New states, tools, or parameter types can be added with minimal friction.
- **No regressions**: All previous bugs (highlight leaks, selection mismatch, drag issues, etc.) have been addressed.

---

## Key Workflows

### Selection

- **Single click**: Selects one object, clears previous selection, highlights, and shows parameter window.
- **Ctrl/Shift+Click**: Adds/removes objects from selection, highlights all, and shows only common parameters.
- **Drag rectangle**: Selects all objects whose visible bounds overlap the rectangle, highlights all, and shows only common parameters.

### Dragging

- **DraggingSelectedState**: Handles both single and multi-selection drags. Moves all selected objects and tiles, applies highlights, and manages overlay.

### Parameter Editing

- **PropertyPanelUI**: Accepts a list of selected objects, computes intersection of editable parameters, shows mixed value indicators, and applies batch edits.

### Tile Drawing

- **Brush**: Fills a square area, preview matches brush size, cannot draw over objects, and only visible in drawing mode.

---

## Finalization Tasks

- **UI/UX Polish**: Review all visual feedback, transitions, and parameter editing flows for clarity and responsiveness.
- **Edge Cases**: Test empty selection, mixed types, and rapid state changes.
- **Documentation**: Ensure all code and UI elements are clearly documented for future maintainers.
- **Performance Testing**: Profile for any remaining bottlenecks in large levels or with many objects.
- **Accessibility**: Review keyboard navigation, color contrast, and input handling for accessibility.

---

## Mermaid Diagram

```mermaid
flowchart TD
    A[User Clicks Object] -- No Modifiers --> B[Clear Selection, Select Object, Show Params]
    A -- Ctrl/Shift --> C[Add/Remove Object to Selection, Show Params]
    D[User Drags Rectangle] --> E[Select All Overlapping, Show Params]
    F[Selection Changes] --> G[Update Highlights & Param Window]
    G --> H[Show All Params (Single)] & I[Show Common Params (Multi)]
    I --> J[Show Mixed Value Indicator if needed]
    J --> K[Batch Edit on Change]
```

---

## Handoff Notes

- The codebase is ready for final polish and QA.
- All major architectural and UX goals have been met.
- Please review the Selection_ParameterEditing_Refactor_Plan.md for detailed design intent and rationale.
- For any new features or bug fixes, follow the established FSM and context-driven patterns.