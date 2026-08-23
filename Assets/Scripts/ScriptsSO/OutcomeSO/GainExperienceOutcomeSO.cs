using UnityEngine;
[CreateAssetMenu(menuName = "Outcomes/Gain Experience")]
public class GainExperienceOutcomeSO : EventOutcomeSO
{
    public int EXPamount;
    public int optionalGold;

    public override void ExecuteOutcome(EventContext context)
    {
        if (RunManager.Instance.Stats.Experience + EXPamount < 0)
        {
            return;
        }
        RunManager.Instance.Stats.Experience += EXPamount;
        RunManager.Instance.Stats.CurrentGold += optionalGold;
    }
}
