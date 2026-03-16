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
            shopEvent.allowedTags
        );

        shopState = RunManager.Instance.shopState;

        goldText.text = $"Gold: {RunManager.Instance.currentGold}";

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

        refreshButton.onClick.RemoveAllListeners();
        refreshButton.onClick.AddListener(() =>
        {
            if (RunManager.Instance.currentGold < 2)
            {
                Debug.Log($"Not enough gold, player only has {RunManager.Instance.currentGold} gold");
                return;
            }

            RunManager.Instance.currentGold -= 2;
            shopState.hasRefreshed = true;
            shopState.currentPage = 1;
            goldText.text = $"Gold: {RunManager.Instance.currentGold}";

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
            if (RunManager.Instance.currentGold < unitCost)
            {
                Debug.Log("Not enough gold");
                return;
            }

            RunManager.Instance.currentGold -= unitCost;
            PlayerUnitManager.Instance.TryAcquireUnit(unitData.definition, unitData.rarity); // Pass rarity!
            goldText.text = $"Gold: {RunManager.Instance.currentGold}";
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
        return unit.EffectiveValue * 2;
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
        eventSO.CompleteEvent();

        // Return to map
        SceneLoader.Instance.LoadScene(GameScene.MapScene);
    }
}
