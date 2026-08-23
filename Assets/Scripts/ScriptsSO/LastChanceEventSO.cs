using UnityEngine;
using static SceneLoader;

[CreateAssetMenu(fileName = "NewLastChanceEvent", menuName = "Game/Progression/Last Chance Event")]
public class LastChanceEventSO : StoryEventSO
{
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
