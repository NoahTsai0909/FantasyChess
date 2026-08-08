using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Chance Outcome")]
public class ChanceOutcomeSO : EventOutcomeSO
{
    [Range(0f, 1f)]
    [Tooltip("1.0 = 100% success, 0.5 = 50%, 0.1 = 10%")]
    public float successChance = 0.5f;

    [Header("Results")]
    public EventOutcomeSO successOutcome;
    public EventOutcomeSO failureOutcome;

    public override void ExecuteOutcome(EventContext context)
    {
        float roll = Random.value; // Generates a float between 0.0 and 1.0

        if (roll <= successChance)
        {
            if (successOutcome != null) successOutcome.ExecuteOutcome(context);
        }
        else
        {
            if (failureOutcome != null) failureOutcome.ExecuteOutcome(context);
        }
    }
}
