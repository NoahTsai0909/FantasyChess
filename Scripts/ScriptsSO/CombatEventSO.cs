using UnityEngine;
using static SceneLoader;

[CreateAssetMenu(fileName = "CombatEvent", menuName = "Events/Combat Event")]
public class CombatEventSO : BaseEventSO
{
    [Header("Combat Settings")]
    public EncounterDefinition encounter;
    public int goldReward;
    public int reputationReward;

    public override void OnSelected()
    {
        // Store the encounter BEFORE calling base
        RunManager.Instance.currentEncounter = encounter;
        Debug.Log($"Combat event selected: {eventName}, Encounter: {encounter?.encounterName}");

        // Make sure targetScene is CombatScene
        targetScene = GameScene.CombatScene; // Force it to be combat scene

        // Now call base - this will set selectedEvent and load scene
        base.OnSelected();
    }

}
