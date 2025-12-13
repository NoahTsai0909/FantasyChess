using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static SceneLoader;

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

    private void OnCombatEvent(CombatEventBus.CombatEventType type, UnitInstance source, UnitInstance target)
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
        combatActive = false;

        if (disasterManager != null)
            disasterManager.StopDisaster();

        Time.timeScale = 0.5f;

        Debug.Log($"Combat ended. Player won: {playerWon}");

        // Apply rewards only if player won
        if (playerWon && RunManager.Instance.selectedEvent != null)
        {
            // Get the combat event that started this battle
            var combatEvent = RunManager.Instance.selectedEvent as CombatEventSO;
            if (combatEvent != null)
            {
                // Apply combat-specific rewards
                int baseGold = 30 + (RunManager.Instance.reputation * 5);
                RunManager.Instance.currentGold += baseGold;
                RunManager.Instance.reputation += 1;
            }

            // Mark the event as completed
            RunManager.Instance.selectedEvent.CompleteEvent();
        }
        else if (!playerWon)
        {
            // Player lost - still mark event as completed but no rewards
            if (RunManager.Instance.selectedEvent != null)
                RunManager.Instance.selectedEvent.CompleteEvent();
        }

        // Start coroutine to transition scene
        StartCoroutine(TransitionAfterDelay(playerWon));
    }

    private IEnumerator TransitionAfterDelay(bool playerWon)
    {
        yield return new WaitForSeconds(endCombatDelay);

        if (playerWon)
        {
            // Go to map scene to continue run
            SceneLoader.Instance.LoadScene(GameScene.MapScene);
        }
        else
        {
            // Player lost - go to main menu or run summary
            SceneLoader.Instance.LoadScene(GameScene.MainMenuScene);
            RunManager.Instance.ResetRun();
        }
    }

    public void InitializeBattlefield(TeamDefinition playerTeam, EncounterDefinition encounter)
    {
        // Spawn player team
        foreach (var unitPlacement in playerTeam.units)
        {
            if (unitPlacement.unitPrefab != null)
            {
                SpawnUnit(unitPlacement.unitPrefab, playerGrid,
                         unitPlacement.row, unitPlacement.col, true);
            }
        }

        // Spawn enemies
        foreach (var unitPlacement in encounter.enemyUnits)
        {
            if (unitPlacement.unitPrefab != null)
            {
                SpawnUnit(unitPlacement.unitPrefab, enemyGrid,
                         unitPlacement.row, unitPlacement.col, false);
            }
        }
    }

    private UnitInstance SpawnUnit(UnitInstance unitPrefab, GridManager grid,
                                   int row, int col, bool isPlayer)
    {
        // 1. Instantiate the SPECIFIC prefab (BannerKnight, SolemnPriest, etc.)
        UnitInstance unit = Instantiate(unitPrefab);

        // 2. Set team
        unit.isPlayer = isPlayer;

        // 3. Initialize with UI prefabs
        unit.Initialize(grid, row, col);

        return unit;
    }

}
