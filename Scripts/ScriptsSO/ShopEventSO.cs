using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopEvent", menuName = "Events/Shop Event")]
public class ShopEventSO : BaseEventSO
{
    [Header("Shop Rules")]
    public int totalUnitsGenerated = 6;
    public int totalTacticsGenerated = 0;
    public int unitsPerPage = 3;
    public int refreshCost = 2;  // Add this line
    public bool discount = false;
    public UnitTagFlags allowedTags = UnitTagFlags.None;
    public Region region;

    [Header("Provision Filtering")]
    public int maxProvisionCost = -1;  // -1 means no filter, will add later
    public int minProvisionCost = 0;

    [Header("Rarity Filtering")]
    public bool forceRarity = false;
    public Rarity designatedRarity = Rarity.Common;

    public override void OnCompleted()
    {
        RunManager.Instance.shopState = null;
        if (RunManager.Instance != null)
        {
            // This clears the old events and increments your 1/3 event counter
            RunManager.Instance.CompleteRegularEvent();
        }
        base.OnCompleted();
    }
}
