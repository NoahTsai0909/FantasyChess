using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RunManager;
using static SceneLoader;

public class ShopSceneController : MonoBehaviour
{
    [SerializeField] private Transform unitAnchor;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button PrepSceneButton;
    [SerializeField] private float horizontalSpacing = 5f;
    [SerializeField] private Button purchaseButtonPrefab;
    [SerializeField] private Transform shopUIAnchor;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private Button continueButton;

    private ShopEventSO shopEvent;
    private ShopState shopState;

    private List<UnitInstance> spawnedUnits = new();
    private List<Button> spawnedButtons = new();

    void Start()
    {
        shopEvent = RunManager.Instance.selectedEvent as ShopEventSO;
        if (shopEvent == null)
        {
            Debug.LogError("ShopScene loaded without ShopEventSO");
            return;
        }

        RunManager.Instance.InitializeShop(
            shopEvent.totalUnitsGenerated,
            shopEvent.region,
            shopEvent.allowedTags,
            shopEvent.minProvisionCost,    
            shopEvent.maxProvisionCost
        );

        shopState = RunManager.Instance.shopState;

        goldText.text = $"Gold: {RunManager.Instance.Stats.CurrentGold}";

        continueButton.onClick.AddListener(() =>
        {
            CompleteEventAndReturn(shopEvent);
        });

        SetupRefreshButton();
        SetupPrepSceneButton();
        DisplayCurrentPage();
    }

    void SetupRefreshButton()
    {
        refreshButton.gameObject.SetActive(!shopState.hasRefreshed);
        refreshButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Refresh ({shopEvent.refreshCost}g)";

        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(() =>
        {
            if (RunManager.Instance.Stats.CurrentGold < shopEvent.refreshCost)
            {
                Debug.Log($"Not enough gold to refresh! Need {shopEvent.refreshCost}g, have {RunManager.Instance.Stats.CurrentGold}g");
                return;
            }

            RunManager.Instance.Stats.CurrentGold -= shopEvent.refreshCost;
            shopState.hasRefreshed = true;

            shopState.currentPage = 1;
            goldText.text = $"Gold: {RunManager.Instance.Stats.CurrentGold}";

            refreshButton.gameObject.SetActive(false);
            DisplayCurrentPage();
        });
    }

    void SetupPrepSceneButton()
    {
        PrepSceneButton.onClick.AddListener(() => SceneLoader.Instance.LoadScene(GameScene.PrepScene));
    }

    void SpawnShopUnit(UnitSaveData unitData, float xPos)
    {
        // Spawn unit preview
        UnitInstance unit = Instantiate(unitData.definition.unitPrefab, unitAnchor);
        unit.InitializeFromSaveData(unitData);
        unit.transform.localPosition = new Vector3(xPos, 0f, 0f);

        // Force correct scale (adjust these values based on your grid cell size)
        unit.transform.localScale = new Vector3(1f, 1f, 1f); // Match PrepScene scale

        unit.isPlayer = true;
        spawnedUnits.Add(unit);

        // Spawn purchase button
        SpawnPurchaseButton(unit, unitData, unit.transform.position);
    }

    void SpawnPurchaseButton(UnitInstance unit, UnitSaveData unitData, Vector3 unitWorldPosition)
    {
        Button button = Instantiate(purchaseButtonPrefab, shopUIAnchor);

        // Position button in world space (below the unit)
        button.transform.position = unitWorldPosition + new Vector3(0f, -4f, 0f);

        // Make sure the button faces the camera
        button.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        int unitCost = GetPurchasePrice(unitData);
        // Update button text
        var text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
            text.text = $"Buy ({unitCost}g)";

        // Hook up click logic
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            if (RunManager.Instance.Stats.CurrentGold < unitCost)
            {
                Debug.Log("Not enough gold");
                return;
            }

            RunManager.Instance.Stats.CurrentGold -= unitCost;
            PlayerUnitManager.Instance.TryAcquireUnit(unitData.definition, unitData.rarity); // Pass rarity!
            goldText.text = $"Gold: {RunManager.Instance.Stats.CurrentGold}";
            shopState.purchasedUnits.Add(unitData.definition);

            spawnedUnits.Remove(unit);
            spawnedButtons.Remove(button);

            Destroy(unit.gameObject);
            Destroy(button.gameObject);
        });

        spawnedButtons.Add(button);
    }

    int GetPurchasePrice(UnitSaveData unit)
    {
        int discountValue = 0;
        if (shopEvent.discount)
        {
            discountValue = RarityToMultiplier(unit.rarity); 
        }

        return (unit.EffectiveValue * 2) - discountValue;
    }

    public static int RarityToMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 1;
            case Rarity.Uncommon: return 2;
            case Rarity.Rare: return 3;
            case Rarity.Epic: return 4;
            default: return 1;
        }
    }

    void DisplayCurrentPage()
    {
        ClearSpawnedUnits();

        // Get units for current page, excluding purchased ones
        var pageUnits = shopState.offeredUnits
            .Skip(shopState.currentPage * shopEvent.unitsPerPage)
            .Take(shopEvent.unitsPerPage)
            .Where(u => !shopState.purchasedUnits.Contains(u.definition)) // Check definition
            .ToList();

        int count = pageUnits.Count;
        float startX = -(count - 1) * horizontalSpacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            SpawnShopUnit(pageUnits[i], startX + i * horizontalSpacing);
        }
    }

    void ClearSpawnedUnits()
    {
        foreach (var unit in spawnedUnits)
        {
            if (unit != null)
                Destroy(unit.gameObject);
        }

        foreach (var button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        spawnedUnits.Clear();
        spawnedButtons.Clear();
    }

    private void CompleteEventAndReturn(BaseEventSO eventSO)
    {
        // Mark event as completed
        eventSO.OnCompleted();

        // Return to map
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
