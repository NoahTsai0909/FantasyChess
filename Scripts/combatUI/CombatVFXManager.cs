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

        if (RequiresProjectile(action))
        {
            PlayProjectile(action, onImpact);
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

        StartCoroutine(
            TravelProjectile(
                proj.transform,
                action.target.transform,
                projectileTravelTime,
                onImpact
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
                break;

            case CombatActionType.Heal:
                action.target.Visuals.Flash(Color.green);
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


}

