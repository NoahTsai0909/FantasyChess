using UnityEngine;

[CreateAssetMenu(menuName = "Outcomes/Change Page")]
public class ChangePageOutcomeSO : EventOutcomeSO
{
    public int targetPageIndex;

    public override void ExecuteOutcome(EventContext context)
    {
        if (context != null && context.uiController != null)
        {
            context.keepEventOpen = true; // Tell the UI we are staying here!
            context.uiController.LoadEventPage(targetPageIndex);
        }
        else
        {
            Debug.LogError("ChangePageOutcomeSO failed: UI Controller reference is missing!");
        }
    }
}
