using UnityEngine;

[CreateAssetMenu(menuName = "Events/Conditions/Experience Condition")]
public class ExperienceConditionSO : ChoiceConditionSO
{
    public int requiredExperience = 1;

    public override bool IsMet()
    {
        // Replace this with wherever you actually store your Run's Experience!
        return RunManager.Instance.Stats.Experience >= requiredExperience;
    }

    public override string GetRequirementText()
    {
        // This will print: "Requires 1 Experience"
        return $"Requires {requiredExperience} Experience";
    }
}
