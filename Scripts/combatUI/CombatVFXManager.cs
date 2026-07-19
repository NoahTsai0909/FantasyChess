using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class CombatVFXManager : MonoBehaviour
{
    public static CombatVFXManager Instance;

    [Header("Defaults")]
    [SerializeField] private GameObject defaultProjectilePrefab;
    [SerializeField] private float projectileTravelTime;

    [Header("Fallback Projectiles")]
    [SerializeField] private Sprite defaultDamageProjectile;
    [SerializeField] private Sprite defaultHealProjectile;
    [SerializeField] private Sprite defaultShieldProjectile;
    [SerializeField] private Sprite defaultBurnProjectile;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject healImpactPrefab;
    [SerializeField] private GameObject attackImpactPrefab;
    [SerializeField] private GameObject shieldImpactPrefab;
    [SerializeField] private GameObject burnImpactPrefab;
    [SerializeField] private GameObject meleeSlashPrefab;

    [Header("Projectile Motion")]
    public float maxArcHeight = 2.0f; // How high it arcs
    [Tooltip("Controls the speed of the projectile over its travel time.")]
    public AnimationCurve projectileSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private void Awake()
    {
        Instance = this;
    }

    public void PlayActionVFX(CombatAction action, Action onImpact)
    {
        if (action.source == null || action.target == null)
        {
            onImpact?.Invoke();
            return;
        }

        if (RequiresProjectile(action) && action.source != action.target)
        {
            // Check if the source is melee AND if the action is actually a damage attack
            // Note: Update "definition" if your UnitInstance uses a capital "Definition"
            if (action.source.Definition != null && action.source.Definition.isMelee && action.type == CombatActionType.Damage)
            {
                PlayMeleeEffect(action, onImpact);
            }
            else
            {
                // Ranged units, or melee units using non-damage actions (like heals)
                PlayProjectile(action, onImpact);
            }
        }
        else
        {
            PlayInstantEffect(action);
            onImpact?.Invoke();
        }
    }

    private bool RequiresProjectile(CombatAction action)
    {
        // You can expand this later
        switch (action.type)
        {
            case CombatActionType.Damage:
            case CombatActionType.Heal:
            case CombatActionType.ApplyBurn:
                return true;

            default:
                return false;
        }
    }

    private void PlayProjectile(CombatAction action, Action onImpact)
    {
        Sprite projectileSprite = GetProjectileForAction(action);

        if (projectileSprite == null)
        {
            onImpact?.Invoke();
            return;
        }

        Vector3 start = action.source.transform.position;
        GameObject proj = Instantiate(defaultProjectilePrefab, start, Quaternion.identity);

        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        sr.sprite = projectileSprite;

        // --- NEW LOGIC: Wrap the impact action ---
        Action wrappedOnImpact = () =>
        {
            // 1. Spawn the VFX if it's a heal and the target is still alive
            if (action.type == CombatActionType.Heal && action.target != null && healImpactPrefab != null)
            {
                Instantiate(healImpactPrefab, action.target.transform.position, Quaternion.identity);
            }
            else if (action.type == CombatActionType.Damage && action.target != null && attackImpactPrefab != null)
            {
                Instantiate(attackImpactPrefab, action.target.transform.position, Quaternion.identity);
            }
            else if (action.type == CombatActionType.Shield && action.target != null && shieldImpactPrefab != null)
            {
                Instantiate(shieldImpactPrefab, action.target.transform.position, Quaternion.identity);
            }
            else if (action.type == CombatActionType.ApplyBurn && action.target != null && burnImpactPrefab != null)
            {
                Instantiate(burnImpactPrefab, action.target.transform.position, Quaternion.identity);
            }

                // 2. Execute the actual gameplay heal logic you passed in originally
                onImpact?.Invoke();
        };

        StartCoroutine(
            TravelProjectile(
                proj.transform,
                action.target.transform,
                projectileTravelTime,
                wrappedOnImpact // <-- Pass the wrapped action here!
            )
        );
    }

    private Sprite GetProjectileForAction(CombatAction action)
    {
        // 1. Use action-specific override if provided
        if (action.projectileOverride != null)
        {
            return action.projectileOverride;
        }

        // 3. Fall back to default based on action type
        return GetFallbackProjectile(action.type);
    }


    private IEnumerator TravelProjectile(
    Transform projectile,
    Transform target,
    float duration,
    Action onImpact)
    {
        float elapsed = 0f;
        Vector3 start = projectile.position;

        while (elapsed < duration)
        {
            if (target == null)
            {
                onImpact?.Invoke();
                Destroy(projectile.gameObject);
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentTargetPos = target.position;

            // Move
            projectile.position = Vector3.Lerp(start, currentTargetPos, t);

            //Update rotation dynamically
            Vector3 dir = currentTargetPos - projectile.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            projectile.rotation = Quaternion.Euler(0, 0, angle);

            yield return null;
        }

        if (target != null)
            onImpact?.Invoke();

        Destroy(projectile.gameObject);
    }

    private void PlayInstantEffect(CombatAction action)
    {
        if (action.target == null || action.target.Visuals == null) return;

        switch (action.type)
        {
            case CombatActionType.Shield:
                action.target.Visuals.Flash(Color.gold);
                if (shieldImpactPrefab != null)
                {
                    Instantiate(shieldImpactPrefab, action.target.transform.position, Quaternion.identity);
                }
                break;

            case CombatActionType.Heal:
                action.target.Visuals.Flash(Color.green);
                if (healImpactPrefab != null)
                {
                    Instantiate(healImpactPrefab, action.target.transform.position, Quaternion.identity);
                }
                break;
        }
    }

    private Sprite GetFallbackProjectile(CombatActionType actionType)
    {
        return actionType switch
        {
            CombatActionType.Damage => defaultDamageProjectile,
            CombatActionType.Heal => defaultHealProjectile,
            CombatActionType.Shield => defaultShieldProjectile,
            CombatActionType.ApplyBurn => defaultBurnProjectile,
            _ => defaultDamageProjectile // Default fallback
        };
    }

    private void PlayMeleeEffect(CombatAction action, Action onImpact)
    {
        StartCoroutine(MeleeRoutine(action, onImpact));
    }

    private IEnumerator MeleeRoutine(CombatAction action, Action onImpact)
    {
        // 1. Spawn the slash effect at the ATTACKER'S position
        if (action.source != null && meleeSlashPrefab != null)
        {
            // Store a reference to the spawned object so we can modify it
            GameObject slashVFX = Instantiate(meleeSlashPrefab, action.source.transform.position, Quaternion.identity);

            // Check if it's an enemy (isPlayer == false)
            if (!action.source.isPlayer)
            {
                // Invert the X scale to flip the entire prefab horizontally
                Vector3 scale = slashVFX.transform.localScale;
                scale.x *= -1;
                slashVFX.transform.localScale = scale;
            }
        }

        // 2. Wait for a short duration to simulate the attack landing
        yield return new WaitForSeconds(0.2f);

        // 3. NEW: Spawn the target impact VFX right before the damage happens
        if (action.target != null && attackImpactPrefab != null)
        {
            Instantiate(attackImpactPrefab, action.target.transform.position, Quaternion.identity);
        }

        // 4. Trigger the actual damage and the target's visual impact
        onImpact?.Invoke();
    }


}

