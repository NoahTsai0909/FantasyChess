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
    protected int currentEnergy;
    private int currentShield;
    public int GetCurrentHP() => currentHP;

    protected float cooldownTimer;

    public int burnStacks = 0;
    public int slowStacks = 0;
    public int hasteStacks = 0;

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
            float speedMultiplier = GetCooldownSpeedMultiplier();
            cooldownTimer -= Time.deltaTime * speedMultiplier;
        }
        else
        {
            for (int i = 0; i < stats.Multicast; i++)
            {
                UseAbility();
            }
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

    public virtual void InitializeFromSaveData(UnitSaveData data)
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
        isPassive = definition.isPassive;

        // Permanent progression (keyed by GUID, not definition long-term)
        permanentStats = RunManager.Instance.GetPermanentStatsForUnit(id)?? RunManager.Instance.CreatePermanentStatsForUnit(id);

        // Fresh combat-only modifiers
        temporaryStats = new TemporaryStats();

        RecalculateStats();
    }


    public void RecalculateStats()
    {
        stats = new StatBlock(
            GetRarityAdjustedDefinition(),
            permanentStats,
            temporaryStats  // This should be the same instance
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

    public virtual void EnterCombat(GridManager grid, int row, int col, bool isPlayer)
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
        currentEnergy = stats.maxEnergy;
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
        CombatEventBus.Publish(CombatEventBus.CombatEventType.AbilityUsed, this, null, 0);
    }

    public virtual void CombatStartEffect()
    {

    }

    public virtual void TakeDamage(int dmg)
    {
        if (dmg <= 0)
            return;

        int remainingDamage = dmg;

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, remainingDamage);
            currentShield -= absorbed;
            remainingDamage -= absorbed;

            UpdateShieldBar();
            CombatEventBus.Publish(
                CombatEventType.ShieldDamaged,
                this,
                this,
                0
            );
        }

        if (remainingDamage > 0)
        {
            currentHP = Mathf.Max(0, currentHP - remainingDamage);

            Flash(Color.red);
            UpdateHealthBar();

            CombatEventBus.Publish(
                CombatEventType.DamageTaken,
                this,
                this,
                0
            );

            if (currentHP <= 0)
                Die();
        }
    }

    public virtual void HealDamage(int dmg)
    {
        currentHP = Mathf.Min(stats.MaxHP, currentHP + dmg);
        UpdateHealthBar();
    }

    public virtual void ShieldDamage(int dmg)
    {
        currentShield = currentShield + dmg;
        UpdateShieldBar();
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

        CombatEventBus.Publish(CombatEventType.UnitDied, this, this, 0);
        Destroy(gameObject);
    }


    public void TakeDisasterDamage(int damage)
    {
        if (damage <= 0)
            return;

        int remainingDamage = damage;

        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, remainingDamage);
            currentShield -= absorbed;
            remainingDamage -= absorbed;

            UpdateShieldBar();
        }

        if (remainingDamage > 0)
        {
            currentHP = Mathf.Max(0, currentHP - remainingDamage);

            Flash(Color.purple);
            UpdateHealthBar();

            if (currentHP <= 0)
                Die();
        }
    }

    public void ApplyBurn(int stacks)
    {
        burnStacks += stacks;
        CombatEventBus.PublishStatusChanged(this, StatusEffectType.Burn, burnStacks);
    }

    public void ApplySlow(int stacks)
    {
        slowStacks += stacks;
        CombatEventBus.PublishStatusChanged(this, StatusEffectType.Slow, slowStacks);
    }

    public void ApplyHaste(int stacks)
    {
        hasteStacks += stacks;  
        CombatEventBus.PublishStatusChanged(this, StatusEffectType.Haste, hasteStacks);
    }

    /* =========================
    * Helpers
    * ========================= */

    private void UpdateHealthBar()
    {
        if (uiManager == null) return;
        uiManager.UpdateHealthBar(this, (float)currentHP / stats.MaxHP);
    }

    private void UpdateShieldBar()
    {
        if (uiManager == null) return;
        uiManager.UpdateShieldBar(this, (float)currentShield / stats.MaxHP);
    }
    private void UpdateCooldownBar()
    {
        if (uiManager == null) return;
        uiManager.UpdateCooldownBar(this, cooldownTimer / stats.Cooldown);
    }

    public void Flash(Color color)
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
            Definition.cooldown,
            Mathf.RoundToInt(Definition.shield * multiplier),
            Mathf.RoundToInt(Definition.burn * multiplier),
            Mathf.RoundToInt(Definition.poison * multiplier),
            Definition.maxEnergy,
            Definition.slow,
            Definition.haste,
            Definition.multicast,
            GetRarityAdjustedValue()
        );
    }

    private int GetRarityAdjustedValue()
    {
        int rarityMultiplier;
        if (CurrentRarity == Rarity.Common) rarityMultiplier = 1;
        else if (CurrentRarity == Rarity.Uncommon) rarityMultiplier = 2;
        else if (CurrentRarity == Rarity.Rare) rarityMultiplier = 3;
        else if (CurrentRarity == Rarity.Epic) rarityMultiplier = 4;
        else rarityMultiplier = 1;

        return rarityMultiplier * definition.provisionCost;

    }

    public void TemporaryStatModify(ModifiableStats modifiableStats, int bonus)
    {
        Debug.Log($"{unitName}.TemporaryStatModify({modifiableStats}, {bonus}) called");

        if (modifiableStats == ModifiableStats.Burn)
        {
            temporaryStats.burnBonus += bonus;
        }
        else if (modifiableStats == ModifiableStats.Poison)
        {
            temporaryStats.poisonBonus += bonus;
        }
        else if (modifiableStats == ModifiableStats.MaxEnergy)
        {
            temporaryStats.maxEnergyBonus += bonus;
            currentEnergy += bonus;
        }
        else if (modifiableStats == ModifiableStats.Attack)
        {
            temporaryStats.attackBonus += bonus;
        }
            RecalculateStats();
        return;
    }

    private float GetCooldownSpeedMultiplier()
    {
        bool slowed = slowStacks > 0;
        bool hasted = hasteStacks > 0;

        if (slowed && hasted)
            return 1f;       // cancel out

        if (slowed)
            return 0.5f;     // 50% slower

        if (hasted)
            return 1.5f;     // 50% faster

        return 1f;           // normal
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

    protected UnitInstance FindRandomEnemy()
    {
        if (targetingSystem == null) return null;
        var criteria = new TargetingSystem.TargetCriteria(
            TargetingSystem.TargetTeam.Enemy,
            TargetingSystem.SortMethod.Random
        );
        return targetingSystem.FindUnit(criteria, transform.position);
    }

    protected bool IsAdjacent(UnitInstance other)
    {
        if (other == null) return false;

        int rowDiff = Mathf.Abs(row - other.row);
        int colDiff = Mathf.Abs(col - other.col);

        // 4-direction adjacency
        return (rowDiff + colDiff) == 1;
    }

    protected bool IsSide(UnitInstance other)
    {
        if (other == null) return false;
        return Mathf.Abs(row - other.row) == 1 && col == other.col;
    }

    protected List<UnitInstance> FindAdjacentAllies()
    {
        if (targetingSystem == null) return new List<UnitInstance>();

        var allies = targetingSystem.GetAllies();
        var adjacentAllies = new List<UnitInstance>();

        foreach (var ally in allies)
        {
            if (ally == this) continue;
            if (IsAdjacent(ally))
            {
                adjacentAllies.Add(ally);
            }
        }
        return adjacentAllies;
    }

    protected List<UnitInstance> FindSideAllies()
    {
        if (targetingSystem == null) return new List<UnitInstance>();

        var allies = targetingSystem.GetAllies();
        var sideAllies = new List<UnitInstance>();

        foreach (var ally in allies)
        {
            if (ally == this) continue;
            if (IsSide(ally))
            {
                sideAllies.Add(ally);
            }
        }
        return sideAllies;
    }

    protected List<UnitInstance> FindAllAllies()
    {
        if (targetingSystem == null) return new List<UnitInstance>();
        return targetingSystem.GetAllies();
    }


    protected virtual void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target, int amount)
    {
    }

    protected virtual void HandleCombatAction(CombatAction action) { }
}
