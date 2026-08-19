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

        // Root panel
        GameObject rootObj = GameObject.Find("TokenBagRoot");
        if (rootObj == null)
        {
            rootObj = new GameObject("TokenBagRoot", typeof(RectTransform), typeof(Image));
            rootObj.transform.SetParent(canvasGO.transform, false);
            var rect = rootObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rootObj.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 1f);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Gameplay root
        GameObject gameplayRoot = GameObject.Find("GameplayRoot");
        if (gameplayRoot == null)
        {
            gameplayRoot = new GameObject("GameplayRoot", typeof(RectTransform));
            gameplayRoot.transform.SetParent(rootObj.transform, false);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Create top-right Menu button
        CreateNamedButtonIfMissing("MenuButton", rootObj.transform, new Vector2(180f, 70f), new Vector2(1f,1f), new Vector2(1f,1f), new Vector2(-90f,-70f), "Menu", 30, new Color(0.18f,0.18f,0.18f));

        // Gameplay buttons
        CreateNamedButtonIfMissing("DrawTokenButton", gameplayRoot.transform, new Vector2(430f,110f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,150f), "Draw a token", 34, new Color(0.24f,0.42f,0.69f));
        CreateNamedButtonIfMissing("PutBackButton", gameplayRoot.transform, new Vector2(320f,80f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,-40f), "Put Back", 28, new Color(0.30f,0.60f,0.54f));
        CreateNamedButtonIfMissing("RemoveButton", gameplayRoot.transform, new Vector2(320f,80f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,-150f), "Remove", 28, new Color(0.72f,0.24f,0.24f));

        // CurrentKillerText and RemovedKillersText
        CreateNamedTextIfMissing("CurrentKillerText", gameplayRoot.transform, new Vector2(520f,90f), new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), new Vector2(0f,60f), "", 36, TextAlignmentOptions.Center, Color.white);
        CreateNamedTextIfMissing("RemovedKillersText", gameplayRoot.transform, new Vector2(500f,340f), new Vector2(0.5f,0.95f), new Vector2(0.5f,0.95f), new Vector2(0f,-105f), "", 25, TextAlignmentOptions.Top, new Color(1f,0.9f,0.9f));

        // Menu panel
        GameObject menuPanel = GameObject.Find("MenuPanel");
        if (menuPanel == null)
        {
            menuPanel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
            menuPanel.transform.SetParent(rootObj.transform, false);
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
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Confirmation panel
        GameObject confirmationPanel = GameObject.Find("ConfirmationPanel");
        if (confirmationPanel == null)
        {
            confirmationPanel = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
            confirmationPanel.transform.SetParent(rootObj.transform, false);
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
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Removed selection panel
        GameObject selectionPanel = GameObject.Find("RemovedSelectionPanel");
        if (selectionPanel == null)
        {
            selectionPanel = new GameObject("RemovedSelectionPanel", typeof(RectTransform), typeof(Image));
            selectionPanel.transform.SetParent(rootObj.transform, false);
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
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        // Save prefabs for non-dynamic UI pieces
        // Ensure Assets/Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // To avoid nested-prefab circular dependency problems, delete any existing managed prefabs
        // and recreate them in a safe order. Intentionally avoid creating a Canvas prefab to keep
        // TokenBagRoot as an independent prefab that can be used under any Canvas.
        var managedPrefabs = new[] { "TokenBagRoot", "MenuPanel", "ConfirmationPanel", "RemovedSelectionPanel" };
        foreach (var pn in managedPrefabs)
        {
            var path = $"Assets/Prefabs/{pn}.prefab";
            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        // Save the prefabs. TokenBagRoot first to ensure others don't embed its scene instance.
        SaveAsPrefab(rootObj, "TokenBagRoot");
        SaveAsPrefab(menuPanel, "MenuPanel");
        SaveAsPrefab(confirmationPanel, "ConfirmationPanel");
        SaveAsPrefab(selectionPanel, "RemovedSelectionPanel");
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