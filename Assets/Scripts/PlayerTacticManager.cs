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
        Rarity finalRarity = incomingRarity;

        var existingTactic = RunManager.Instance.playerTactics.FirstOrDefault(p =>
            p.tacticData != null &&
            p.tacticData.definition == incomingDef);

        if (existingTactic != null)
        {
            finalRarity = existingTactic.tacticData.rarity;
        }

        var mergeTarget = RunManager.Instance.playerTactics.FirstOrDefault(p =>
            p.tacticData != null &&
            p.tacticData.definition == incomingDef &&
            p.tacticData.rarity == finalRarity && 
            p.tacticData.rarity < Rarity.Epic);

        if (mergeTarget != null)
        {
            mergeTarget.tacticData.rarity += 1;
            Debug.Log($"Merged Tactic {incomingDef.tacticName} to {mergeTarget.tacticData.rarity}!");

            TacticBarManager[] activeBars = FindObjectsByType<TacticBarManager>(FindObjectsSortMode.None);
            foreach (var bar in activeBars)
            {
                if (bar.alignment == TacticBarManager.BarAlignment.Left)
                {
                    var instance = bar.GetAllTactics().FirstOrDefault(t => t.id == mergeTarget.tacticData.id);
                    if (instance != null) instance.UpgradeTier();
                }
            }
            return true;
        }

        var newPlacement = new RunManager.TacticPlacement
        {
            tacticData = new RunManager.TacticSaveData
            {
                definition = incomingDef,
                rarity = finalRarity
            },
            orderIndex = RunManager.Instance.playerTactics.Count
        };

        RunManager.Instance.playerTactics.Add(newPlacement);
        Debug.Log($"Acquired new Tactic: {incomingDef.tacticName}");

        TacticBarManager[] activeBarsForSpawn = FindObjectsByType<TacticBarManager>(FindObjectsSortMode.None);
        foreach (var bar in activeBarsForSpawn)
        {
            if (bar.alignment == TacticBarManager.BarAlignment.Left)
            {
                TacticInstance newTactic = Instantiate(incomingDef.tacticPrefab);
                newTactic.InitializeFromSaveData(newPlacement.tacticData);
                newTactic.myPlacement = newPlacement;
                bar.AddTactic(newTactic);
            }
        }

        return true;
    }
}
