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
        Sprite projectileSprite = action.source.Definition.defaultProjectile;

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


}

