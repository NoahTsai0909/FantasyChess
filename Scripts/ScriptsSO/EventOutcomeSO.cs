using UnityEngine;

// NEW: A container to pass data from the UI to the Outcome
public class EventContext
{
    public UnitSaveData generatedUnit;
}

public abstract class EventOutcomeSO : ScriptableObject
{
    // Modify this to accept the context
    public abstract void ExecuteOutcome(EventContext context);

    public virtual bool CanAfford() { return true; }
}