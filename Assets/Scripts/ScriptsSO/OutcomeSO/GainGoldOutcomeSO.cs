using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Gain Gold")]
public class GainGoldOutcomeSO : EventOutcomeSO
{
    public int amount;

    public override void ExecuteOutcome(EventContext context)
    {
        RunManager.Instance.Stats.CurrentGold += amount;
    }
}
