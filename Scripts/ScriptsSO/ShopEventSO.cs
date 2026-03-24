using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopEvent", menuName = "Events/Shop Event")]
public class ShopEventSO : BaseEventSO
{
    [Header("Shop Rules")]
    public int totalUnitsGenerated = 6;
    public int unitsPerPage = 3;
    public int refreshCost = 2;  // Add this line
    public UnitTagFlags allowedTags = UnitTagFlags.None;
    public Region region = Region.Solmire;

    [Header("Provision Filtering")]
    public int maxProvisionCost = -1;  // -1 means no filter, will add later
    public int minProvisionCost = 0;

    public override void OnCompleted()
    {
        RunManager.Instance.shopState = null;
        base.OnCompleted();
    }
}
