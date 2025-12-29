using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static CombatEventBus;

public class UnitInstance : MonoBehaviour
{

    [SerializeField] private UnitDefinition definition;
    public UnitDefinition Definition => definition;

    public Rarity CurrentRarity { get; private set; }

    protected PermanentStats permanentStats;
    protected TemporaryStats temporaryStats;
    protected StatBlock stats;

    public StatBlock Stats => stats;

    public string unitName;
    protected bool isPassive;

    /* =========================
     * Combat state
     * ========================= */

    private bool inCombat = false;

    private int currentHP;
    public int GetCurrentHP() => currentHP;

    protected float cooldownTimer;

    public Guid id;
    public int row;
    public int col;
    public bool isPlayer;
    private GridManager myGrid;
    public RunManager.UnitPlacement myPlacement;
    private TargetingSystem targetingSystem;

    /* =========================
     * Visuals / UI
     * ========================= */

    private SpriteRenderer sr;
    private Color originalSpriteColor;
    private Coroutine flashCoroutine;

    private BattleUIManager uiManager;

    /* =========================
     * Unity lifecycle
     * ========================= */

    protected virtual void Awake()
    {
        temporaryStats = new TemporaryStats();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (definition != null)
        {
            sr.sprite = definition.unitSprite;
            unitName = definition.unitName;
            isPassive = definition.isPassive;
        }

        originalSpriteColor = sr.color;
        UpdateSpriteDirection();
    }

    private void Update()
    {
        if (!inCombat || isPassive)
            return;

        UpdateCooldownBar();

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else
        {
            UseAbility();
            cooldownTimer = stats.Cooldown;
        }
    }

    public void InitializeEnemy(UnitDefinition def, Rarity rarity)
    {
        definition = def;
        CurrentRarity = rarity;

        permanentStats = null;
        temporaryStats = new TemporaryStats();

        RecalculateStats();
    }

    public void InitializeFromSaveData(UnitSaveData data)
    {
        if (data == null)
        {
            Debug.LogError("InitializeFromSaveData called with null UnitSaveData");
            return;
        }

        // Core identity
        definition = data.definition;
        CurrentRarity = data.rarity;
        id = data.id;

        // Permanent progression (keyed by GUID, not definition long-term)
        permanentStats = RunManager.Instance.GetPermanentStatsForUnit(id);

        // Fresh combat-only modifiers
        temporaryStats = new TemporaryStats();

        RecalculateStats();
    }

    public void RecalculateStats()
    {
        stats = new StatBlock(
            GetRarityAdjustedDefinition(),
            permanentStats,
            temporaryStats
        );
    }

    public void SetPlayerSide(bool isPlayerSide)
    {
        isPlayer = isPlayerSide;
        UpdateSpriteDirection();
    }

    private void UpdateSpriteDirection()
    {
        if (sr != null)
            sr.flipX = !isPlayer;
    }

    public void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
    {
        this.row = row;
        this.col = col;

        grid.PlaceUnit(row, col, this, isPlayer);

        SetPlayerSide(isPlayer);
        InitializeCombatState();
        SetupTargeting();
        SetupCombatUI();

        inCombat = true;
    }

    public void InitializeCombatState()
    {
        currentHP = stats.MaxHP;
        cooldownTimer = stats.Cooldown;
    }

    private void SetupTargeting()
    {
        gameManager gm = FindFirstObjectByType<gameManager>();
        if (gm == null) return;

        targetingSystem = new TargetingSystem(
            isPlayer ? gm.playerGrid : gm.enemyGrid,
            isPlayer ? gm.enemyGrid : gm.playerGrid,
            isPlayer
        );
    }

    private void SetupCombatUI()
    {
        uiManager = FindFirstObjectByType<BattleUIManager>();
        if (uiManager == null) return;

        uiManager.CreateUnitUI(this, transform.position);
        UpdateHealthBar();
    }

    /* =========================
     * TIER / MERGE LOGIC
     * ========================= */

    public bool CanUpgradeTier()
    {
        return CurrentRarity != Rarity.Epic;
    }

    public void UpgradeTier()
    {
        if (!CanUpgradeTier())
            return;

        Rarity old = CurrentRarity;
        CurrentRarity = RarityScaling.GetNextRarity(CurrentRarity);

        RecalculateStats();

        Debug.Log($"{definition.unitName} upgraded {old} to {CurrentRarity}");
    }

    /* =========================
     * COMBAT ACTIONS
     * ========================= */

    protected virtual void UseAbility()
    {
        CombatEventBus.Publish(CombatEventBus.CombatEventType.AbilityUsed, this, null);
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP = Mathf.Max(0, currentHP - dmg);
        UpdateHealthBar();

        Flash(Color.red);
        CombatEventBus.Publish(CombatEventType.DamageTaken, this, this);

        if (currentHP <= 0)
            Die();
    }

    public virtual void HealDamage(int dmg)
    {
        currentHP = Mathf.Min(stats.MaxHP, currentHP + dmg);
        UpdateHealthBar();
    }

    public virtual void Die()
    {
        if (myGrid != null)
        {
            Vector2Int pos = myGrid.GetUnitPosition(this);
            myGrid.RemoveUnit(pos.x, pos.y);
        }

        if (uiManager != null)
            uiManager.RemoveUnitUI(this);

        CombatEventBus.Publish(CombatEventType.UnitDied, this, this);
        Destroy(gameObject);
    }


    public void TakeDisasterDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        // Update health bar but NO event publishing
        UpdateHealthBar();

        // Optional: Different visual feedback (purple flash instead of red)
        if (flashCoroutine != null)
        {
            sr.color = originalSpriteColor;
        }
        flashCoroutine = StartCoroutine(FlashDisasterDamage());

        if (currentHP <= 0)
        {
            CombatEventBus.Publish(CombatEventType.UnitDied, null, this);
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDisasterDamage()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = new Color(0.5f, 0f, 0.5f); // Purple flash
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;

        flashCoroutine = null;
    }

    /* =========================
    * Helpers
    * ========================= */

    private void UpdateHealthBar()
    {
        if (uiManager == null) return;
        uiManager.UpdateHealthBar(this, (float)currentHP / stats.MaxHP);
    }

    private void UpdateCooldownBar()
    {
        if (uiManager == null) return;
        uiManager.UpdateCooldownBar(this, cooldownTimer / stats.Cooldown);
    }

    private void Flash(Color color)
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine(color));
    }

    private System.Collections.IEnumerator FlashRoutine(Color color)
    {
        sr.color = color;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalSpriteColor;
        flashCoroutine = null;
    }
    public virtual string GetAbilityDescription()
    {
        return "";
    }


    IStatSource GetRarityAdjustedDefinition()
    {
        int delta = CurrentRarity - Definition.startingRarity;
        float multiplier = RarityScaling.GetMultiplier(delta);

        return new UnitDefinitionView(
            Mathf.RoundToInt(Definition.attack * multiplier),
            Mathf.RoundToInt(Definition.heal * multiplier),
            Mathf.RoundToInt(Definition.maxHP * multiplier),
            Definition.cooldown
        );
    }

    /* =========================
     * Targeting helpers
     * ========================= */

    protected UnitInstance FindNearestEnemy()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Enemy,
            TargetingSystem.SortMethod.Nearest
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }

    protected UnitInstance FindFarthestEnemy()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Enemy,
            TargetingSystem.SortMethod.Farthest
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }

    protected UnitInstance FindLowestHealthAlly()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Ally,
            TargetingSystem.SortMethod.LowestHealth
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }

    protected virtual void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target)
    {
    }
}
