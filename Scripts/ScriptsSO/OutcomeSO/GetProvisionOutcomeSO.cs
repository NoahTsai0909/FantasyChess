using UnityEngine;
[CreateAssetMenu(menuName = "Outcomes/Gain Provision")]

public class GetProvisionOutcomeSO : EventOutcomeSO
{
    public int provisionAmount;

    public override void ExecuteOutcome(EventContext context)
    {
        RunManager.Instance.Stats.ProvisionCap += provisionAmount;
    }
}