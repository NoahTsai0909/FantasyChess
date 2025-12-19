using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopEvent", menuName = "Events/Shop Event")]
public class ShopEventSO : BaseEventSO
{
    [Header("Shop Rules")]
    public int totalUnitsGenerated = 6;
    public int unitsPerPage = 3;
    public UnitTagFlags allowedTags = UnitTagFlags.None;
    public Region region = Region.Aurelia;

    public override void OnCompleted()
    {
        RunManager.Instance.shopState = null;
        base.OnCompleted();
    }
}
