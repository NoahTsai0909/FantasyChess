using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LootSummaryUI : MonoBehaviour
{
    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI xpText;

    [Header("Reward Display")]
    [SerializeField] private Transform rewardAnchor;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private GameObject rewardContainer;

    private UnitDefinition pendingUnit;
    private Rarity pendingUnitRarity;
    private TacticDefinition pendingTactic;
    private Rarity pendingTacticRarity;

    // Track the specific component types just like EventSceneController
    private UnitInstance spawnedUnitPreview;
    private TacticInstance spawnedTacticPreview;

    public void ShowSummary(int gold, int xp, UnitDefinition unitDef, Rarity uRarity, TacticDefinition tacticDef, Rarity tRarity)
    {
        gameObject.SetActive(true);
        goldText.SetText(TextIconUtility.ParseDescription($"+ [GOLD] {gold}"));
        xpText.text = $"+{xp} XP";


        pendingUnit = unitDef;
        pendingUnitRarity = uRarity;
        pendingTactic = tacticDef;
        pendingTacticRarity = tRarity;


        // Clean up any old previews
        ClearPreviews();

        // Spawn the preview and hook up buttons
        if (pendingUnit != null || pendingTactic != null)
        {
            rewardContainer.SetActive(true);

            if (pendingUnit != null)
                SpawnUnitPreview(pendingUnit, pendingUnitRarity);
            else if (pendingTactic != null)
                SpawnTacticPreview(pendingTactic, pendingTacticRarity);

            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(AcceptReward);

            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(CloseSummary);
        }
        else
        {
            rewardContainer.SetActive(false);
        }
    }

    private void SpawnUnitPreview(UnitDefinition def, Rarity rarity)
    {
        spawnedUnitPreview = Instantiate(def.unitPrefab, rewardAnchor);

        // Mimicking EventSceneController logic
        UnitSaveData mockData = new UnitSaveData { definition = def, rarity = rarity };
        spawnedUnitPreview.InitializeFromSaveData(mockData);

        spawnedUnitPreview.isPlayer = true;
        spawnedUnitPreview.enabled = false; // Stop logic ticks

        spawnedUnitPreview.transform.localPosition = Vector3.zero;
        spawnedUnitPreview.transform.localScale = Vector3.one * 30f;

        if (spawnedUnitPreview.Visuals != null)
            spawnedUnitPreview.Visuals.SetBaseScale(spawnedUnitPreview.transform.localScale);

        SpriteRenderer renderer = spawnedUnitPreview.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder = 100;
    }

    private void SpawnTacticPreview(TacticDefinition def, Rarity rarity)
    {
        spawnedTacticPreview = Instantiate(def.tacticPrefab, rewardAnchor);

        // Mimicking EventSceneController logic
        RunManager.TacticSaveData mockData = new RunManager.TacticSaveData { definition = def, rarity = rarity };
        spawnedTacticPreview.InitializeFromSaveData(mockData);

        spawnedTacticPreview.isPlayer = true;
        spawnedTacticPreview.enabled = false;

        spawnedTacticPreview.transform.localPosition = Vector3.zero;
        spawnedTacticPreview.transform.localScale = Vector3.one * 50f;

        Canvas canvas = spawnedTacticPreview.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
        }
    }

    private void AcceptReward()
    {
        if (pendingUnit != null)
            PlayerUnitManager.Instance.TryAcquireUnit(pendingUnit, pendingUnitRarity);
        else if (pendingTactic != null)
            PlayerTacticManager.Instance.TryAcquireTactic(pendingTactic, pendingTacticRarity);

        CloseSummary();
    }

    private void CloseSummary()
    {
        ClearPreviews();
        gameObject.SetActive(false);
    }

    private void ClearPreviews()
    {
        if (spawnedUnitPreview != null) Destroy(spawnedUnitPreview.gameObject);
        if (spawnedTacticPreview != null) Destroy(spawnedTacticPreview.gameObject);
    }
}