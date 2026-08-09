using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Open Unit Selector")]
public class OpenUnitSelectorOutcomeSO : EventOutcomeSO
{
    [Tooltip("The effect that will happen to the unit the player clicks.")]
    public UnitTargetEffectSO effectToApply;

    [Tooltip("OPTIONAL: An outcome to trigger ONLY IF a unit is successfully selected (e.g., Gain Ironmoon Cultist).")]
    public EventOutcomeSO onSuccessOutcome;
    public override void ExecuteOutcome(EventContext context)
    {
        if (context != null && context.uiController != null)
        {
            // Keep the event open so it doesn't transition back to the map
            context.keepEventOpen = true;

            // Pass the context and success outcome to the UI
            context.uiController.ShowUnitSelectorPanel(effectToApply, onSuccessOutcome, context);
        }
        else
        {
            Debug.LogError("OpenUnitSelectorOutcomeSO failed: UI Controller reference is missing!");
        }
    }
}
