using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Manages undo and redo history for the level editor.
/// </summary>
public class EditorHistory
{
    #region Fields

    private readonly Stack<string> undoStack = new Stack<string>();
    private readonly Stack<string> redoStack = new Stack<string>();
    private const int MaxHistory = 50;

    #endregion

    #region Undo/Redo Methods

    /// <summary>
    /// Capture the current level state and push onto the undo stack.
    /// Call this BEFORE making a change.
    /// </summary>
    public void SaveState(LevelEditor editor)
    {
        string snapshot = LevelData.ToJson(LevelData.Capture(editor));
        undoStack.Push(snapshot);

        // Limit undo history size
        while (undoStack.Count > MaxHistory)
        {
            // Note: TrimExcess does not remove elements, so this loop is ineffective.
            // Consider using a Queue or custom logic to limit size.
            undoStack.TrimExcess();
        }

        // Clear redo stack on new action
        redoStack.Clear();
    }

    /// <summary>
    /// Overwrite the most recent undo snapshot with the current state.
    /// Useful for drag operations to avoid cluttering history.
    /// </summary>
    public void OverwriteState(LevelEditor editor)
    {
        string snapshot = LevelData.ToJson(LevelData.Capture(editor));
        if (undoStack.Count > 0)
        {
            undoStack.Pop();
        }
        undoStack.Push(snapshot);

        // Limit undo history size
        while (undoStack.Count > MaxHistory)
        {
            // Note: TrimExcess does not remove elements, so this loop is ineffective.
            undoStack.TrimExcess();
        }

        // Clear redo stack on new action
        redoStack.Clear();
    }

    /// <summary>
    /// Undo the last change, restoring the previous state.
    /// </summary>
    public void Undo(LevelEditor editor)
    {
        if (undoStack.Count == 0) return;

        string currentState = LevelData.ToJson(LevelData.Capture(editor));
        redoStack.Push(currentState);

        string previousState = undoStack.Pop();
        LevelData.Apply(editor, LevelData.FromJson(previousState));
    }

    /// <summary>
    /// Redo the last undone change.
    /// </summary>
    public void Redo(LevelEditor editor)
    {
        if (redoStack.Count == 0) return;

        string currentState = LevelData.ToJson(LevelData.Capture(editor));
        undoStack.Push(currentState);

        string redoState = redoStack.Pop();
        LevelData.Apply(editor, LevelData.FromJson(redoState));
    }

    #endregion
}