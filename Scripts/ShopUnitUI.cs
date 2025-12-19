using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ShopUnitUI : MonoBehaviour, IPointerClickHandler
{
    private Action onPurchase;

    public void Setup(Action purchaseCallback)
    {
        onPurchase = purchaseCallback;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Unit clicked!");
        onPurchase?.Invoke();
    }
}

