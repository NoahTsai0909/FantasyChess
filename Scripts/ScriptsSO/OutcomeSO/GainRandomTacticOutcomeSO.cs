using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Gain Random Tactic")]
public class GainRandomTacticOutcomeSO : EventOutcomeSO
{
    public override void ExecuteOutcome(EventContext context)
    {
        // 1. If the UI generated a specific preview tactic for the button, give them EXACTLY what they saw!
        if (context != null && context.generatedTactic != null)
        {
            PlayerTacticManager.Instance.TryAcquireTactic(context.generatedTactic.definition, context.generatedTactic.rarity);
        }
        else
        {
            // 2. Otherwise, generate a completely random one from the pool
            var randomTacticData = TacticGenerationService.GenerateTactic();
            if (randomTacticData != null && randomTacticData.definition != null)
            {
                PlayerTacticManager.Instance.TryAcquireTactic(randomTacticData.definition, randomTacticData.rarity);
            }
        }
    }
}
