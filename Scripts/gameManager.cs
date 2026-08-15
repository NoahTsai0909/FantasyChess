using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static CombatEventBus;
using static SceneLoader;
using static UnityEngine.Rendering.DebugUI.Table;
using System;

public class gameManager : MonoBehaviour
{

    [Header("Grids")]
    public GridManager playerGrid;
    public GridManager enemyGrid;
    public GridManager benchGrid;
    [Header("Tactic Bars")]
    public TacticBarManager playerTacticBarManager;
    public TacticBarManager enemyTacticBarManager;

    [Header("UI Manager")]
    [SerializeField] private BattleUIManager battleUIManager;
    [SerializeField] private Button inspectStatsButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject unitStatsWindowObject;
    [SerializeField] private CombatResultUI combatResultUI;

    [Header("UI & Drag Managers")]
    [SerializeField] private DragAndDropManager dragManager; 
    [SerializeField] private ProvisionManager provisionManager; 
    [SerializeField] private Button startCombatButton;
    [SerializeField] private SellZone sellZone;

    [Header("Combat Settings")]
    [SerializeField] private float endCombatDelay = 1.0f;

    [Header("Disaster System")]
    [SerializeField] private DisasterManager disasterManager;

    private bool combatActive = true;
    public bool isCombatActive() => combatActive;

    private UnitInstance unitReward;

    void Start()
    {
        if (inspectStatsButton != null) inspectStatsButton.gameObject.SetActive(true);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (unitStatsWindowObject != null) unitStatsWindowObject.SetActive(false);

        if (inspectStatsButton != null)
        {
            inspectStatsButton.onClick.AddListener(() =>
            {
                if (unitStatsWindowObject != null) unitStatsWindowObject.SetActive(true);
            });
        }

        if (startCombatButton != null)
        {
            startCombatButton.onClick.AddListener(() =>
            {
                StartActualCombat();
            });
        }

        combatActive = false;
        TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
        EncounterDefinition currentEncounter = RunManager.Instance.currentEncounter;
        if (playerTeam != null && currentEncounter != null)
        {
            InitializeBattlefield(playerTeam, currentEncounter, false);
            InitializeBench();
            InitializeTacticsBar();
            InitializeEnemyTacticsBar(currentEncounter);
        }
        if (!HasLivingUnits(playerGrid))
        {
            CheckCombatEnd();
        }
        if (battleUIManager != null)
        {
            battleUIManager.Initialize(playerGrid, enemyGrid);
        }
        CombatEventBus.OnCombatEvent += OnCombatEvent;
        if (RunHUDManager.Instance != null)
        {
            RunHUDManager.Instance.SlideOutAndHide(0.5f);
        }
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= OnCombatEvent;
    }

    private void OnCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (type == CombatEventBus.CombatEventType.UnitDied && combatActive)
        {
            playerGrid.RefreshAllAuras();
            enemyGrid.RefreshAllAuras();
            CheckCombatEnd();
        }
    }

    private void CheckCombatEnd()
    {
        if (!combatActive) return;

        bool playerHasUnits = HasLivingUnits(playerGrid);
        bool enemyHasUnits = HasLivingUnits(enemyGrid);

        if (!playerHasUnits && !enemyHasUnits)
        {
            // Draw - both teams dead
            EndCombat(true, false);
        }
        else if (!playerHasUnits)
        {
            // Player lost
            EndCombat(false, true);
        }
        else if (!enemyHasUnits)
        {
            // Player won
            EndCombat(true, false);
        }
    }

    private bool HasLivingUnits(GridManager grid)
    {
        // Check if grid has any non-null units (alive)
        List<UnitInstance> units = grid.GetAllUnits();
        return units.Count > 0;
    }

    private void EndCombat(bool playerWon, bool isDraw)
    {
        CombatEventBus.PublishCombatEnd();
        if (playerTacticBarManager != null) playerTacticBarManager.StopCombat();
        if (enemyTacticBarManager != null) enemyTacticBarManager.StopCombat();
        combatActive = false;

        foreach (var unit in playerGrid.GetAllUnits())
        {
            if (unit != null) unit.inCombat = false;
        }
        foreach (var unit in enemyGrid.GetAllUnits())
        {
            if (unit != null) unit.inCombat = false;
        }

        TransferCombatStatsToRunManager();

        if (disasterManager != null)
            disasterManager.StopDisaster();

        Time.timeScale = 0.5f;

        // Apply rewards only if player won
        if (playerWon && RunManager.Instance.selectedEvent != null)
        {
            // Get the combat event that started this battle
            var combatEvent = RunManager.Instance.selectedEvent as CombatEventSO;
            if (combatEvent != null)
            {
                // Apply combat-specific rewards
                RunManager.Instance.Stats.CurrentGold += combatEvent.goldReward;
                RunManager.Instance.Stats.Experience += combatEvent.reputationReward;
                PlayerUnitManager.Instance.TryAcquireUnit(unitReward.Definition, unitReward.CurrentRarity);
            }
            // Mark the event as completed
            RunManager.Instance.selectedEvent.OnCompleted();
        }
        else if (!playerWon)
        {
            // Player lost - still mark event as completed but no rewards
            if (RunManager.Instance.selectedEvent != null)
                RunManager.Instance.selectedEvent.OnCompleted();
            RunManager.Instance.Stats.PlayerHealth -= RunManager.Instance.Stats.CurrentDay;
        }

        // Start coroutine to transition scene
        StartCoroutine(TransitionAfterDelay(playerWon));
    }

    private IEnumerator TransitionAfterDelay(bool playerWon)
    {
        yield return new WaitForSeconds(endCombatDelay);
        ResetBattlefieldToStasis();
        if (combatResultUI != null)
        {
            combatResultUI.ShowResult(playerWon);
        }
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);

            // Clear any old clicks and add the scene transition logic
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                Time.timeScale = 1f;

                if (playerWon || RunManager.Instance.Stats.PlayerHealth > 0)
                {
                    // Go to map scene to continue run
                    SceneLoader.Instance.LoadScene(GameScene.MapScene);
                }
                else
                {
                    SceneLoader.Instance.LoadScene(GameScene.RunSummaryScene);
                }
            });
        }
    }

    public void InitializeBattlefield(TeamDefinition playerTeam, EncounterDefinition encounter, bool startCombat = true)
    {
        foreach (var placement in playerTeam.units)
        {
            if (placement.unitData == null || placement.unitData.definition == null || placement.unitData.definition.unitPrefab == null)
                continue;

            SpawnPlayerUnit(placement, playerGrid, startCombat);
        }

        foreach (var enemyPlacement in encounter.enemyUnits)
        {
            SpawnEnemyUnit(enemyPlacement, enemyGrid, startCombat);
        }

        if (startCombat)
        {
            foreach (var playerUnit in playerGrid.GetAllUnits())
            {
                playerUnit.CombatStartEffect();
                playerGrid.RefreshAllAuras();
                if (playerTacticBarManager != null) playerTacticBarManager.RefreshAllTacticAuras();
            }
            foreach (var enemyUnit in enemyGrid.GetAllUnits())
            {
                enemyUnit.CombatStartEffect();
                enemyGrid.RefreshAllAuras();
                if (enemyTacticBarManager != null) enemyTacticBarManager.RefreshAllTacticAuras();
            }
        }
        unitReward = enemyGrid.GetRandomUnit();

    }


    private UnitInstance SpawnPlayerUnit(RunManager.UnitPlacement placement, GridManager grid, bool startCombat)
    {
        UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
        unit.InitializeFromSaveData(placement.unitData);
        unit.myPlacement = placement;
        unit.EnterCombat(grid, placement.row, placement.col, true, startCombat);

        return unit;
    }

    private UnitInstance SpawnEnemyUnit(RunManager.UnitPlacement placement, GridManager grid, bool startCombat)
    {
        UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
        unit.InitializeEnemy(placement.unitData.definition, placement.unitData.rarity);
        unit.EnterCombat(grid, placement.row, placement.col, false, startCombat);

        return unit;
    }

    private void ResetBattlefieldToStasis()
    {
        if (battleUIManager != null)
        {
            foreach (var unit in playerGrid.GetAllUnits())
            {
                battleUIManager.RemoveUnitUI(unit);
            }
            foreach (var unit in enemyGrid.GetAllUnits())
            {
                battleUIManager.RemoveUnitUI(unit);
            }
        }
        // Wipe both grids clean (this destroys the existing GameObjects)
        playerGrid.ClearAllUnits();
        enemyGrid.ClearAllUnits();

        // Get the teams again
        TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
        EncounterDefinition currentEncounter = RunManager.Instance.currentEncounter;

        // Respawn them, but pass "false" to tell them NOT to fight
        if (playerTeam != null && currentEncounter != null)
        {
            InitializeBattlefield(playerTeam, currentEncounter, false);
            InitializeTacticsBar();
            InitializeEnemyTacticsBar(currentEncounter);
        }
    }

    private void StartActualCombat()
    {
        if (provisionManager != null && !provisionManager.IsProvisionValid())
        {
            UniversalPopupManager.ShowPopup($"Provision exceeded provision cap!\nProvision cap: {RunManager.Instance.Stats.ProvisionCap}");
            return;
        }

        if (startCombatButton != null) startCombatButton.gameObject.SetActive(false);
        if (provisionManager != null) provisionManager.HideProvisionText();
        if (dragManager != null) dragManager.enabled = false;
        if (sellZone != null) sellZone.gameObject.SetActive(false);

        SaveFormationToRunManager();

        if (benchGrid != null)
        {
            StartCoroutine(SlideBenchOutRoutine(0.5f));
        }

        if (battleUIManager != null)
        {
            foreach (var unit in playerGrid.GetAllUnits()) battleUIManager.RemoveUnitUI(unit);
            foreach (var unit in enemyGrid.GetAllUnits()) battleUIManager.RemoveUnitUI(unit);
        }
        playerGrid.ClearAllUnits();
        enemyGrid.ClearAllUnits();

        combatActive = true;
        TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
        EncounterDefinition currentEncounter = RunManager.Instance.currentEncounter;

        if (playerTeam != null && currentEncounter != null)
        {
            InitializeBattlefield(playerTeam, currentEncounter, true);
        }

        // Pass "true" for the player, and "false" for the enemy!
        if (playerTacticBarManager != null) playerTacticBarManager.StartCombat(true);
        if (enemyTacticBarManager != null) enemyTacticBarManager.StartCombat(false);

        CheckCombatEnd();
    }

    private IEnumerator SlideBenchOutRoutine(float duration)
    {
        Transform benchTransform = benchGrid.transform;
        Vector3 originalBenchPos = benchTransform.position;
        Vector3 targetBenchPos = originalBenchPos + new Vector3(0, -10f, 0);

        List<UnitInstance> units = benchGrid.GetAllUnits();
        List<Vector3> originalUnitPos = new List<Vector3>();
        foreach (var u in units)
        {
            originalUnitPos.Add(u != null ? u.transform.position : Vector3.zero);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            benchTransform.position = Vector3.Lerp(originalBenchPos, targetBenchPos, t);

            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    units[i].transform.position = Vector3.Lerp(originalUnitPos[i], originalUnitPos[i] + new Vector3(0, -10f, 0), t);
                }
            }
            yield return null;
        }

        benchGrid.gameObject.SetActive(false);
        benchGrid.ClearAllUnits();
        benchTransform.position = originalBenchPos;
    }

    private void SaveFormationToRunManager()
    {
        if (RunManager.Instance == null) return;

        // Save Battle Grid
        List<RunManager.UnitPlacement> battleTeam = new List<RunManager.UnitPlacement>();
        foreach (UnitInstance unit in playerGrid.GetAllUnits())
        {
            if (unit.myPlacement != null)
            {
                unit.myPlacement.row = playerGrid.GetUnitPosition(unit).x;
                unit.myPlacement.col = playerGrid.GetUnitPosition(unit).y;
                battleTeam.Add(unit.myPlacement);
            }
        }
        RunManager.Instance.playerTeamPlacements = battleTeam;

        if (benchGrid != null)
        {
            for (int i = 0; i < RunManager.Instance.playerBenchPlacements.Count; i++)
            {
 
                UnitInstance unitInSlot = benchGrid.GetUnitAtPosition(0, i);

                if (unitInSlot != null && unitInSlot.myPlacement != null)
                {
                    RunManager.Instance.playerBenchPlacements[i].unitData = unitInSlot.myPlacement.unitData;
                }
                else
                {
                    RunManager.Instance.playerBenchPlacements[i].unitData = null;
                }
                RunManager.Instance.playerBenchPlacements[i].row = -1;
                RunManager.Instance.playerBenchPlacements[i].col = -1;
            }
        }

        if (playerTacticBarManager != null)
        {
            RunManager.Instance.playerTactics.Clear();
            var activeTactics = playerTacticBarManager.GetAllTactics();
            for (int i = 0; i < activeTactics.Count; i++)
            {
                if (activeTactics[i].myPlacement != null)
                {
                    activeTactics[i].myPlacement.orderIndex = i;
                    RunManager.Instance.playerTactics.Add(activeTactics[i].myPlacement);
                }
            }
        }
    }

    private void InitializeBench()
    {
        if (benchGrid == null) return;

        benchGrid.gameObject.SetActive(true);

        TeamDefinition benchTeam = RunManager.Instance.GetTeamForBench();
        if (benchTeam != null && benchTeam.units != null)
        {
            int col = 0;
            foreach (var placement in benchTeam.units)
            {
                if (placement.unitData != null && placement.unitData.definition != null && placement.unitData.definition.unitPrefab != null)
                {
                    UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);
                    unit.InitializeFromSaveData(placement.unitData);
                    unit.myPlacement = placement;
                    unit.EnterCombat(benchGrid, 0, col, true, false);
                }
                col++;
            }
        }
    }

    private void InitializeTacticsBar()
    {
        if (playerTacticBarManager == null) return;

        playerTacticBarManager.ClearAllTactics();

        // Ensure tactics are sorted by their saved orderIndex
        var sortedTactics = RunManager.Instance.playerTactics;
        sortedTactics.Sort((a, b) => a.orderIndex.CompareTo(b.orderIndex));

        foreach (var placement in sortedTactics)
        {
            if (placement.tacticData == null || placement.tacticData.definition == null) continue;

            // Spawn the tactic prefab
            TacticInstance tactic = Instantiate(placement.tacticData.definition.tacticPrefab);
            tactic.InitializeFromSaveData(placement.tacticData);
            tactic.myPlacement = placement;

            // Add it to the bar
            playerTacticBarManager.AddTactic(tactic);
        }
    }

    private void InitializeEnemyTacticsBar(EncounterDefinition encounter)
    {
        if (enemyTacticBarManager == null || encounter == null) return;

        enemyTacticBarManager.ClearAllTactics();

        // Assuming you add 'enemyTactics' to your EncounterDefinition
        if (encounter.enemyTactics != null)
        {
            foreach (var placement in encounter.enemyTactics)
            {
                if (placement.tacticData == null || placement.tacticData.definition == null) continue;

                TacticInstance tactic = Instantiate(placement.tacticData.definition.tacticPrefab);

                // You may want to add an InitializeEnemy method to TacticInstance later, 
                // but this works exactly like units for now!
                tactic.InitializeFromSaveData(placement.tacticData);
                tactic.myPlacement = placement;

                enemyTacticBarManager.AddTactic(tactic);
            }
        }
    }

    private void TransferCombatStatsToRunManager()
    {
        if (CombatStatsTracker.Instance == null || RunManager.Instance == null) return;

        // Retrieve the stats from the active combat scene tracker
        Dictionary<Guid, UnitCombatStats> currentFightStats = CombatStatsTracker.Instance.GetAllStats();

        foreach (var kvp in currentFightStats)
        {
            Guid unitId = kvp.Key;
            UnitCombatStats fightStats = kvp.Value;

            // If this unit isn't in the master dictionary yet, initialize them
            if (!RunManager.Instance.masterUnitStats.ContainsKey(unitId))
            {
                RunManager.Instance.masterUnitStats[unitId] = new UnitLifetimeStats
                {
                    unitName = fightStats.UnitName
                };
            }
            UnitLifetimeStats lifetime = RunManager.Instance.masterUnitStats[unitId];
            lifetime.id = unitId;
            lifetime.totalDirectDamageDealt += fightStats.DirectDamageDealt;
            lifetime.totalBurnDamageDealt += fightStats.BurnDamageDealt;
            lifetime.totalPoisonDamageDealt += fightStats.PoisonDamageDealt;
            lifetime.totalDamageTaken += fightStats.DamageTaken;
            lifetime.totalHealingDone += fightStats.HealingDone;
            lifetime.totalShieldingDone += fightStats.ShieldingDone;
            lifetime.totalSlowsApplied += fightStats.SlowsApplied;
            lifetime.totalHastesApplied += fightStats.HastesApplied;
            lifetime.totalAdvancesGiven += fightStats.AdvancesGiven;
        }
    }
}
