# Selection and Parameter Editing Refactor Plan

## Goal

- Enable **single object selection** with a click (industry-standard UX).
- Support **multi-selection** with shift/ctrl (already present).
- Generalize the **parameter editing window** to:
  - Show parameters for a single selected object.
  - Show only common parameters for multi-selection.
  - Allow batch editing of shared parameters.
- Ensure **selection and parameter editing** UX matches industry leaders (e.g., Adobe, Figma, Unity).

---

## Selection Logic

### Single Selection

- **Click** on an object (no modifiers):
  - **Clear previous selection**.
  - **Select only the clicked object**.
  - **Highlight** the object.
  - **Show parameter window** for that object.

### Multi-Selection

- **Ctrl/Shift + Click** on objects:
  - **Add/remove** objects from selection.
  - **Highlight** all selected objects.
  - **Show parameter window** for all selected objects (see below).

### Drag Selection

- **Drag rectangle**:
  - Select all objects whose **visible bounds** overlap the rectangle.
  - **Highlight** all selected objects.
  - **Show parameter window** for all selected objects.

---

## Parameter Editing Window

### Single Selection

- Show **all editable parameters** for the selected object.
- Allow **editing and immediate update**.

### Multi-Selection

- Show **only parameters shared by all selected objects** (by name and type).
- If values differ, show a **mixed value indicator** (e.g., "--" or grayed out).
- Editing a parameter **applies to all selected objects**.

### UX Details

- **Auto-update** window on selection change.
- **Clear window** if nothing is selected.
- **Batch apply** changes to all selected objects.

---

## Implementation Steps

1. **SelectionState Refactor**
   - On click (no modifiers), clear selection and select only the clicked object.
   - On Ctrl/Shift+Click, add/remove objects from selection.
   - On drag, select all objects overlapping the rectangle.
   - Always update highlights to match selection.

2. **Parameter Window Refactor**
   - Refactor PropertyPanelUI to:
     - Accept a list of selected objects.
     - Compute intersection of editable parameters (by name/type).
     - Show mixed value indicator for differing values.
     - Apply edits to all selected objects.

3. **UI/UX Polish**
   - Add clear visual feedback for selection and parameter editing.
   - Ensure keyboard/mouse navigation is smooth.
   - Test for edge cases (empty selection, mixed types, etc).

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

## Summary

- **Single click** = single selection.
- **Ctrl/Shift+Click** = multi-selection.
- **Parameter window** always reflects current selection, supports batch editing.
- **Highlights** always match selection.
- **UX** is clear, responsive, and matches industry standards.

---

## Next Steps

- Refactor SelectingState and PropertyPanelUI as described.
- Test all selection and parameter editing flows.
- Polish UI/UX for clarity and responsiveness.