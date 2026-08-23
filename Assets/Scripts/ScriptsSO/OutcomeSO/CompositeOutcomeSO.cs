using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Composite Outcome")]
public class CompositeOutcomeSO : EventOutcomeSO
{
    [Tooltip("Executes all of these outcomes in order from top to bottom.")]
    public List<EventOutcomeSO> outcomes = new List<EventOutcomeSO>();

    public override void ExecuteOutcome(EventContext context)
    {
        foreach (var outcome in outcomes)
        {
            if (outcome != null)
            {
                outcome.ExecuteOutcome(context);
            }
        }
    }
}
