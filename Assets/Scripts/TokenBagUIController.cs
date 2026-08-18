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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<TokenBagUIController>() != null)
        {
            return;
        }

        var appObject = new GameObject("TokenBagUIController");
        appObject.AddComponent<TokenBagUIController>();
    }

    private void Awake()
    {
        BuildUi();
        gameState.Reset();
        RefreshGameUi();
    }

    private void BuildUi()
    {
        CreateCanvas();
        CreateRootPanel();
        CreateMenuButton();
        CreateGameplayButtons();
        CreateTextLabels();
        CreateMenuPanel();
        CreateConfirmationPanel();
        CreateRemovedSelectionPanel();
        CloseAllPanels();
    }

    private void CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
    }

    private void CreateRootPanel()
    {
        rootPanel = new GameObject("TokenBagRoot", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
        rootPanel.SetParent(canvas.transform, false);
        rootPanel.anchorMin = Vector2.zero;
        rootPanel.anchorMax = Vector2.one;
        rootPanel.offsetMin = Vector2.zero;
        rootPanel.offsetMax = Vector2.zero;
        rootPanel.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.10f, 1f);

        gameplayRoot = new GameObject("GameplayRoot", typeof(RectTransform));
        gameplayRoot.transform.SetParent(rootPanel, false);
    }

    private void CreateMenuButton()
    {
        menuButton = CreateTextButton(
            rootPanel,
            "MenuButton",
            new Vector2(180f, 70f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-90f, -70f),
            "Menu",
            30,
            new Color(0.18f, 0.18f, 0.18f, 1f));

        menuButton.onClick.AddListener(OnMenuButtonClicked);
    }

    private void CreateGameplayButtons()
    {
        drawTokenButton = CreateTextButton(
            gameplayRoot.transform as RectTransform,
            "DrawTokenButton",
            new Vector2(430f, 110f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 150f),
            "Draw a token",
            34,
            new Color(0.24f, 0.42f, 0.69f, 1f));

        drawTokenButton.onClick.AddListener(OnDrawTokenClicked);

        putBackButton = CreateTextButton(
            gameplayRoot.transform as RectTransform,
            "PutBackButton",
            new Vector2(320f, 80f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -40f),
            "Put Back",
            28,
            new Color(0.30f, 0.60f, 0.54f, 1f));

        putBackButton.onClick.AddListener(OnPutBackClicked);

        removeButton = CreateTextButton(
            gameplayRoot.transform as RectTransform,
            "RemoveButton",
            new Vector2(320f, 80f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -150f),
            "Remove",
            28,
            new Color(0.72f, 0.24f, 0.24f, 1f));

        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    private void CreateTextLabels()
    {
        currentKillerText = CreateTextObject(
            gameplayRoot.transform as RectTransform,
            "CurrentKillerText",
            new Vector2(520f, 90f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, 60f),
            "",
            36,
            TextAlignmentOptions.Center,
            new Color(1f, 1f, 1f, 1f));

        removedKillersText = CreateTextObject(
            gameplayRoot.transform as RectTransform,
            "RemovedKillersText",
            new Vector2(500f, 340f),
            new Vector2(0.5f, 0.95f),
            new Vector2(0.5f, 0.95f),
            new Vector2(0f, -105f),
            "",
            25,
            TextAlignmentOptions.Top,
            new Color(1f, 0.9f, 0.9f, 1f));
    }

    private void CreateMenuPanel()
    {
        menuRoot = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
        menuRoot.transform.SetParent(rootPanel, false);
        RectTransform menuRect = menuRoot.GetComponent<RectTransform>();
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.sizeDelta = new Vector2(420f, 260f);
        menuRect.anchoredPosition = Vector2.zero;
        menuRoot.GetComponent<Image>().color = new Color(0.14f, 0.14f, 0.16f, 1f);

        resetBagButton = CreateTextButton(menuRect, "ResetBagButton", new Vector2(300f, 70f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), "Reset the bag", 28, new Color(0.17f, 0.17f, 0.17f, 1f));
        returnFromRemovedButton = CreateTextButton(menuRect, "ReturnFromRemovedButton", new Vector2(300f, 70f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), "Return from removed", 28, new Color(0.17f, 0.17f, 0.17f, 1f));
        menuCloseButton = CreateTextButton(menuRect, "MenuCloseButton", new Vector2(120f, 52f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-70f, -25f), "Close", 24, new Color(0.30f, 0.30f, 0.30f, 1f));

        resetBagButton.onClick.AddListener(OnResetBagClicked);
        returnFromRemovedButton.onClick.AddListener(OnReturnFromRemovedClicked);
        menuCloseButton.onClick.AddListener(CloseMenu);

        menuRoot.SetActive(false);
    }

    private void CreateConfirmationPanel()
    {
        confirmationRoot = new GameObject("ConfirmationPanel", typeof(RectTransform), typeof(Image));
        confirmationRoot.transform.SetParent(rootPanel, false);
        RectTransform confirmationRect = confirmationRoot.GetComponent<RectTransform>();
        confirmationRect.anchorMin = new Vector2(0.5f, 0.5f);
        confirmationRect.anchorMax = new Vector2(0.5f, 0.5f);
        confirmationRect.sizeDelta = new Vector2(420f, 220f);
        confirmationRect.anchoredPosition = Vector2.zero;
        confirmationRoot.GetComponent<Image>().color = new Color(0.11f, 0.11f, 0.12f, 1f);

        var prompt = CreateTextObject(confirmationRect, "ConfirmationPrompt", new Vector2(360f, 80f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), "Reset the bag?\nThis will restore all killers.", 25, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 1f));
        prompt.raycastTarget = false;

        confirmationYesButton = CreateTextButton(confirmationRect, "ConfirmYesButton", new Vector2(150f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-80f, 35f), "Yes", 24, new Color(0.27f, 0.52f, 0.29f, 1f));
        confirmationNoButton = CreateTextButton(confirmationRect, "ConfirmNoButton", new Vector2(150f, 50f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(80f, 35f), "No", 24, new Color(0.55f, 0.30f, 0.30f, 1f));

        confirmationYesButton.onClick.AddListener(OnResetConfirmed);
        confirmationNoButton.onClick.AddListener(OnConfirmationCancelled);

        confirmationRoot.SetActive(false);
    }

    private void CreateRemovedSelectionPanel()
    {
        removedSelectionRoot = new GameObject("RemovedSelectionPanel", typeof(RectTransform), typeof(Image));
        removedSelectionRoot.transform.SetParent(rootPanel, false);
        RectTransform selectionRect = removedSelectionRoot.GetComponent<RectTransform>();
        selectionRect.anchorMin = new Vector2(0.5f, 0.5f);
        selectionRect.anchorMax = new Vector2(0.5f, 0.5f);
        selectionRect.sizeDelta = new Vector2(420f, 440f);
        selectionRect.anchoredPosition = Vector2.zero;
        removedSelectionRoot.GetComponent<Image>().color = new Color(0.10f, 0.10f, 0.13f, 1f);

        selectionInfoRoot = new GameObject("SelectionInfo", typeof(RectTransform), typeof(Image));
        selectionInfoRoot.transform.SetParent(selectionRect, false);
        RectTransform infoRect = selectionInfoRoot.GetComponent<RectTransform>();
        infoRect.anchorMin = Vector2.zero;
        infoRect.anchorMax = Vector2.one;
        infoRect.offsetMin = new Vector2(25f, 55f);
        infoRect.offsetMax = new Vector2(-25f, -70f);
        selectionInfoRoot.GetComponent<Image>().color = new Color(0.21f, 0.21f, 0.21f, 0.75f);

        selectionCloseButton = CreateTextButton(selectionRect, "SelectionCloseButton", new Vector2(120f, 42f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), "Close", 22, new Color(0.30f, 0.30f, 0.30f, 1f));
        selectionCloseButton.onClick.AddListener(CloseRemovedSelection);

        removedSelectionRoot.SetActive(false);
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

        menuRoot.SetActive(false);
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
        removedSelectionRoot.SetActive(false);
        returnSelectionOpen = false;
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
