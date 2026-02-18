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

        GameObject proj = Instantiate(defaultProjectilePrefab);
        SpriteRenderer sr = proj.GetComponent<SpriteRenderer>();
        sr.sprite = projectileSprite;

        Vector3 start = action.source.transform.position;
        Vector3 end = action.target.transform.position;

        Vector3 dir = end - start;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        sr.transform.rotation = Quaternion.Euler(0, 0, angle);


        float travelTime = projectileTravelTime;

        StartCoroutine(
            TravelProjectile(proj.transform, start, end, travelTime, onImpact)
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


    private IEnumerator TravelProjectile(Transform projectile, Vector3 start, Vector3 end, float duration, Action onImpact)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            projectile.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        onImpact?.Invoke();
        Destroy(projectile.gameObject);
    }

    private void PlayInstantEffect(CombatAction action)
    {
        switch (action.type)
        {
            case CombatActionType.Shield:
                // Flash blue
                action.target.Flash(Color.gold);
                break;

            case CombatActionType.Heal:
                action.target.Flash(Color.green);
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

