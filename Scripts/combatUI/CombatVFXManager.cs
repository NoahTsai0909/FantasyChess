using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

public class CombatVFXManager : MonoBehaviour
{
    public static CombatVFXManager Instance;

    [Header("Defaults")]
    [SerializeField] private float projectileTravelTime;

    [Header("Fallback Projectile Prefabs")]
    [SerializeField] private GameObject defaultDamageProjectilePrefab;
    [SerializeField] private GameObject defaultHealProjectilePrefab;
    [SerializeField] private GameObject defaultShieldProjectilePrefab;
    [SerializeField] private GameObject defaultBurnProjectilePrefab;
    [SerializeField] private GameObject defaultSlowProjectilePrefab;
    [SerializeField] private GameObject defaultHasteProjectilePrefab;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject healImpactPrefab;
    [SerializeField] private GameObject attackImpactPrefab;
    [SerializeField] private GameObject shieldImpactPrefab;
    [SerializeField] private GameObject burnImpactPrefab;
    [SerializeField] private GameObject meleeSlashPrefab;
    [SerializeField] private GameObject slowImpactPrefab;
    [SerializeField] private GameObject hasteImpactPrefab;

    [Header("Fallback Colors")]
    [SerializeField] private Color defaultDamageColor = Color.red;
    [SerializeField] private Color defaultHealColor = Color.green;
    [SerializeField] private Color defaultShieldColor = Color.yellow;
    [SerializeField] private Color defaultBurnColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color defaultSlowColor = Color.brown;
    [SerializeField] private Color defaultHasteColor = Color.cyan;

    [Header("Projectile Motion")]
    public float maxArcHeight = 2.0f; // How high it arcs
    [Tooltip("Controls the speed of the projectile over its travel time.")]
    public AnimationCurve projectileSpeedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Draw the physical shape of the arc! Start at 0, peak at 1, end at 0.")]
    public AnimationCurve projectileArcCurve = new AnimationCurve(
    new Keyframe(0f, 0f),
    new Keyframe(0.3f, 1f), // Peaks early at 30% of the flight
    new Keyframe(1f, 0f)
);

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
            case CombatActionType.Shield:
            case CombatActionType.ApplySlow:
            case CombatActionType.ApplyHaste:
                return true;

            default:
                return false;
        }
    }

    private void PlayProjectile(CombatAction action, Action onImpact)
    {
        GameObject projectilePrefab = GetProjectileForAction(action);

        if (projectilePrefab == null)
        {
            onImpact?.Invoke();
            return;
        }

        Vector3 start = action.source.transform.position;
        GameObject proj = Instantiate(projectilePrefab, start, Quaternion.identity);

        TrailRenderer tr = proj.GetComponent<TrailRenderer>();
        if (tr != null)
        {
            Color projColor = GetColorForAction(action);
            /*tr.startColor = projColor;

            // Fade out to the same color but with 0 Alpha
            tr.endColor = new Color(projColor.r, projColor.g, projColor.b, 0f);*/
        }

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
            else if (action.type == CombatActionType.ApplySlow && action.target != null && slowImpactPrefab != null)
            {
                Instantiate(slowImpactPrefab, action.target.transform.position, Quaternion.identity);
            }
            else if (action.type == CombatActionType.ApplyHaste && action.target != null && hasteImpactPrefab != null)
            {
                Instantiate(hasteImpactPrefab, action.target.transform.position, Quaternion.identity);
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

    private GameObject GetProjectileForAction(CombatAction action)
    {
        // 1. Use action-specific override if provided (Ensure CombatAction.projectileOverride is now a GameObject!)
        if (action.projectileOverride != null)
        {
            return action.projectileOverride;
        }

        // 3. Fall back to default based on action type
        return GetFallbackProjectile(action.type);
    }


    private IEnumerator TravelProjectile(Transform projectile, Transform target, float duration, Action onImpact)
    {
        float elapsed = 0f;
        Vector3 startPos = projectile.position;

        while (elapsed < duration)
        {
            if (target == null)
            {
                onImpact?.Invoke();
                Destroy(projectile.gameObject);
                yield break;
            }

            elapsed += Time.deltaTime;

            // 1. Calculate linear time (0.0 to 1.0)
            float linearT = elapsed / duration;

            // 2. Feed it through your curve for easing (speed changes)
            float easedT = projectileSpeedCurve.Evaluate(linearT);

            // 3. Get the base straight-line position using the eased time
            Vector3 currentTargetPos = target.position;
            Vector3 currentPos = Vector3.Lerp(startPos, currentTargetPos, easedT);

            // 4. Add the Arc! 
            // Mathf.Sin of PI * linearT gives a perfect curve that starts at 0, peaks at 0.5, and ends at 0.
            float arc = projectileArcCurve.Evaluate(linearT) * maxArcHeight;
            currentPos.y += arc; // Push it upward along the Y axis

            // 5. Calculate rotation before moving so the projectile points exactly where it's arcing
            Vector3 moveDirection = currentPos - projectile.position;
            if (moveDirection != Vector3.zero)
            {
                float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
                projectile.rotation = Quaternion.Euler(0, 0, angle);
            }

            // 6. Finally, update the position
            projectile.position = currentPos;

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

    private GameObject GetFallbackProjectile(CombatActionType actionType)
    {
        return actionType switch
        {
            CombatActionType.Damage => defaultDamageProjectilePrefab,
            CombatActionType.Heal => defaultHealProjectilePrefab,
            CombatActionType.Shield => defaultShieldProjectilePrefab,
            CombatActionType.ApplyBurn => defaultBurnProjectilePrefab,
            CombatActionType.ApplyHaste => defaultHasteProjectilePrefab,
            CombatActionType.ApplySlow => defaultSlowProjectilePrefab,
            _ => defaultDamageProjectilePrefab // Default fallback
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

    private Color GetColorForAction(CombatAction action)
    {
        return action.type switch
        {
            CombatActionType.Damage => defaultDamageColor,
            CombatActionType.Heal => defaultHealColor,
            CombatActionType.Shield => defaultShieldColor,
            CombatActionType.ApplyBurn => defaultBurnColor,
            CombatActionType.ApplySlow => defaultSlowColor,
            CombatActionType.ApplyHaste => defaultHasteColor,
            _ => Color.white
        };
    }

}

