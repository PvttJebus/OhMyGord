namespace LevelEditorSystem
{
    /// <summary>
    /// States for the LevelEditor FSM.
    /// </summary>
    public enum EditorFSMState
    {
        Idle,
        Selecting,
        DraggingSelected,
        DrawingTiles,
        ErasingTiles,
        StickySpawning,
        GroupEditing
    }
}