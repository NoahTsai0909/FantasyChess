using UnityEngine;

public abstract class ChoiceConditionSO : ScriptableObject
{
    // Evaluates if the player meets the requirements
    public abstract bool IsMet();

    // The text to show the player if they CANNOT click the button
    public abstract string GetRequirementText();
}
