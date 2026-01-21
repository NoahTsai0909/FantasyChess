using UnityEngine;
using System.Collections.Generic;

public enum StatusEffectType
{
    Burn,
    Poison,
    Freeze,
    Slow,
    Haste,
}

public static class StatusEffectOrder
{
    public static readonly List<StatusEffectType> Order = new()
    {
        StatusEffectType.Burn,
        StatusEffectType.Poison,
        StatusEffectType.Freeze,
        StatusEffectType.Slow,
        StatusEffectType.Haste,
    };
}

public class StatusEffectBar : MonoBehaviour
{
    [SerializeField] private Transform iconContainer;
    [SerializeField] private StatusEffectIcon iconPrefab;

    private Dictionary<StatusEffectType, StatusEffectIcon> icons = new();

    public void SetStatus(StatusEffectType type, int stacks)
    {
        if (stacks <= 0)
        {
            RemoveStatus(type);
            return;
        }

        if (!icons.TryGetValue(type, out var icon))
        {
            icon = Instantiate(iconPrefab, iconContainer);
            icon.Initialize(type);
            icons[type] = icon;
            ReorderIcons();
        }

        icon.SetStacks(stacks);
    }

    public void RemoveStatus(StatusEffectType type)
    {
        if (!icons.TryGetValue(type, out var icon))
            return;

        Destroy(icon.gameObject);
        icons.Remove(type);
        ReorderIcons();
    }

    private void ReorderIcons()
    {
        int siblingIndex = 0;

        foreach (var type in StatusEffectOrder.Order)
        {
            if (icons.TryGetValue(type, out var icon))
            {
                icon.transform.SetSiblingIndex(siblingIndex);
                siblingIndex++;
            }
        }
    }

}
