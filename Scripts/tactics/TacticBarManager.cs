using System.Collections.Generic;
using UnityEngine;

public class TacticBarManager : MonoBehaviour
{
    [Header("Bar Settings")]
    [Tooltip("The standard distance between tactics")]
    public float defaultSpacing = 3f;

    [Tooltip("The maximum physical width the bar is allowed to take up on screen before it starts squishing tactics closer together")]
    public float maxBarWidth = 24f;

    [Header("Visuals")]
    public Vector2 tacticVisualOffset = new Vector2(0f, 0.5f);

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
            tactic.myBar = this; // Assumes TacticInstance has a reference to the bar
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

    /// <summary>
    /// Dynamically calculates positions to center the bar and squish items if needed.
    /// </summary>
    public void UpdateVisualLayout()
    {
        int count = activeTactics.Count;
        if (count == 0) return;

        // 1. Auto-Size: Determine the actual spacing to use
        float currentSpacing = defaultSpacing;
        float totalWidth = (count - 1) * currentSpacing;

        // If the tactics exceed the visual boundaries, squish them together
        if (totalWidth > maxBarWidth)
        {
            currentSpacing = maxBarWidth / (count > 1 ? count - 1 : 1);
        }

        // 2. Calculate the starting X position to keep the bar perfectly centered
        float halfW = (count - 1) * currentSpacing * 0.5f;
        Vector2 center = transform.position;

        // 3. Apply positions
        for (int i = 0; i < count; i++)
        {
            float x = center.x + (i * currentSpacing) - halfW;
            Vector2 targetPos = new Vector2(x, center.y) + tacticVisualOffset;

            if (activeTactics[i] != null)
            {
                // Note: You can replace this with a Coroutine/Lerp in the future for smooth sliding!
                activeTactics[i].transform.position = targetPos;
            }
        }
    }

    /// <summary>
    /// Used by DragAndDropManager. Calculates which index the player is trying to drop the tactic into based on mouse X.
    /// </summary>
    public int GetInsertIndexFromPosition(Vector3 worldPosition)
    {
        int count = activeTactics.Count;
        if (count == 0) return 0;

        // Calculate what spacing is currently being used
        float currentSpacing = (count - 1) * defaultSpacing > maxBarWidth
            ? maxBarWidth / (count - 1)
            : defaultSpacing;

        float halfW = (count - 1) * currentSpacing * 0.5f;
        float startX = transform.position.x - halfW;

        // Find exactly how far along the bar the mouse is
        float localX = worldPosition.x - (startX - currentSpacing * 0.5f);
        int index = Mathf.FloorToInt(localX / currentSpacing);

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
