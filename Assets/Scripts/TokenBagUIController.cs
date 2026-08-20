using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
// TODO: this does nothing, right? Remove?
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
    [SerializeField] private Button restoreKillerButton;
    [SerializeField] private Button menuCloseButton;
    [SerializeField] private Button confirmationYesButton;
    [SerializeField] private Button confirmationNoButton;
    [SerializeField] private Button closeKillerOptionsButton;
    [SerializeField] private GameObject killerOptionsPanel;
    [SerializeField] private GameObject killerNameButtonPrefab;
    private List<Button> restoreKillerButtonsList = new List<Button>();
    [SerializeField] private GameObject noRemovedKillersPanel;
    [SerializeField] private Button noRemovedKillersButton;

    private bool menuOpen;
    private bool resetConfirmationOpen;
    private bool returnSelectionOpen;

    // At runtime, bind to UI GameObjects created in the Editor.
    // The Editor bootstrap will create the UI once; TokenBagUIController uses those objects at runtime.
    private void Awake()
    {
        Setup();
        gameState.Reset();
        RefreshGameUi();
    }

    // TODO: move code in this method into 2 new methods: AddButtonListeners() and SetupPanels()
    private void Setup()
    {
        // TODO: remove if nulls? 
        if (menuButton != null) menuButton.onClick.AddListener(OpenMenu);
        if (drawTokenButton != null) drawTokenButton.onClick.AddListener(DrawKillerToken);
        if (putBackButton != null) putBackButton.onClick.AddListener(PutKillerBack);
        if (removeButton != null) removeButton.onClick.AddListener(RemoveKiller);
        if (resetBagButton != null) resetBagButton.onClick.AddListener(OpenResetConfirmation);
        if (restoreKillerButton != null) restoreKillerButton.onClick.AddListener(RestoreKiller);
        if (menuCloseButton != null) menuCloseButton.onClick.AddListener(CloseResetConfirmation);
        if (confirmationYesButton != null) confirmationYesButton.onClick.AddListener(ResetBag);
        if (confirmationNoButton != null) confirmationNoButton.onClick.AddListener(CancelReset);
        if (closeKillerOptionsButton != null) closeKillerOptionsButton.onClick.AddListener(CloseRestoreSelection);
        noRemovedKillersButton.onClick.AddListener(CloseNoKillersInfo);

        if (menuPanel != null) menuPanel.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
        if (restoreKillerPanel != null) restoreKillerPanel.SetActive(false);
        if (gameplayRoot != null) gameplayRoot.SetActive(true);
    }

    private void SetGameplayControlsActive(bool isActive)
    {
        if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(isActive);
        }
    }

    private void OpenMenu()
    {
        if (menuOpen || resetConfirmationOpen || returnSelectionOpen)
        {
            return;
        }

        menuPanel.SetActive(true);
        menuOpen = true;
        SetGameplayControlsActive(false);
    }

    private void CloseResetConfirmation()
    {
        // TODO: this looks wrong. We're closing the confirmation window, shouldn't affect menu. 
        menuPanel.SetActive(false);
        menuOpen = false;
        confirmationPanel.SetActive(false);
        resetConfirmationOpen = false;
        SetGameplayControlsActive(true);
        RefreshGameUi();
    }

    private void OpenResetConfirmation()
    {
        if (!menuOpen)
        {
            return;
        }

        menuPanel.SetActive(false);
        confirmationPanel.SetActive(true);
        resetConfirmationOpen = true;
        // SetGameplayControlsActive(false);
    }

    private void CancelReset()
    {
        confirmationPanel.SetActive(false);
        resetConfirmationOpen = false;
        menuPanel.SetActive(true);
        menuOpen = true;
        // SetGameplayControlsActive(false);
    }

    private void ResetBag()
    {
        gameState.Reset();
        CloseResetConfirmation();
        RefreshGameUi();
    }

    private void RestoreKiller()
    {
        menuPanel.SetActive(false);
        menuOpen = false;
        if (gameState.removedKillersList.Count == 0)
        {
            ShowNothingToReturnDialog();
            return;
        }
        
        BuildReturnSelectionList();
        restoreKillerPanel.SetActive(true);
        returnSelectionOpen = true;
        SetGameplayControlsActive(false);
    }

    private void ShowNothingToReturnDialog()
    {
        menuPanel.SetActive(false);
        noRemovedKillersPanel.SetActive(true);
        SetGameplayControlsActive(false);
    }

    private void CloseNoKillersInfo()
    {
        noRemovedKillersPanel.SetActive(false);
        SetGameplayControlsActive(true);
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
            Debug.LogError("Attempt to remove a killer that wasn't in the removedKillersList. Shouldn't be possible.");
            return;
        }
        gameState.removedKillersList.Remove(killer);
        gameState.bagKillersList.Add(killer);
        ClearRestoreKillerButtons();
        RefreshGameUi();
        CloseRestoreSelection();
        menuOpen = false;
        menuPanel.SetActive(false);
        SetGameplayControlsActive(true);
    }

    private void CloseRestoreSelection()
    {
        if (restoreKillerPanel != null)
            restoreKillerPanel.SetActive(false);
        returnSelectionOpen = false;
        ClearRestoreKillerButtons();
        SetGameplayControlsActive(true);
    }

    private void ClearRestoreKillerButtons()
    {
        foreach (Button button in restoreKillerButtonsList)
        {
            Destroy(button.gameObject);
        }
        restoreKillerButtonsList.Clear();
    }

    private void DrawKillerToken()
    {
        if (!gameState.CanDrawToken)
        {
            return;
        }

        gameState.DrawRandomKiller();
        RefreshGameUi();
    }

    private void PutKillerBack()
    {
        if (!gameState.currentKiller.HasValue)
        {
            return;
        }

        gameState.PutBackCurrentKiller();
        RefreshGameUi();
    }

    private void RemoveKiller()
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

        if (!menuOpen && !resetConfirmationOpen && !returnSelectionOpen)
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
