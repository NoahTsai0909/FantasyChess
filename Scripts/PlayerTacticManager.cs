using UnityEngine;
using System.Linq;

public class PlayerTacticManager : MonoBehaviour
{
    public static PlayerTacticManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TryAcquireTactic(TacticDefinition incomingDef, Rarity incomingRarity)
    {
        // 1. Try to Merge
        var mergeTarget = RunManager.Instance.playerTactics.FirstOrDefault(p =>
            p.tacticData != null &&
            p.tacticData.definition == incomingDef &&
            p.tacticData.rarity == incomingRarity &&
            p.tacticData.rarity < Rarity.Epic); // Stop at Epic

        if (mergeTarget != null)
        {
            // Upgrade the save data
            mergeTarget.tacticData.rarity += 1;
            Debug.Log($"Merged Tactic {incomingDef.tacticName} to {mergeTarget.tacticData.rarity}!");

            // If the player's Tactic Bar is currently on screen, visually upgrade the specific instance!
            TacticBarManager[] activeBars = FindObjectsByType<TacticBarManager>(FindObjectsSortMode.None);
            foreach (var bar in activeBars)
            {
                if (bar.alignment == TacticBarManager.BarAlignment.Left) // Left = Player Bar
                {
                    var instance = bar.GetAllTactics().FirstOrDefault(t => t.id == mergeTarget.tacticData.id);
                    if (instance != null) instance.UpgradeTier();
                }
            }
            return true;
        }

        // 2. Add New Tactic (Infinite Capacity)
        var newPlacement = new RunManager.TacticPlacement
        {
            tacticData = new RunManager.TacticSaveData
            {
                definition = incomingDef,
                rarity = incomingRarity
            },
            orderIndex = RunManager.Instance.playerTactics.Count
        };

        RunManager.Instance.playerTactics.Add(newPlacement);
        Debug.Log($"Acquired new Tactic: {incomingDef.tacticName}");

        // If the player's Tactic Bar is currently on screen, physically spawn it into the timeline!
        TacticBarManager[] activeBarsForSpawn = FindObjectsByType<TacticBarManager>(FindObjectsSortMode.None);
        foreach (var bar in activeBarsForSpawn)
        {
            if (bar.alignment == TacticBarManager.BarAlignment.Left)
            {
                // Instantiate the prefab stored in the definition
                TacticInstance newTactic = Instantiate(incomingDef.tacticPrefab);
                newTactic.InitializeFromSaveData(newPlacement.tacticData);
                newTactic.myPlacement = newPlacement;
                bar.AddTactic(newTactic);
            }
        }

        return true;
    }
}
