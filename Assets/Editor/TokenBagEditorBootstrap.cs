#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Editor-only bootstrap: ensures a TokenBagUIController GameObject and the full UI exist in the open scene
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

        // Ensure controller exists
        if (Object.FindObjectOfType<TokenBagUIController>() == null)
        {
            var go = new GameObject("TokenBagUIController");
            go.AddComponent<TokenBagUIController>();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Ensure the UI objects exist
        EnsureUiExists();
    }

    private static void EnsureUiExists()
    {
        // Locate or create Canvas
        GameObject canvasGO = Object.FindObjectOfType<Canvas>()?.gameObject;
        if (canvasGO == null)
        {
            canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Determine if a TokenBagRoot instance already exists under any Canvas in the scene.
        GameObject existingRoot = GameObject.Find("TokenBagRoot");

        // If a TokenBagRoot already exists in the scene, ensure a prefab asset exists for it.
        var prefabPath = "Assets/Prefabs/TokenBagRoot.prefab";
        if (existingRoot != null)
        {
            // Ensure folder
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // If prefab asset missing, save existingRoot as prefab
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                var saved = PrefabUtility.SaveAsPrefabAsset(existingRoot, prefabPath);
                if (saved == null)
                {
                    Debug.LogError($"Failed to save existing TokenBagRoot as prefab at {prefabPath}");
                }
                else
                {
                    Debug.Log($"Saved existing TokenBagRoot to {prefabPath}");
                }

                AssetDatabase.SaveAssets();
            }

            // No further creation needed
        }

        // If we don't have a TokenBagRoot in scene, create one and save as prefab
        if (existingRoot == null)
        {
            // Prepare a temporary root at scene root (not parented to Canvas) to create the prefab asset cleanly.
            GameObject tempRoot = new GameObject("TokenBagRoot_Temp", typeof(RectTransform), typeof(Image));
            var tempRect = tempRoot.GetComponent<RectTransform>();
            tempRect.anchorMin = Vector2.zero;
            tempRect.anchorMax = Vector2.one;
            tempRect.offsetMin = Vector2.zero;
            tempRect.offsetMax = Vector2.zero;
            tempRoot.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 1f);

            // Create GameplayRoot under temp root
            var gameplayRoot = new GameObject("GameplayRoot", typeof(RectTransform));
            gameplayRoot.transform.SetParent(tempRoot.transform, false);

            // Create buttons and labels under gameplayRoot
            CreateNamedButtonIfMissing("DrawTokenButton", gameplayRoot.transform, new Vector2(430f,110f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,150f), "Draw a token", 34, new Color(0.24f,0.42f,0.69f));
            CreateNamedButtonIfMissing("PutBackButton", gameplayRoot.transform, new Vector2(320f,80f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,-40f), "Put Back", 28, new Color(0.30f,0.60f,0.54f));
            CreateNamedButtonIfMissing("RemoveButton", gameplayRoot.transform, new Vector2(320f,80f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,-150f), "Remove", 28, new Color(0.72f,0.24f,0.24f));

            CreateNamedTextIfMissing("CurrentKillerText", gameplayRoot.transform, new Vector2(520f,90f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,60f), "", 36, TextAlignmentOptions.Center, Color.white);
            CreateNamedTextIfMissing("RemovedKillersText", gameplayRoot.transform, new Vector2(500f,340f), new Vector2(0.5f,0.95f), new Vector2(0.5f,0.95f), new Vector2(0f,-105f), "", 25, TextAlignmentOptions.Top, new Color(1f,0.9f,0.9f));

            // Create MenuButton and panels under temp root
            CreateNamedButtonIfMissing("MenuButton", tempRoot.transform, new Vector2(180f, 70f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(-90f,-70f), "Menu", 30, new Color(0.18f,0.18f,0.18f));

            var menuPanel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
            menuPanel.transform.SetParent(tempRoot.transform, false);
            var menuRect = menuPanel.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(0.5f, 0.5f);
            menuRect.anchorMax = new Vector2(0.5f, 0.5f);
            menuRect.sizeDelta = new Vector2(420f, 260f);
            menuRect.anchoredPosition = Vector2.zero;
            menuPanel.GetComponent<Image>().color = new Color(0.14f, 0.14f, 0.16f, 1f);

            CreateNamedButtonIfMissing("ResetBagButton", menuPanel.transform, new Vector2(300f,70f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0f,-50f), "Reset the bag", 28, new Color(0.17f,0.17f,0.17f));
            CreateNamedButtonIfMissing("ReturnFromRemovedButton", menuPanel.transform, new Vector2(300f,70f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0f,-150f), "Return from removed", 28, new Color(0.17f,0.17f,0.17f));
            CreateNamedButtonIfMissing("MenuCloseButton", menuPanel.transform, new Vector2(120f,52f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(-70f,-25f), "Close", 24, new Color(0.30f,0.30f,0.30f));
            menuPanel.SetActive(false);

            var confirmationPanel = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
            confirmationPanel.transform.SetParent(tempRoot.transform, false);
            var confRect = confirmationPanel.GetComponent<RectTransform>();
            confRect.anchorMin = new Vector2(0.5f,0.5f);
            confRect.anchorMax = new Vector2(0.5f,0.5f);
            confRect.sizeDelta = new Vector2(420f,220f);
            confRect.anchoredPosition = Vector2.zero;
            confirmationPanel.GetComponent<Image>().color = new Color(0.11f,0.11f,0.12f,1f);

            var prompt = CreateNamedTextIfMissing("ConfirmationPrompt", confirmationPanel.transform, new Vector2(360f,80f), new Vector2(0.5f,1f), new Vector2(0.5f,1f), new Vector2(0f,-40f), "Reset the bag?\nThis will restore all killers.", 25, TextAlignmentOptions.Center, Color.white);
            prompt.raycastTarget = false;
            CreateNamedButtonIfMissing("ConfirmYesButton", confirmationPanel.transform, new Vector2(150f,50f), new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(-80f,35f), "Yes", 24, new Color(0.27f,0.52f,0.29f));
            CreateNamedButtonIfMissing("ConfirmNoButton", confirmationPanel.transform, new Vector2(150f,50f), new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(80f,35f), "No", 24, new Color(0.55f,0.30f,0.30f));
            confirmationPanel.SetActive(false);

            var selectionPanel = new GameObject("RemovedSelectionPanel", typeof(RectTransform), typeof(Image));
            selectionPanel.transform.SetParent(tempRoot.transform, false);
            var selRect = selectionPanel.GetComponent<RectTransform>();
            selRect.anchorMin = new Vector2(0.5f,0.5f);
            selRect.anchorMax = new Vector2(0.5f,0.5f);
            selRect.sizeDelta = new Vector2(420f,440f);
            selRect.anchoredPosition = Vector2.zero;
            selectionPanel.GetComponent<Image>().color = new Color(0.10f,0.10f,0.13f,1f);

            var selectionInfo = new GameObject("SelectionInfo", typeof(RectTransform), typeof(Image));
            selectionInfo.transform.SetParent(selectionPanel.transform, false);
            var infoRect = selectionInfo.GetComponent<RectTransform>();
            infoRect.anchorMin = Vector2.zero;
            infoRect.anchorMax = Vector2.one;
            infoRect.offsetMin = new Vector2(25f,55f);
            infoRect.offsetMax = new Vector2(-25f,-70f);
            selectionInfo.GetComponent<Image>().color = new Color(0.21f,0.21f,0.21f,0.75f);

            CreateNamedButtonIfMissing("SelectionCloseButton", selectionPanel.transform, new Vector2(120f,42f), new Vector2(0.5f,0f), new Vector2(0.5f,0f), new Vector2(0f,24f), "Close", 22, new Color(0.30f,0.30f,0.30f));

            selectionPanel.SetActive(false);

            // Create prefabs cleanly by saving the temp root as a prefab asset
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Remove any existing managed prefabs to avoid stale nested references

            // Save tempRoot as TokenBagRoot prefab (create asset only)
            var tokenBagPrefabPath = "Assets/Prefabs/TokenBagRoot.prefab";
            // Ensure folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            var saved = PrefabUtility.SaveAsPrefabAsset(tempRoot, tokenBagPrefabPath);
            if (saved == null)
            {
                Debug.LogError($"Failed to save prefab at {tokenBagPrefabPath}");
            }
            AssetDatabase.SaveAssets();

            // Instantiate the new prefab under the Canvas in the scene
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(tokenBagPrefabPath);
            if (prefabAsset != null && canvasGO != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
                instance.name = "TokenBagRoot";
                instance.transform.SetParent(canvasGO.transform, false);
                EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            // Remove the temp root used for prefab creation
            GameObject.DestroyImmediate(tempRoot);
        }

        // If a TokenBagRoot now exists in scene, ensure its child structures exist (safety)
        GameObject tokenBagRoot = GameObject.Find("TokenBagRoot");
        if (tokenBagRoot != null)
        {
            var gameplayRoot = tokenBagRoot.transform.Find("GameplayRoot");
            if (gameplayRoot == null)
            {
                var gr = new GameObject("GameplayRoot", typeof(RectTransform));
                gr.transform.SetParent(tokenBagRoot.transform, false);
            }

            var menuPanel = tokenBagRoot.transform.Find("MenuPanel");
            if (menuPanel == null)
            {
                var mp = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
                mp.transform.SetParent(tokenBagRoot.transform, false);
                mp.GetComponent<Image>().color = new Color(0.14f,0.14f,0.16f,1f);
            }

            var confirmationPanel = tokenBagRoot.transform.Find("ConfirmationPanel");
            if (confirmationPanel == null)
            {
                var cp = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
                cp.transform.SetParent(tokenBagRoot.transform, false);
                cp.GetComponent<Image>().color = new Color(0.11f,0.11f,0.12f,1f);
            }

            var selectionPanel = tokenBagRoot.transform.Find("RemovedSelectionPanel");
            if (selectionPanel == null)
            {
                var sp = new GameObject("RemovedSelectionPanel", typeof(RectTransform), typeof(Image));
                sp.transform.SetParent(tokenBagRoot.transform, false);
                sp.GetComponent<Image>().color = new Color(0.10f,0.10f,0.13f,1f);
            }

        }

        // Save prefabs for non-dynamic UI pieces
        // Ensure Assets/Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // // To avoid nested-prefab circular dependency problems, delete any existing managed prefabs
        // // and recreate them in a safe order. Intentionally avoid creating a Canvas prefab to keep
        // // TokenBagRoot as an independent prefab that can be used under any Canvas.
        // var managedPrefabs = new[] { "TokenBagRoot", "MenuPanel", "ConfirmationPanel", "RemovedSelectionPanel" };
        // foreach (var pn in managedPrefabs)
        // {
        //     var path = $"Assets/Prefabs/{pn}.prefab";
        //     if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
        //     {
        //         AssetDatabase.DeleteAsset(path);
        //     }
        // }

    }

    private static void SaveAsPrefab(GameObject instance, string prefabName)
    {
        if (instance == null) return;

        try
        {
            // If the provided instance is part of an existing prefab instance, resolve the outermost root
            if (PrefabUtility.IsPartOfPrefabInstance(instance))
            {
                var root = PrefabUtility.GetOutermostPrefabInstanceRoot(instance);
                if (root != null)
                {
                    instance = root;
                }
            }

            var prefabPath = $"Assets/Prefabs/{prefabName}.prefab";

            // If a prefab asset already exists at this path, overwrite it using SaveAsPrefabAsset
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null)
            {
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            }
            else
            {
                // Create a new prefab asset and connect the scene instance to it
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);
            }

            AssetDatabase.SaveAssets();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save prefab '{prefabName}': {ex.Message}");
        }
    }

    private static void CreateNamedButtonIfMissing(string name, Transform parent, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, string text, int fontSize, Color buttonColor)
    {
        if (parent == null) return;
        var existing = parent.Find(name);
        if (existing != null) return;

        var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;

        var image = buttonObject.GetComponent<Image>();
        image.color = buttonColor;

        var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        var textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        textComponent.raycastTarget = false;
        textComponent.enableWordWrapping = false;

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    private static TextMeshProUGUI CreateNamedTextIfMissing(string name, Transform parent, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, string text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        if (parent == null) return null;
        var existing = parent.Find(name);
        if (existing != null) return existing.GetComponent<TextMeshProUGUI>();

        var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        var rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.sizeDelta = size;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;

        var textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.alignment = alignment;
        textComponent.color = color;
        textComponent.raycastTarget = false;
        textComponent.enableWordWrapping = true;

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        return textComponent;
    }
}
#endif