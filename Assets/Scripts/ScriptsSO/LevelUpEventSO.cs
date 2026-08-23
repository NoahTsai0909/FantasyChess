using UnityEngine;
using static SceneLoader;

[CreateAssetMenu(fileName = "NewLevelUpEvent", menuName = "Game/Progression/Level Up Event")]
public class LevelUpEventSO : StoryEventSO
{
    [Header("Progression Math")]
    [Tooltip("The total XP required to trigger this specific level up.")]
    public int xpRequired = 10;

    public override void OnCompleted()
    {
        if (RunManager.Instance != null)
        {
            RunManager.Instance.eventInProgress = false;
            RunManager.Instance.selectedEvent = null;
        }

        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
