using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Gain Specific Tactic")]
public class GainSpecificTacticOutcomeSO : EventOutcomeSO
{
    [Tooltip("Fallback definition if the event choice doesn't provide a preview tactic")]
    public TacticDefinition fallbackTactic;

    public override void ExecuteOutcome(EventContext context)
    {
        // 1. If the UI generated a specific preview, give them EXACTLY what they saw!
        if (context != null && context.generatedTactic != null)
        {
            PlayerTacticManager.Instance.TryAcquireTactic(context.generatedTactic.definition, context.generatedTactic.rarity);
        }
        // 2. Fallback logic if there was no preview tactic assigned to the button
        else if (fallbackTactic != null)
        {
            // Even the fallback uses the Generation Service to guarantee accurate rarities!
            var fallbackData = TacticGenerationService.GenerateTactic();
            if (fallbackData != null)
            {
                // Override the definition, but keep the safely calculated rarity
                PlayerTacticManager.Instance.TryAcquireTactic(fallbackTactic, fallbackData.rarity);
            }
        }
    }
}
