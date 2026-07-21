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

// Create a struct to map types to sprites in the Inspector
[System.Serializable]
public struct StatusSpriteMapping
{
    public StatusEffectType type;
    public Sprite iconSprite;
}

public class StatusEffectBar : MonoBehaviour
{
    [SerializeField] private Transform iconContainer;
    [SerializeField] private StatusEffectIcon iconPrefab;

    // Add this list so you can drag and drop your sprites in the Inspector
    [SerializeField] private List<StatusSpriteMapping> statusSprites = new();

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

            // Find the correct sprite for this status type
            Sprite iconSprite = GetSpriteForType(type);
            icon.Initialize(type, iconSprite);

            icons[type] = icon;
            ReorderIcons();
        }

        icon.SetStacks(stacks);
    }

    // Helper method to look up the sprite
    private Sprite GetSpriteForType(StatusEffectType type)
    {
        foreach (var mapping in statusSprites)
        {
            if (mapping.type == type)
                return mapping.iconSprite;
        }
        return null; // Fallback if no sprite is assigned
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
