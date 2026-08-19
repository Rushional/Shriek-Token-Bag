using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TokenBagUIController : MonoBehaviour
{
    private readonly KillerBagState gameState = new KillerBagState();

    [SerializeField] private GameObject gameplayRoot;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private GameObject confirmationPrompt;
    [SerializeField] private GameObject restoreKillerPanel;

    [SerializeField] private Button menuButton;
    [SerializeField] private Button drawTokenButton;
    [SerializeField] private Button putBackButton;
    [SerializeField] private Button removeButton;

    [SerializeField] private TextMeshProUGUI currentKillerText;
    [SerializeField] private TextMeshProUGUI removedKillersText;

    [SerializeField] private Button resetBagButton;
    [SerializeField] private Button returnFromRemovedButton;
    [SerializeField] private Button menuCloseButton;
    [SerializeField] private Button confirmationYesButton;
    [SerializeField] private Button confirmationNoButton;
    [SerializeField] private Button closeKillerOptionsButton;
    [SerializeField] private GameObject killerOptionsPanel;
    [SerializeField] private GameObject killerNameButtonPrefab;
    private List<Button> restoreKillerButtonsList = new List<Button>();
    [SerializeField] private GameObject noRemovedKillersPanel;
    [SerializeField] private Button noRemovedKillersButton;
    [SerializeField] private TextMeshProUGUI noRemovedKillersText;

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

    // TODO: move code in this method into 2 new method: AddButtonListeners() and SetupPanels()
    private void BindReferences()
    {
        if (menuButton != null) menuButton.onClick.AddListener(OnMenuButtonClicked);
        if (drawTokenButton != null) drawTokenButton.onClick.AddListener(OnDrawTokenClicked);
        if (putBackButton != null) putBackButton.onClick.AddListener(OnPutBackClicked);
        if (removeButton != null) removeButton.onClick.AddListener(OnRemoveClicked);
        if (resetBagButton != null) resetBagButton.onClick.AddListener(OnResetBagClicked);
        if (returnFromRemovedButton != null) returnFromRemovedButton.onClick.AddListener(OnReturnFromRemovedClicked);
        if (menuCloseButton != null) menuCloseButton.onClick.AddListener(CloseMenu);
        if (confirmationYesButton != null) confirmationYesButton.onClick.AddListener(OnResetConfirmed);
        if (confirmationNoButton != null) confirmationNoButton.onClick.AddListener(OnConfirmationCancelled);
        if (closeKillerOptionsButton != null) closeKillerOptionsButton.onClick.AddListener(CloseRemovedSelection);

        // Ensure panels are deactivated by default
        if (menuPanel != null) menuPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (restoreKillerPanel != null) restoreKillerPanel.SetActive(false);

        // Gameplay root active
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
    }


    // TODO: Remove activating gameplayRoot. Rename to CloseAllMenus. Start actually using this.  
    private void CloseAllPanels()
    {
        menuPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        restoreKillerPanel.SetActive(false);
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
        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel (menuPanel) not found. Ensure the UI prefabs were created and TokenBagRoot is present in the scene.");
            menuOpen = false;
            return;
        }

        menuPanel.SetActive(true);
        SetGameplayControlsActive(false);
    }

    private void CloseMenu()
    {
        menuOpen = false;
        confirmationOpen = false;
        menuPanel.SetActive(false);
        confirmationPanel.SetActive(false);
        SetGameplayControlsActive(true);
        RefreshGameUi();
    }

    private void OnResetBagClicked()
    {
        if (!menuOpen)
        {
            return;
        }

        menuPanel.SetActive(false);
        confirmationPanel.SetActive(true);
        confirmationOpen = true;
        SetGameplayControlsActive(false);
    }

    private void OnConfirmationCancelled()
    {
        confirmationPanel.SetActive(false);
        confirmationOpen = false;
        menuPanel.SetActive(true);
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

        menuPanel.SetActive(false);
        menuOpen = false;
        BuildReturnSelectionList();
        restoreKillerPanel.SetActive(true);
        returnSelectionOpen = true;
        SetGameplayControlsActive(false);
    }

    private void ShowNothingToReturnDialog()
    {
        menuPanel.SetActive(false);
        confirmationPanel.SetActive(true);
        confirmationOpen = true;
        SetGameplayControlsActive(false);

        var prompt = confirmationPanel.transform.Find("ConfirmationPrompt").GetComponent<TextMeshProUGUI>();
        // This is wrong. We need to make a new way handle this case. A separate Text or even a panel?
        prompt.text = "There is nothing to return.";
        confirmationYesButton.gameObject.SetActive(false);
        confirmationNoButton.gameObject.SetActive(false);

        // Wtf is this? Move this to the prefab probably?
        var closePromptButton = CreateKillerNameButton("Copilot");

        // TODO: probably move this to a separate method. Also replace it with the noRemovedKillersButton. And the others with similarly named elements
        closePromptButton.onClick.AddListener(() =>
        {
            confirmationOpen = false;
            confirmationPanel.SetActive(false);
            confirmationYesButton.gameObject.SetActive(true);
            confirmationNoButton.gameObject.SetActive(true);
            Destroy(closePromptButton.gameObject);
            menuPanel.SetActive(true);
            menuOpen = true;
            SetGameplayControlsActive(false);
        });
    }

    private void BuildReturnSelectionList()
    {
        foreach (Killer killer in gameState.removedKillersList)
        {
            Button restoreKillerButton = CreateKillerNameButton(KillerBagState.GetDisplayName(killer));
            restoreKillerButton.onClick.AddListener(() => OnReturnKillerClicked(killer));
            restoreKillerButtonsList.Add(restoreKillerButton);
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
        foreach (Button button in restoreKillerButtonsList)
        {
            Destroy(button.gameObject);
        }
        restoreKillerButtonsList.Clear();

        // TODO: delete all buttons inside KillerOptionsPanel here to prevent them from piling up
        RefreshGameUi();
        CloseRemovedSelection();
        menuOpen = false;
        menuPanel.SetActive(false);
        SetGameplayControlsActive(true);
    }

    private void CloseRemovedSelection()
    {
        if (restoreKillerPanel != null)
            restoreKillerPanel.SetActive(false);

        returnSelectionOpen = false;

        // Return to menu if it was the source of the selection
        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
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

    private Button CreateKillerNameButton(string killerName)
    {
        var buttonObject = Instantiate(killerNameButtonPrefab);
        buttonObject.transform.SetParent(killerOptionsPanel.transform, false);

        var killerNameButton = buttonObject.GetComponent<Button>();
        var textObject = buttonObject.GetComponentInChildren<TextMeshProUGUI>();

        textObject.text = killerName;

        return killerNameButton;
    }
}
