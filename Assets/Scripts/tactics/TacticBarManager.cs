using System.Collections.Generic;
using UnityEngine;

public class TacticBarManager : MonoBehaviour
{
    public enum BarAlignment { Left, Center, Right }
    public BarAlignment alignment = BarAlignment.Left; // Player will be Left, Enemy will be Right

    [Tooltip("The standard distance between tactics")]
    public float defaultSpacing = 1f;

    [Tooltip("The maximum physical width the bar is allowed to take up on screen before it starts squishing tactics closer together")]
    public float maxBarWidth = 20f;

    [Header("Visuals")]
    public Vector2 tacticVisualOffset = new Vector2(0f, 0.5f);
    [Tooltip("If true, tactics will alternate up and down to interlock.")]
    public bool useVerticalStagger = true;

    [Tooltip("How much to physically drop every alternating tactic.")]
    public float verticalStaggerAmount = -1f;

    // The dynamic, gapless timeline of tactics
    private List<TacticInstance> activeTactics = new List<TacticInstance>();

    [Header("Combat State")]
    public bool isCombatRunning = false;


    void Update()
    {
        if (!isCombatRunning) return;

        // 1. Find the first active tactic in the line that HAS NOT fired yet
        TacticInstance currentActive = GetFirstReadyActiveTactic();

        // 2. If we found one, tick its timer down
        if (currentActive != null)
        {
            if (currentActive.TickCooldown(Time.deltaTime))
            {
                // BOOM! Timer hit 0. Fire the effect!
                currentActive.ExecuteActiveEffect();

                // Mark it as permanently spent for this combat so the runner moves to the next one
                currentActive.MarkAsSpent();
            }
        }
    }

    private TacticInstance GetFirstReadyActiveTactic()
    {
        foreach (var tactic in activeTactics)
        {
            // We ignore passives, and we ignore anything that has already triggered
            if (tactic != null && !tactic.isPassive && !tactic.isSpent)
            {
                return tactic;
            }
        }
        return null;
    }

    public void StartCombat(bool isPlayerBar)
    {
        isCombatRunning = true;
        foreach (var tactic in activeTactics)
        {
            if (tactic != null)
            {
                // Pass it straight down to the tactic
                tactic.SetupTargeting(isPlayerBar);
                tactic.EnterCombat();
            }
        }
    }

    public void StopCombat()
    {
        isCombatRunning = false;
        foreach (var tactic in activeTactics)
        {
            if (tactic != null)
            {
                tactic.inCombat = false;
                if (tactic.isPassive) tactic.RemovePassiveEffect();
            }
        }
    }

    /// <summary>
    /// Adds a tactic to the end of the timeline.
    /// </summary>

    public void AddTactic(TacticInstance tactic)
    {
        if (tactic == null) return;

        if (!activeTactics.Contains(tactic))
        {
            activeTactics.Add(tactic);
            tactic.myBar = this;
            tactic.SetupTargeting(alignment == BarAlignment.Left);

            UpdateVisualLayout();
        }
    }

    /// <summary>
    /// Inserts a tactic at a specific index. Perfect for Drag & Drop reordering!
    /// </summary>
    public void InsertTactic(int index, TacticInstance tactic)
    {
        if (tactic == null) return;

        // If it's already in the bar, remove it first so we can re-insert it at the new position
        if (activeTactics.Contains(tactic))
        {
            activeTactics.Remove(tactic);
        }

        // Clamp the index safely so we don't get out of bounds errors
        index = Mathf.Clamp(index, 0, activeTactics.Count);

        activeTactics.Insert(index, tactic);
        tactic.myBar = this;
        UpdateVisualLayout();
    }

    /// <summary>
    /// Removes a tactic from the bar and closes the gap.
    /// </summary>
    public void RemoveTactic(TacticInstance tactic, bool destroyVisual = true)
    {
        if (activeTactics.Contains(tactic))
        {
            activeTactics.Remove(tactic);

            if (destroyVisual && tactic != null)
            {
                Destroy(tactic.gameObject);
            }

            UpdateVisualLayout(); // Automatically closes the gap!
        }
    }

    public void RefreshAllTacticAuras()
    {
        foreach (var tactic in activeTactics)
        {
            if (tactic != null && tactic.isPassive)
            {
                tactic.RemovePassiveEffect();
                tactic.ApplyPassiveEffect();
            }
        }
    }

    public void UpdateVisualLayout()
    {
        int count = activeTactics.Count;
        if (count == 0) return;

        // 1. Auto-Size: Determine the actual spacing to use
        float currentSpacing = defaultSpacing;
        float totalWidth = (count - 1) * currentSpacing;

        if (totalWidth > maxBarWidth)
        {
            currentSpacing = maxBarWidth / (count > 1 ? count - 1 : 1);
        }

        Vector2 anchorPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            float x = anchorPos.x;
            float y = anchorPos.y; // Start with the anchor's default Y

            // ALIGNMENT (X-Axis)
            if (alignment == BarAlignment.Center)
            {
                float halfW = totalWidth * 0.5f;
                x = anchorPos.x + (i * currentSpacing) - halfW;
            }
            else if (alignment == BarAlignment.Left)
            {
                x = anchorPos.x + (i * currentSpacing);
            }
            else if (alignment == BarAlignment.Right)
            {
                x = anchorPos.x - (i * currentSpacing);
            }

            // STAGGER (Y-Axis): If it is an odd number (1, 3, 5), drop it down!
            if (useVerticalStagger && i % 2 != 0)
            {
                y += verticalStaggerAmount;
            }

            Vector2 targetPos = new Vector2(x, y) + tacticVisualOffset;

            if (activeTactics[i] != null)
            {
                if (!activeTactics[i].isDragging)
                {
                    activeTactics[i].transform.position = targetPos;
                }
            }
        }

    }

    public int GetInsertIndexFromPosition(Vector3 worldPosition)
    {
        int count = activeTactics.Count;
        if (count == 0) return 0;

        float currentSpacing = (count - 1) * defaultSpacing > maxBarWidth
            ? maxBarWidth / (count > 1 ? count - 1 : 1)
            : defaultSpacing;

        float startX = transform.position.x;

        if (alignment == BarAlignment.Center)
        {
            float halfW = (count - 1) * currentSpacing * 0.5f;
            startX = transform.position.x - halfW;
        }

        // Calculate distance from the anchor
        float localX = worldPosition.x - startX;

        // If right aligned, the bar grows to the left, so we invert the distance
        if (alignment == BarAlignment.Right)
        {
            localX = startX - worldPosition.x;
        }

        // Using RoundToInt instead of FloorToInt ensures dropping between two items feels natural
        int index = Mathf.RoundToInt(localX / currentSpacing);

        return Mathf.Clamp(index, 0, count);
    }

    public List<TacticInstance> GetAllTactics()
    {
        return new List<TacticInstance>(activeTactics);
    }

    public void ClearAllTactics()
    {
        // Loop backwards when destroying objects in a list
        for (int i = activeTactics.Count - 1; i >= 0; i--)
        {
            RemoveTactic(activeTactics[i], true);
        }
    }
}
