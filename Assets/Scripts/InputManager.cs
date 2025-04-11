using UnityEngine;

public static class InputManager
{
    /// <summary>
    /// Checks for global debug inputs and triggers appropriate actions.
    /// </summary>
    public static void HandleGlobalDebugInput(GameManager gameManager)
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            gameManager.ReloadLevel();
        }

        if (Input.GetKey(KeyCode.Tilde))
        {
            gameManager.MainMenu();
        }
    }
}