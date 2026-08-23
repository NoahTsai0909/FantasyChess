using UnityEngine;

public abstract class UnitTargetEffectSO : ScriptableObject
{
    // Takes the specific placement (bench or grid) so we know exactly where it lives
    public abstract void ApplyEffect(RunManager.UnitPlacement placement);
}
