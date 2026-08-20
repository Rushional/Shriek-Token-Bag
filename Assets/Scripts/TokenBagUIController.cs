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
    [SerializeField] private Image killerImage;
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


    [SerializeField] private Sprite bigBadWolfSprite;
    [SerializeField] private Sprite hansSprite;
    [SerializeField] private Sprite drFrightSprite;
    [SerializeField] private Sprite geppettoSprite;
    [SerializeField] private Sprite ratchetLadySprite;
    [SerializeField] private Sprite bagheadSprite;
    [SerializeField] private Sprite hunterSprite;
    [SerializeField] private Sprite razorfaceSprite;
    [SerializeField] private Sprite tormentorSprite;
    [SerializeField] private Sprite krampusSprite;
    private Dictionary<Killer, Sprite> killerSprites;

    private void Awake()
    {
        Setup();
        gameState.Reset();
    }

    private void Setup()
    {
        FillKillerSpritesDictionary();
        AddButtonListeners();
        SetupPanelsGameStart();
    }

    private void FillKillerSpritesDictionary()
    {
        killerSprites = new Dictionary<Killer, Sprite>
        {
            { Killer.BigBadWolf, bigBadWolfSprite },
            { Killer.Hans, hansSprite },
            { Killer.DrFright, drFrightSprite },
            { Killer.Geppetto, geppettoSprite },
            { Killer.RatchetLady, ratchetLadySprite },
            { Killer.Baghead, bagheadSprite },
            { Killer.HUNTER, hunterSprite },
            { Killer.Razorface, razorfaceSprite },
            { Killer.Tormentor, tormentorSprite },
            { Killer.Krampus, krampusSprite }
        };
    }

    private void AddButtonListeners()
    {
        menuButton.onClick.AddListener(OpenMenu);
        menuCloseButton.onClick.AddListener(CloseMenuPressed);
        drawTokenButton.onClick.AddListener(DrawKillerToken);
        putBackButton.onClick.AddListener(PutKillerBack);
        removeButton.onClick.AddListener(RemoveKiller);
        resetBagButton.onClick.AddListener(OpenResetConfirmation);
        confirmationYesButton.onClick.AddListener(ResetBag);
        confirmationNoButton.onClick.AddListener(CancelReset);
        restoreKillerButton.onClick.AddListener(OpenRestoreKillerPanel);
        closeKillerOptionsButton.onClick.AddListener(CloseRestoreSelection);
        noRemovedKillersButton.onClick.AddListener(CloseNoKillersInfo);
    }

    private void SetupPanelsGameStart()
    {
        CloseMenu();
        CloseResetConfirmationPanel();
        restoreKillerPanel.SetActive(false);

        gameplayRoot.SetActive(true);

        ResetTokenButtons();

        CleanupCurrentKiller();
        removedKillersText.text = string.Empty;
        removedKillersText.gameObject.SetActive(false);
    }

    private void ResetTokenButtons()
    {
        drawTokenButton.gameObject.SetActive(true);
        putBackButton.gameObject.SetActive(false);
        removeButton.gameObject.SetActive(false);
    }

    private void CleanupCurrentKiller()
    {
        currentKillerText.text = string.Empty;
        currentKillerText.gameObject.SetActive(false);
        killerImage.gameObject.SetActive(false);
    }

    private void SetGameplayControlsActive(bool isActive)
    {
        gameplayRoot.SetActive(isActive);
    }

    private void OpenMenu()
    {
        menuPanel.SetActive(true);
        SetGameplayControlsActive(false);
    }

    private void CloseMenuPressed()
    {
        CloseMenu();
        SetGameplayControlsActive(true);
    }

    private void CloseMenu()
    {
        menuPanel.SetActive(false);
    }

    private void OpenResetConfirmation()
    {
        CloseMenu();
        confirmationPanel.SetActive(true);
    }

    private void CancelReset()
    {
        CloseResetConfirmationPanel();
        menuPanel.SetActive(true);
    }

    private void ResetBag()
    {
        CloseResetConfirmationPanel();
        gameState.Reset();
        SetupPanelsGameStart();
    }

    private void CloseResetConfirmationPanel()
    {
        confirmationPanel.SetActive(false);
    }

    private void OpenRestoreKillerPanel()
    {
        CloseMenu();
        if (!gameState.HasRemovedKillers)
        {
            noRemovedKillersPanel.SetActive(true);
            return;
        }
        
        BuildReturnSelectionList();
        restoreKillerPanel.SetActive(true);
    }

    private void CloseNoKillersInfo()
    {
        noRemovedKillersPanel.SetActive(false);
        SetGameplayControlsActive(true);
    }

    private void BuildReturnSelectionList()
    {
        foreach (Killer killer in gameState.GetRemovedKillersList())
        {
            Button restoreKillerButton = CreateKillerNameButton(KillerBagState.GetDisplayName(killer));
            restoreKillerButton.onClick.AddListener(() => RestoreKiller(killer));
            restoreKillerButtonsList.Add(restoreKillerButton);
        }
    }

    private void RestoreKiller(Killer killer)
    {
        gameState.RestoreRemovedKiller(killer);
        ClearRestoreKillerButtons();
        RefreshRemovedKillers();
        CloseRestoreSelection();
        menuPanel.SetActive(false);
        SetGameplayControlsActive(true);
    }

    private void RefreshRemovedKillers()
    {
        removedKillersText.gameObject.SetActive(gameState.HasRemovedKillers);
        removedKillersText.text = CreateRemovedKillersString(gameState.GetRemovedKillersList());
    }

    public string CreateRemovedKillersString(List<Killer> removedKillersList)
    {
        if (removedKillersList.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, removedKillersList.Select(KillerBagState.GetDisplayName));
    }

    private void CloseRestoreSelection()
    {
        restoreKillerPanel.SetActive(false);
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
        ShowCurrentKiller();

        drawTokenButton.gameObject.SetActive(false);
        putBackButton.gameObject.SetActive(true);
        removeButton.gameObject.SetActive(true);
    }

    private void ShowCurrentKiller()
    {
        currentKillerText.text = gameState.GetCurrentDisplayName();
        currentKillerText.gameObject.SetActive(true);
        killerImage.sprite = killerSprites[gameState.GetCurrentKiller().Value];
        killerImage.gameObject.SetActive(true);
    }

    private void PutKillerBack()
    {
        gameState.PutBackCurrentKiller();
        CleanupCurrentKiller();
        ResetTokenButtons();
    }

    private void RemoveKiller()
    {
        gameState.RemoveCurrentKiller();
        CleanupCurrentKiller();
        RefreshRemovedKillers();
        ResetTokenButtons();
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
