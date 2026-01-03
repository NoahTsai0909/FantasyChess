using UnityEngine;

[CreateAssetMenu(menuName = "Events/Gold Event")]
public class GoldEventSO : BaseEventSO
{
    public int goldAmount;

    public override int getGoldAmount()
    {
        return goldAmount;
    }
}
