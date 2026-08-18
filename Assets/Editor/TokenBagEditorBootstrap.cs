#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only bootstrap: ensures a TokenBagUIController GameObject exists in the open scene
[InitializeOnLoad]
public static class TokenBagEditorBootstrap
{
    static TokenBagEditorBootstrap()
    {
        // Delay call so it's safe during assembly reloads
        EditorApplication.delayCall += EnsureTokenBagInScene;
    }

    private static void EnsureTokenBagInScene()
    {
        // Don't run while entering Play mode
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // If a controller already exists, do nothing
        if (Object.FindObjectOfType<TokenBagUIController>() != null)
            return;

        // Create the controller GameObject in the current scene so the UI objects are visible in the Hierarchy
        var go = new GameObject("TokenBagUIController");
        go.AddComponent<TokenBagUIController>();

        // Mark the scene dirty so the user can save the scene with the new object
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
#endif