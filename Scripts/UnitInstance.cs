using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static CombatEventBus;

public class UnitInstance : MonoBehaviour
{

    [SerializeField] private UnitDefinition definition;

    private int currentHP;
    public int GetCurrentHP() => currentHP;
    public UnitDefinition Definition => definition;
    protected float cooldownTimer;
    protected bool isPassive;
    public string unitName;
    public bool isPlayer;   // Keep this — it affects sprite direction
    protected TemporaryStats temporaryStats;
    protected PermanentStats permanentStats;
    protected StatBlock stats;

    public int row;
    public int col;

    private SpriteRenderer sr;
    private Coroutine flashCoroutine;
    private Color originalSpriteColor;
    private GridManager myGrid;  // The grid this unit belongs to
    private TargetingSystem targetingSystem;

    [SerializeField] private GameObject healthBarPrefab;
    private Image healthBarFill;
    private GameObject healthBarInstance;

    [SerializeField] private GameObject cooldownBarPrefab;
    private Image cooldownBarFill;
    private GameObject cooldownBarInstance;
    private BattleUIManager uiManager;
    private UnitInstance sourcePrefab;
    public UnitInstance SourcePrefab => sourcePrefab;
    protected virtual void Awake()
    {
        temporaryStats = new TemporaryStats();

        // Sprite setup
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (stats == null)
        {
            Debug.LogWarning($"{name} started without Initialize()");
        }
        sr.sprite = definition.unitSprite;
        originalSpriteColor = sr.color;
        sr.flipX = !isPlayer;
    }

    private void Update()
    {
        if (ShouldUpdateCombat() && !isPassive)
        {
            UpdateCooldownBar();
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;
            }
            else if (definition != null)
            {
                UseAbility();
                cooldownTimer = stats.Cooldown;
            }
        }
    }

    protected virtual void UseAbility()
    {
        //Base implementation, children should override this
        CombatEventBus.Publish(CombatEventBus.CombatEventType.AbilityUsed, this, null);
    }

    public void Initialize(GridManager grid, int targetRow, int targetCol, UnitInstance source)
    {
        sourcePrefab = source;
        myGrid = grid;
        row = targetRow;
        col = targetCol;

        // Tell GridManager to position this unit
        permanentStats = RunManager.Instance.GetPermanentStatsForUnit(sourcePrefab);
        stats = new StatBlock(definition, permanentStats, temporaryStats);
        currentHP = stats.MaxHP;
        cooldownTimer = stats.Cooldown;
        unitName = definition.unitName;
        isPassive = definition.isPassive;
        myGrid.PlaceUnit(this, row, col);
        gameManager gm = FindFirstObjectByType<gameManager>();
        if (gm != null)
        {
            targetingSystem = new TargetingSystem(
                isPlayer ? gm.playerGrid : gm.enemyGrid,
                isPlayer ? gm.enemyGrid : gm.playerGrid,
                isPlayer
            );
        }
        uiManager = FindFirstObjectByType<BattleUIManager>();
        if (uiManager != null)
        {
            uiManager.CreateUnitUI(this, transform.position);
            UpdateHealthBar();
        }

    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Max(0, currentHP);

        UpdateHealthBar();

        if (flashCoroutine != null)
        {
            sr.color = originalSpriteColor;
        }

        flashCoroutine = StartCoroutine(FlashDamage());
        CombatEventBus.Publish(CombatEventType.DamageTaken, this, this);

        if (currentHP <= 0)
        {
            Die();
        }

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
            // Still publish death event, but with special handling if needed
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

    public virtual void HealDamage(int dmg)
    {
        currentHP += dmg;
        currentHP = Mathf.Min(currentHP, stats.MaxHP);
        CombatEventBus.Publish(CombatEventType.Healed, this, this);
        UpdateHealthBar();
    }

    private void UpdateCooldownBar() { 
    
        if (uiManager != null)
        {
            float fillAmount = (float)cooldownTimer / stats.Cooldown;
            uiManager.UpdateCooldownBar(this, fillAmount);
        }
    }

    private void UpdateHealthBar()
    {
        if (uiManager != null)
        {
            float fillAmount = (float)currentHP / stats.MaxHP;
            uiManager.UpdateHealthBar(this, fillAmount);
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        if (sr == null) yield break;

        Color originalColor = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;

        flashCoroutine = null;
    }

    public virtual void Die()
    {
        Debug.Log($"{definition.unitName} died");

        if (myGrid != null)
        {
            Vector2Int pos = myGrid.GetUnitPosition(this);
            myGrid.RemoveUnit(pos.x, pos.y);
        }
        if (uiManager != null)
        {
            uiManager.RemoveUnitUI(this);
        }
        CombatEventBus.Publish(CombatEventType.UnitDied, this, this);
        Destroy(gameObject);
    }
    public virtual string GetAbilityDescription()
    {
        return "";
    }


    public void SetSourcePrefab(UnitInstance prefab)
    {
        sourcePrefab = prefab;
    }

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

    private bool ShouldUpdateCombat()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "CombatScene")
            return false;
        return true;
    }

    protected virtual void HandleCombatEvent(CombatEventType type, UnitInstance source, UnitInstance target)
    {
    }
}
