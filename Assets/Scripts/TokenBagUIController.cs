using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenBagUIController : MonoBehaviour
{
    private const float ReferenceWidth = 1080f;
    private const float ReferenceHeight = 2400f;

    private readonly KillerBagState gameState = new KillerBagState();

    private Canvas canvas;
    private CanvasScaler canvasScaler;
    private RectTransform rootPanel;
    private GameObject gameplayRoot;
    private GameObject menuRoot;
    private GameObject confirmationRoot;
    private GameObject removedSelectionRoot;
    private GameObject selectionInfoRoot;

    private Button menuButton;
    private Button drawTokenButton;
    private Button putBackButton;
    private Button removeButton;

    private TextMeshProUGUI currentKillerText;
    private TextMeshProUGUI removedKillersText;

    private Button resetBagButton;
    private Button returnFromRemovedButton;
    private Button menuCloseButton;
    private Button confirmationYesButton;
    private Button confirmationNoButton;
    private Button selectionCloseButton;

    private readonly List<Button> returnableButtons = new List<Button>();

    private bool menuOpen;
    private bool confirmationOpen;
    private bool returnSelectionOpen;

    // At runtime, bind to UI GameObjects created in the Editor.
    // The Editor bootstrap will create the UI once; TokenBagUIController uses those objects at runtime.
    private void Awake()
    {
        BindReferences();
        gameState.Reset();
        RefreshGameUi();
    }

    private void BindReferences()
    {
        // Find primary roots
        canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene. Ensure the Editor has created the UI before running.");
            return;
        }

        var rootObj = GameObject.Find("TokenBagRoot");
        if (rootObj == null)
        {
            Debug.LogError("TokenBagRoot not found in scene. Ensure the Editor has created the UI before running.");
            return;
        }

        rootPanel = rootObj.GetComponent<RectTransform>();
        // Prefer references under TokenBagRoot to avoid collisions with other scene objects
        Transform rootTransform = rootObj.transform;

        var gameplayTransform = rootTransform.Find("GameplayRoot");
        gameplayRoot = gameplayTransform != null ? gameplayTransform.gameObject : rootObj.transform.Find("GameplayRoot")?.gameObject;

        var menuTransform = rootTransform.Find("MenuPanel");
        menuRoot = menuTransform != null ? menuTransform.gameObject : GameObject.Find("MenuPanel");

        var confirmationTransform = rootTransform.Find("ConfirmationPanel");
        confirmationRoot = confirmationTransform != null ? confirmationTransform.gameObject : GameObject.Find("ConfirmationPanel");

        var removedSelectionTransform = rootTransform.Find("RemovedSelectionPanel");
        removedSelectionRoot = removedSelectionTransform != null ? removedSelectionTransform.gameObject : GameObject.Find("RemovedSelectionPanel");

        var selectionInfoObj = rootTransform.Find("RemovedSelectionPanel/SelectionInfo")?.gameObject ?? GameObject.Find("SelectionInfo");
        selectionInfoRoot = selectionInfoObj;

        // Find buttons and wire up listeners by searching under rootObj first
        var buttons = rootObj.GetComponentsInChildren<Button>(true);
        foreach (var btn in buttons)
        {
            switch (btn.gameObject.name)
            {
                case "MenuButton":
                    menuButton = btn;
                    menuButton.onClick.AddListener(OnMenuButtonClicked);
                    break;
                case "DrawTokenButton":
                    drawTokenButton = btn;
                    drawTokenButton.onClick.AddListener(OnDrawTokenClicked);
                    break;
                case "PutBackButton":
                    putBackButton = btn;
                    putBackButton.onClick.AddListener(OnPutBackClicked);
                    break;
                case "RemoveButton":
                    removeButton = btn;
                    removeButton.onClick.AddListener(OnRemoveClicked);
                    break;
                case "ResetBagButton":
                    resetBagButton = btn;
                    resetBagButton.onClick.AddListener(OnResetBagClicked);
                    break;
                case "ReturnFromRemovedButton":
                    returnFromRemovedButton = btn;
                    returnFromRemovedButton.onClick.AddListener(OnReturnFromRemovedClicked);
                    break;
                case "MenuCloseButton":
                    menuCloseButton = btn;
                    menuCloseButton.onClick.AddListener(CloseMenu);
                    break;
                case "ConfirmYesButton":
                    confirmationYesButton = btn;
                    confirmationYesButton.onClick.AddListener(OnResetConfirmed);
                    break;
                case "ConfirmNoButton":
                    confirmationNoButton = btn;
                    confirmationNoButton.onClick.AddListener(OnConfirmationCancelled);
                    break;
                case "SelectionCloseButton":
                    selectionCloseButton = btn;
                    selectionCloseButton.onClick.AddListener(CloseRemovedSelection);
                    break;
            }
        }

        // Also try global finds if any button was not found under root
        if (menuButton == null) menuButton = GameObject.Find("MenuButton")?.GetComponent<Button>();
        if (menuButton != null && !menuButton.onClick.GetPersistentEventCount().Equals(0)) { /* already hooked */ } else if (menuButton != null) menuButton.onClick.AddListener(OnMenuButtonClicked);

        if (drawTokenButton == null) drawTokenButton = GameObject.Find("DrawTokenButton")?.GetComponent<Button>();
        if (drawTokenButton != null && drawTokenButton.onClick.GetPersistentEventCount().Equals(0)) drawTokenButton.onClick.AddListener(OnDrawTokenClicked);

        if (putBackButton == null) putBackButton = GameObject.Find("PutBackButton")?.GetComponent<Button>();
        if (putBackButton != null && putBackButton.onClick.GetPersistentEventCount().Equals(0)) putBackButton.onClick.AddListener(OnPutBackClicked);

        if (removeButton == null) removeButton = GameObject.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null && removeButton.onClick.GetPersistentEventCount().Equals(0)) removeButton.onClick.AddListener(OnRemoveClicked);

        if (resetBagButton == null) resetBagButton = GameObject.Find("ResetBagButton")?.GetComponent<Button>();
        if (resetBagButton != null && resetBagButton.onClick.GetPersistentEventCount().Equals(0)) resetBagButton.onClick.AddListener(OnResetBagClicked);

        if (returnFromRemovedButton == null) returnFromRemovedButton = GameObject.Find("ReturnFromRemovedButton")?.GetComponent<Button>();
        if (returnFromRemovedButton != null && returnFromRemovedButton.onClick.GetPersistentEventCount().Equals(0)) returnFromRemovedButton.onClick.AddListener(OnReturnFromRemovedClicked);

        if (menuCloseButton == null) menuCloseButton = GameObject.Find("MenuCloseButton")?.GetComponent<Button>();
        if (menuCloseButton != null && menuCloseButton.onClick.GetPersistentEventCount().Equals(0)) menuCloseButton.onClick.AddListener(CloseMenu);

        if (confirmationYesButton == null) confirmationYesButton = GameObject.Find("ConfirmYesButton")?.GetComponent<Button>();
        if (confirmationYesButton != null && confirmationYesButton.onClick.GetPersistentEventCount().Equals(0)) confirmationYesButton.onClick.AddListener(OnResetConfirmed);

        if (confirmationNoButton == null) confirmationNoButton = GameObject.Find("ConfirmNoButton")?.GetComponent<Button>();
        if (confirmationNoButton != null && confirmationNoButton.onClick.GetPersistentEventCount().Equals(0)) confirmationNoButton.onClick.AddListener(OnConfirmationCancelled);

        if (selectionCloseButton == null) selectionCloseButton = GameObject.Find("SelectionCloseButton")?.GetComponent<Button>();
        if (selectionCloseButton != null && selectionCloseButton.onClick.GetPersistentEventCount().Equals(0)) selectionCloseButton.onClick.AddListener(CloseRemovedSelection);

        // Texts: search under root first
        currentKillerText = rootObj.GetComponentInChildren<TextMeshProUGUI>(true);
        // Attempt to find by name
        var tmpros = rootObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in tmpros)
        {
            if (t.gameObject.name == "CurrentKillerText") currentKillerText = t;
            if (t.gameObject.name == "RemovedKillersText") removedKillersText = t;
        }

        if (currentKillerText == null) currentKillerText = GameObject.Find("CurrentKillerText")?.GetComponent<TextMeshProUGUI>();
        if (removedKillersText == null) removedKillersText = GameObject.Find("RemovedKillersText")?.GetComponent<TextMeshProUGUI>();

        // Ensure panels are deactivated by default
        if (menuRoot != null) menuRoot.SetActive(false);
        if (confirmationRoot != null) confirmationRoot.SetActive(false);
        if (removedSelectionRoot != null) removedSelectionRoot.SetActive(false);

        // Gameplay root active
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
    }


    private void CloseAllPanels()
    {
        menuRoot.SetActive(false);
        confirmationRoot.SetActive(false);
        removedSelectionRoot.SetActive(false);
        menuOpen = false;
        confirmationOpen = false;
        returnSelectionOpen = false;

        gameplayRoot.SetActive(true);
    }

    private void SetGameplayControlsActive(bool isActive)
    {
        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(isActive);
        }
    }

    private void OnMenuButtonClicked()
    {
        if (menuOpen || confirmationOpen || returnSelectionOpen)
        {
            return;
        }

        menuOpen = true;
        if (menuRoot == null)
        {
            Debug.LogError("MenuPanel (menuRoot) not found. Ensure the UI prefabs were created and TokenBagRoot is present in the scene.");
            menuOpen = false;
            return;
        }

        menuRoot.SetActive(true);
        SetGameplayControlsActive(false);
    }

    private void CloseMenu()
    {
        menuOpen = false;
        confirmationOpen = false;
        menuRoot.SetActive(false);
        confirmationRoot.SetActive(false);
        SetGameplayControlsActive(true);
        RefreshGameUi();
    }

    private void OnResetBagClicked()
    {
        if (!menuOpen)
        {
            return;
        }

        menuRoot.SetActive(false);
        confirmationRoot.SetActive(true);
        confirmationOpen = true;
        SetGameplayControlsActive(false);
    }

    private void OnConfirmationCancelled()
    {
        confirmationRoot.SetActive(false);
        confirmationOpen = false;
        menuRoot.SetActive(true);
        menuOpen = true;
        SetGameplayControlsActive(false);
    }

    private void OnResetConfirmed()
    {
        gameState.Reset();
        CloseMenu();
        RefreshGameUi();
    }

    private void OnReturnFromRemovedClicked()
    {
        if (!menuOpen)
        {
            return;
        }

        if (gameState.removedKillersList.Count == 0)
        {
            ShowNothingToReturnDialog();
            return;
        }

        // Hide the menu (we will return to it if the player closes the selection without choosing).
        menuRoot.SetActive(false);
        menuOpen = false;
        BuildReturnSelectionList();
        removedSelectionRoot.SetActive(true);
        returnSelectionOpen = true;
        SetGameplayControlsActive(false);
    }

    private void ShowNothingToReturnDialog()
    {
        menuRoot.SetActive(false);
        confirmationRoot.SetActive(true);
        confirmationOpen = true;
        SetGameplayControlsActive(false);

        var prompt = confirmationRoot.transform.Find("ConfirmationPrompt").GetComponent<TextMeshProUGUI>();
        prompt.text = "There is nothing to return.";
        confirmationYesButton.gameObject.SetActive(false);
        confirmationNoButton.gameObject.SetActive(false);

        var closePromptButton = CreateTextButton(
            confirmationRoot.GetComponent<RectTransform>(),
            "ConfirmationCloseButton",
            new Vector2(150f, 50f),
            new Vector2(0.5f, 0f),
            new Vector2(0.5f, 0f),
            new Vector2(0f, 35f),
            "Close",
            24,
            new Color(0.46f, 0.46f, 0.46f, 1f));

        closePromptButton.onClick.AddListener(() =>
        {
            confirmationOpen = false;
            confirmationRoot.SetActive(false);
            confirmationYesButton.gameObject.SetActive(true);
            confirmationNoButton.gameObject.SetActive(true);
            Destroy(closePromptButton.gameObject);
            menuRoot.SetActive(true);
            menuOpen = true;
            SetGameplayControlsActive(false);
        });
    }

    private void BuildReturnSelectionList()
    {
        foreach (var button in returnableButtons)
        {
            Destroy(button.gameObject);
        }

        returnableButtons.Clear();

        var parent = removedSelectionRoot.transform.Find("SelectionInfo");
        var verticalOffset = 0f;

        foreach (var killer in gameState.removedKillersList)
        {
            var option = CreateTextButton(
                parent as RectTransform,
                $"ReturnOption_{killer}",
                new Vector2(260f, 42f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f - verticalOffset),
                KillerBagState.GetDisplayName(killer),
                22,
                new Color(0.23f, 0.23f, 0.23f, 1f));

            var killerValue = killer;
            option.onClick.AddListener(() => OnReturnKillerClicked(killerValue));
            returnableButtons.Add(option);
            verticalOffset += 50f;
        }
    }

    private void OnReturnKillerClicked(Killer killer)
    {
        if (!gameState.removedKillersList.Contains(killer))
        {
            return;
        }

        gameState.removedKillersList.Remove(killer);
        gameState.bagKillersList.Add(killer);
        RefreshGameUi();
        CloseRemovedSelection();
        menuOpen = false;
        menuRoot.SetActive(false);
        SetGameplayControlsActive(true);
    }

    private void CloseRemovedSelection()
    {
        if (removedSelectionRoot != null)
            removedSelectionRoot.SetActive(false);

        returnSelectionOpen = false;

        // Return to menu if it was the source of the selection
        if (menuRoot != null)
        {
            menuRoot.SetActive(true);
            menuOpen = true;
        }

        SetGameplayControlsActive(true);
    }

    private void OnDrawTokenClicked()
    {
        if (!gameState.CanDrawToken)
        {
            return;
        }

        gameState.DrawRandomKiller();
        RefreshGameUi();
    }

    private void OnPutBackClicked()
    {
        if (!gameState.currentKiller.HasValue)
        {
            return;
        }

        gameState.PutBackCurrentKiller();
        RefreshGameUi();
    }

    private void OnRemoveClicked()
    {
        if (!gameState.currentKiller.HasValue)
        {
            return;
        }

        gameState.RemoveCurrentKiller();
        RefreshGameUi();
    }

    private void RefreshGameUi()
    {
        bool hasCurrent = gameState.currentKiller.HasValue;

        drawTokenButton.gameObject.SetActive(!hasCurrent && gameState.bagKillersList.Count > 0);
        putBackButton.gameObject.SetActive(hasCurrent);
        removeButton.gameObject.SetActive(hasCurrent);
        currentKillerText.gameObject.SetActive(hasCurrent);

        if (hasCurrent)
        {
            currentKillerText.text = gameState.GetCurrentDisplayName();
        }
        else
        {
            currentKillerText.text = string.Empty;
        }

        bool hasRemoved = gameState.removedKillersList.Count > 0;
        removedKillersText.gameObject.SetActive(hasRemoved);
        removedKillersText.text = gameState.GetRemovedDisplayText();

        if (!menuOpen && !confirmationOpen && !returnSelectionOpen)
        {
            SetGameplayControlsActive(true);
        }
        else
        {
            SetGameplayControlsActive(false);
        }
    }

    private static Button CreateTextButton(RectTransform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, string text, int fontSize, Color buttonColor)
    {
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

        return buttonObject.GetComponent<Button>();
    }

    private static TextMeshProUGUI CreateTextObject(RectTransform parent, string name, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, string text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
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

        return textComponent;
    }
}
