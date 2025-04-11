# In-Editor Property Editing UI — Implementation Plan

---

## **Goal**

Enable designers to view and modify LevelObject parameters **directly within the in-game editor**, improving workflow speed and flexibility.

---

## **Subtasks**

### 1. **Define Editable Properties**

- Create a custom attribute `[EditableInLevelEditor]` to mark fields/properties.
- Support optional metadata: display name, min/max, tooltip.

---

### 2. **Build the Property Panel UI**

- Design a Unity UI Canvas overlay or dockable panel.
- Add a scrollable container for property fields.
- Style for clarity and usability.

---

### 3. **Implement Reflection-Based Property Discovery**

- When a LevelObject is selected or in edit mode:
  - Use reflection to find all `[EditableInLevelEditor]` fields/properties.
  - Read metadata for display customization.

---

### 4. **Generate UI Controls Dynamically**

- For each property:
  - Create appropriate UI element (slider, toggle, input field, dropdown, color picker).
  - Initialize with current value.
  - Bind change events to update the property live.

---

### 5. **Handle Change Management**

- Before applying changes, save the object's state for undo.
- Support undo/redo integration with existing editor history.

---

### 6. **Prefab Defaults & Overrides**

- Compare current values to prefab defaults (using `originalPrefabInstanceID`).
- Indicate overridden properties visually.
- Add "Reset to Default" button per property.

---

### 7. **Extensibility**

- Allow custom drawers for complex types.
- Support nested components if needed.

---

## **Optional Enhancements**

- Multi-object editing support.
- Search/filter properties.
- Collapsible property groups.

---

## **Summary**

This plan will provide a **powerful, user-friendly** in-game property editor, enabling rapid iteration and fine-tuning of LevelObjects without leaving the level design environment.
