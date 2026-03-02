using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple main menu controller with two buttons: Play and Quit.
/// Attach this script to an empty GameObject in your menu scene,
/// then hook the public methods to your UI buttons' OnClick events.
/// </summary>
public class menu : MonoBehaviour
{
    [Tooltip("Name of the scene to load when Play is pressed.")]
    public string gameSceneName = "Gameplay";

    /// <summary>
    /// Called by the Play button.  Loads the game scene.
    /// </summary>
    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[menu] gameSceneName is not set!");
            return;
        }

        Debug.Log("[menu] Loading scene: " + gameSceneName);
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Called by the Quit button.  Exits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[menu] Quit requested.");
        Time.timeScale = 1f; // Ensure time is running
        Application.Quit();

        // in the editor, Application.Quit() does nothing, so log to console
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
