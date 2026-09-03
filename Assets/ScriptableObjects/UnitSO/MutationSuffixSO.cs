using UnityEngine;

public abstract class MutationSuffixSO : ScriptableObject
{
    public string suffixName; // e.g., "of Embers"

    public abstract void ExecuteEffect(UnitInstance caster);
    public abstract string GetActionPhrase(UnitInstance caster, bool capitalizeFirstLetter);
}
