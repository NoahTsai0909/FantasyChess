using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CombatEventBus;
using static SceneLoader;
using static UnityEngine.Rendering.DebugUI.Table;

public class gameManager : MonoBehaviour
{

    [Header("Grids")]
    public GridManager playerGrid;
    public GridManager enemyGrid;

    [Header("UI Manager")]
    [SerializeField] private BattleUIManager battleUIManager;

    [Header("Combat Settings")]
    [SerializeField] private float endCombatDelay = 1.5f;

    [Header("Disaster System")]
    [SerializeField] private DisasterManager disasterManager;

    private bool combatActive = true;
    public bool isCombatActive() => combatActive;

    private UnitInstance unitReward;

    void Start()
    {
        TeamDefinition playerTeam = RunManager.Instance.GetTeamForCombat();
        EncounterDefinition currentEncounter = RunManager.Instance.currentEncounter;
        if (playerTeam != null && currentEncounter != null)
        {
            InitializeBattlefield(playerTeam, currentEncounter);
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
    }

    private void OnDestroy()
    {
        CombatEventBus.OnCombatEvent -= OnCombatEvent;
    }

    private void OnCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
        if (type == CombatEventBus.CombatEventType.UnitDied && combatActive)
        {
            // Check combat state when a unit dies
            StartCoroutine(CheckCombatEndDelayed(0.1f)); // Small delay to let grid update
        }
    }

    private IEnumerator CheckCombatEndDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        CheckCombatEnd();
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
        combatActive = false;

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
                RunManager.Instance.currentGold += combatEvent.goldReward;
                RunManager.Instance.reputation += combatEvent.reputationReward;
                PlayerUnitManager.Instance.TryAcquireUnit(unitReward.Definition, unitReward.CurrentRarity);
            }
            // Mark the event as completed
            RunManager.Instance.selectedEvent.CompleteEvent();
        }
        else if (!playerWon)
        {
            // Player lost - still mark event as completed but no rewards
            if (RunManager.Instance.selectedEvent != null)
                RunManager.Instance.selectedEvent.CompleteEvent();
            RunManager.Instance.playerHealth -= RunManager.Instance.currentDay;
        }

        // Start coroutine to transition scene
        StartCoroutine(TransitionAfterDelay(playerWon));
    }

    private IEnumerator TransitionAfterDelay(bool playerWon)
    {
        yield return new WaitForSeconds(endCombatDelay);

        if (playerWon || RunManager.Instance.playerHealth > 0)
        {
            // Go to map scene to continue run
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
        }
        else
        {
            // Player lost - go to main menu or run summary
            Time.timeScale = 1f;
            SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
            RunManager.Instance.ResetRun();
        }
    }

    public void InitializeBattlefield(TeamDefinition playerTeam, EncounterDefinition encounter)
    {
        // Player team
        foreach (var placement in playerTeam.units)
        {
            if (placement.unitData == null)
                continue;

            SpawnPlayerUnit(placement, playerGrid);
        }

        // Enemies (still definition-based)
        foreach (var enemyPlacement in encounter.enemyUnits)
        {
            SpawnEnemyUnit(enemyPlacement, enemyGrid);
        }

        foreach (var playerUnit in playerGrid.GetAllUnits())
        {
            playerUnit.CombatStartEffect();
        }
        foreach (var enemyUnit in enemyGrid.GetAllUnits())
        {
            enemyUnit.CombatStartEffect();
        }
        unitReward = enemyGrid.GetRandomUnit();

    }


    private UnitInstance SpawnPlayerUnit(RunManager.UnitPlacement placement, GridManager grid)
    {
        // Spawn visual from prefab
        UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);

        // Initialize stats from the saved data
        unit.InitializeFromSaveData(placement.unitData);

        // Set placement reference and enter combat
        unit.EnterCombat(grid, placement.row, placement.col, true);

        return unit;
    }

    private UnitInstance SpawnEnemyUnit(RunManager.UnitPlacement placement, GridManager grid)
    {
        UnitInstance unit = Instantiate(placement.unitData.definition.unitPrefab);


        unit.InitializeEnemy(placement.unitData.definition, placement.unitData.rarity);

        unit.EnterCombat(grid, placement.row, placement.col, false);

        return unit;
    }

}
