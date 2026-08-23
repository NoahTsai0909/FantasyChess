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
        // 1. Do Combat specific setup
        RunManager.Instance.currentEncounter = encounter;
        targetScene = GameScene.CombatScene;

        // 2. Call base to log it and load the scene
        base.OnSelected();
    }

    public override void OnCompleted()
    {
        RunManager.Instance.CompleteBattleEvent();
        base.OnCompleted();
    }
}
