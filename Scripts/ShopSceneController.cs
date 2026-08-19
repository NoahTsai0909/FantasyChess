using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RunManager;

public class ShopSceneController : MonoBehaviour
{
    [Header("UI & Anchors")]
    [SerializeField] private ShopUnitCard shopUnitCardPrefab;
    [SerializeField] private Transform shopUIAnchor; // This should be your HorizontalLayoutGroup Panel
    [SerializeField] private Transform hiddenUnitAnchor; // An empty GameObject to hold the invisible dummy units

    [Header("Buttons & Text")]
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button prepSceneButton;
    [SerializeField] private Button continueButton;

    [Header("Player Boards")]
    [SerializeField] public GridManager battleGrid;
    [SerializeField] public GridManager benchGrid;
    [SerializeField] private TacticBarManager playerTacticBarManager;

    private ShopEventSO shopEvent;
    private ShopState shopState;

    private List<ShopUnitCard> spawnedCards = new();
    private List<UnitInstance> spawnedDummies = new();
    private List<TacticInstance> spawnedDummyTactics = new();

    void Awake()
    {
        if (RunHUDManager.Instance != null)
        {
            RunHUDManager.Instance.ResetAndShow();
        }
    }

    void Start()
    {
        shopEvent = RunManager.Instance.selectedEvent as ShopEventSO;
        if (shopEvent == null)
        {
            Debug.LogError("ShopScene loaded without ShopEventSO");
            return;
        }
        RunManager.Instance.InitializeShop(
            shopEvent.totalUnitsGenerated,
            shopEvent.totalTacticsGenerated,
            RunManager.Instance.playerRegion,
            shopEvent.allowedTags,
            shopEvent.minProvisionCost,
            shopEvent.maxProvisionCost,
            shopEvent.forceRarity,
            shopEvent.designatedRarity
        );

        shopState = RunManager.Instance.shopState;

        continueButton.onClick.AddListener(() => CompleteEventAndReturn(shopEvent));

        SetupRefreshButton();
        SetupPrepSceneButton();
        DisplayCurrentPage();

        LoadBattleGridFromRunManager();
        LoadBenchGridFromRunManager();
        LoadTacticBarFromRunManager();

        if (playerTacticBarManager != null)
        {
            playerTacticBarManager.RefreshAllTacticAuras();
        }
        if (battleGrid != null) battleGrid.RefreshAllAuras();
    }

    void SetupRefreshButton()
    {
        refreshButton.gameObject.SetActive(!shopState.hasRefreshed);

        var buttonText = refreshButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null) buttonText.text = $"Refresh ({shopEvent.refreshCost}g)";

        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(() =>
        {
            if (RunManager.Instance.Stats.CurrentGold < shopEvent.refreshCost)
            {
                UniversalPopupManager.ShowPopup($"Not enough [GOLD]");
                return;
            }

            RunManager.Instance.Stats.CurrentGold -= shopEvent.refreshCost;
            shopState.hasRefreshed = true;
            shopState.currentPage = 1;
            refreshButton.gameObject.SetActive(false);
            DisplayCurrentPage();
        });
    }

    void SetupPrepSceneButton()
    {
        prepSceneButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene(SceneLoader.GameScene.PrepScene));
    }

    void DisplayCurrentPage()
    {
        ClearSpawnedUnits();
        if (shopState.offeredUnits != null && shopState.offeredUnits.Count > 0)
        {
            var pageUnits = shopState.offeredUnits
                .Skip(shopState.currentPage * shopEvent.unitsPerPage)
                .Take(shopEvent.unitsPerPage)
                .ToList();

            for (int i = 0; i < pageUnits.Count; i++)
            {
                SpawnShopCard(pageUnits[i]);
            }
        }

        if (shopState.offeredTactics != null && shopState.offeredTactics.Count > 0)
        {
            var pageTactics = shopState.offeredTactics
                .Skip(shopState.currentPage * shopEvent.unitsPerPage)
                .Take(shopEvent.unitsPerPage)
                .ToList();

            for (int i = 0; i < pageTactics.Count; i++)
            {
                SpawnShopTacticCard(pageTactics[i]);
            }
        }
    }

    void SpawnShopCard(UnitSaveData unitData)
    {
        ShopUnitCard card = Instantiate(shopUnitCardPrefab, shopUIAnchor);

        if (shopState.purchasedUnits.Contains(unitData.definition))
        {
            card.MarkAsPurchased();
            spawnedCards.Add(card);
            return;
        }

        UnitInstance dummyUnit = Instantiate(unitData.definition.unitPrefab, hiddenUnitAnchor);
        dummyUnit.InitializeFromSaveData(unitData);
        dummyUnit.isPlayer = true;
        dummyUnit.gameObject.SetActive(false);
        spawnedDummies.Add(dummyUnit);

        int unitCost = GetPurchasePrice(unitData);

        card.Initialize(dummyUnit, unitCost, () =>
        {
            if (RunManager.Instance.Stats.CurrentGold < unitCost)
            {
                UniversalPopupManager.ShowPopup($"Not enough [GOLD]");
                return;
            }
            SaveCurrentBoardsToRunManager();
            RunManager.Instance.Stats.CurrentGold -= unitCost;

            PlayerUnitManager.Instance.TryAcquireUnit(unitData.definition, unitData.rarity);

            shopState.purchasedUnits.Add(unitData.definition);

            spawnedDummies.Remove(dummyUnit);
            Destroy(dummyUnit.gameObject);

            card.MarkAsPurchased();

            LoadBattleGridFromRunManager();
            LoadBenchGridFromRunManager();
            LoadTacticBarFromRunManager();
            if (benchGrid != null) benchGrid.RefreshAllAuras();
            if (battleGrid != null) battleGrid.RefreshAllAuras();
            if (playerTacticBarManager != null) playerTacticBarManager.RefreshAllTacticAuras();
        });

        spawnedCards.Add(card);
    }

    void SpawnShopTacticCard(RunManager.TacticSaveData tacticData)
    {
        ShopUnitCard card = Instantiate(shopUnitCardPrefab, shopUIAnchor);

        if (shopState.purchasedTactics.Contains(tacticData.definition))
        {
            card.MarkAsPurchased();
            spawnedCards.Add(card);
            return;
        }

        // --- NEW: Spawn the invisible Dummy Tactic! ---
        TacticInstance dummyTactic = Instantiate(tacticData.definition.tacticPrefab, hiddenUnitAnchor);
        dummyTactic.InitializeFromSaveData(tacticData);
        dummyTactic.gameObject.SetActive(false);
        spawnedDummyTactics.Add(dummyTactic);

        int tacticCost = GetTacticPurchasePrice(tacticData);

        // Pass the dummyTactic instead of the definition
        card.InitializeTactic(dummyTactic, tacticCost, () =>
        {
            if (RunManager.Instance.Stats.CurrentGold < tacticCost)
            {
                UniversalPopupManager.ShowPopup($"Not enough [GOLD]");
                return;
            }
            SaveCurrentBoardsToRunManager();
            RunManager.Instance.Stats.CurrentGold -= tacticCost;

            PlayerTacticManager.Instance.TryAcquireTactic(tacticData.definition, tacticData.rarity);
            shopState.purchasedTactics.Add(tacticData.definition);

            // Clean up the dummy!
            spawnedDummyTactics.Remove(dummyTactic);
            Destroy(dummyTactic.gameObject);

            card.MarkAsPurchased();

            LoadTacticBarFromRunManager();
            if (playerTacticBarManager != null) playerTacticBarManager.RefreshAllTacticAuras();
            if (battleGrid != null) battleGrid.RefreshAllAuras();
            if (benchGrid != null) benchGrid.RefreshAllAuras();
        });

        spawnedCards.Add(card);
    }



    int GetPurchasePrice(UnitSaveData unit)
    {
        int discountValue = 0;
        if (shopEvent.discount)
        {
            discountValue = RarityToMultiplier(unit.rarity);
        }
        return (unit.EffectiveValue * 2) - discountValue;
    }

    int GetTacticPurchasePrice(RunManager.TacticSaveData tactic)
    {
        int discountValue = 0;
        if (shopEvent.discount)
        {
            discountValue = RarityToMultiplier(tactic.rarity);
        }
        return (RarityToMultiplier(tactic.rarity) * 5) - discountValue;
    }

    public static int RarityToMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1;
            case Rarity.Uncommon: return 2;
            case Rarity.Rare: return 3;
            case Rarity.Epic: return 4;
            default: return 1;
        }
    }

    void ClearSpawnedUnits()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        spawnedCards.Clear();

        foreach (var dummy in spawnedDummies)
        {
            if (dummy != null) Destroy(dummy.gameObject);
        }
        spawnedDummies.Clear();
        foreach (var dummyTactic in spawnedDummyTactics)
        {
            if (dummyTactic != null) Destroy(dummyTactic.gameObject);
        }
        spawnedDummyTactics.Clear();
    }

    private void CompleteEventAndReturn(BaseEventSO eventSO)
    {
        SaveCurrentBoardsToRunManager();
        eventSO.OnCompleted();
        SceneLoader.Instance.LoadScene(SceneLoader.GameScene.MapScene);
    }

    private void LoadBattleGridFromRunManager()
    {
        if (battleGrid == null) return;

        battleGrid.ClearAllUnits();
        foreach (var placement in RunManager.Instance.playerTeamPlacements)
        {
            if (placement.unitData == null || placement.unitData.definition == null) continue;

            UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
            unit.InitializeFromSaveData(placement.unitData);
            unit.myPlacement = placement;

            // Spawn them, but tell them NOT to start combat (isPlayer = true, startCombat = false)
            unit.EnterCombat(battleGrid, placement.row, placement.col, true, false);
        }
    }

    private void LoadBenchGridFromRunManager()
    {
        if (benchGrid == null) return;

        benchGrid.ClearAllUnits();
        for (int i = 0; i < RunManager.Instance.playerBenchPlacements.Count; i++)
        {
            var placement = RunManager.Instance.playerBenchPlacements[i];
            if (placement.unitData == null || placement.unitData.definition == null) continue;

            UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
            unit.InitializeFromSaveData(placement.unitData);
            unit.myPlacement = placement;
            unit.myPlacement.row = 0;
            unit.myPlacement.col = i;
            unit.EnterCombat(benchGrid, 0, i, true, false);
        }
    }

    private void LoadTacticBarFromRunManager()
    {
        if (playerTacticBarManager == null) return;

        playerTacticBarManager.ClearAllTactics();

        // Ensure tactics are spawned in their correct saved order
        var sortedTactics = RunManager.Instance.playerTactics.OrderBy(t => t.orderIndex).ToList();

        foreach (var placement in sortedTactics)
        {
            if (placement.tacticData == null || placement.tacticData.definition == null) continue;

            TacticInstance tactic = Instantiate(placement.tacticData.definition.tacticPrefab);
            tactic.InitializeFromSaveData(placement.tacticData);
            tactic.myPlacement = placement;
            
            playerTacticBarManager.AddTactic(tactic);
        }
        playerTacticBarManager.RefreshAllTacticAuras();

    }

    private void SaveCurrentBoardsToRunManager()
    {
        // 1. Save Battle Grid
        if (battleGrid != null)
        {
            RunManager.Instance.playerTeamPlacements.Clear();
            foreach (var unit in battleGrid.GetAllUnits())
            {
                if (unit != null && unit.myPlacement != null)
                {
                    RunManager.Instance.playerTeamPlacements.Add(unit.myPlacement);
                }
            }
        }

        // 2. Save Bench Grid
        if (benchGrid != null)
        {
            // Dynamically fetch your bench size to safely fill the empty slots
            int currentBenchSize = RunManager.Instance.playerBenchPlacements.Count;
            RunManager.Instance.playerBenchPlacements.Clear();

            for (int i = 0; i < currentBenchSize; i++)
            {
                RunManager.Instance.playerBenchPlacements.Add(new RunManager.UnitPlacement());
            }

            // Slot in the active units based on their physical column
            foreach (var unit in benchGrid.GetAllUnits())
            {
                if (unit != null && unit.myPlacement != null)
                {
                    int col = unit.myPlacement.col;
                    if (col >= 0 && col < currentBenchSize)
                    {
                        RunManager.Instance.playerBenchPlacements[col] = unit.myPlacement;
                    }
                }
            }
            RunManager.Instance.SanitizeBench();
        }

        // 3. Save Tactic Bar
        if (playerTacticBarManager != null)
        {
            RunManager.Instance.playerTactics.Clear();
            var tactics = playerTacticBarManager.GetAllTactics();
            for (int i = 0; i < tactics.Count; i++)
            {
                if (tactics[i] != null && tactics[i].myPlacement != null)
                {
                    tactics[i].myPlacement.orderIndex = i;
                    RunManager.Instance.playerTactics.Add(tactics[i].myPlacement);
                }
            }
        }
    }
}
