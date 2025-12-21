using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;


public class UnitHoverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);

    private Canvas canvas;
    private RectTransform rect;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        FollowMouse();
    }

    public void Show(UnitInstance unit)
    {
        if (unit == null || unit.Definition == null)
            return;

        gameObject.SetActive(true);

        nameText.text = unit.Definition.unitName;

        StatBlock stats = unit.Stats;

        statsText.text = $"HP: {stats.MaxHP}\n";

        if (stats.Attack > 0)
            statsText.text += $"ATK: {stats.Attack}\n";

        if (stats.Heal > 0)
            statsText.text += $"Heal: {stats.Heal}\n";

        if (!unit.Definition.isPassive)
            statsText.text += $"CD: {stats.Cooldown}s\n";

        statsText.text += $"Rarity: {unit.CurrentRarity}";
        abilityText.text = unit.GetAbilityDescription();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void FollowMouse()
    {
        if (Mouse.current == null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 size = rect.sizeDelta;

        Vector2 pivot = new Vector2(0f, 0f); // bottom-left default

        // If expanding UP would clip (mouse near top)
        if (mousePos.y + size.y > Screen.height)
        {
            pivot.y = 1f; // anchor at top, expand downward
        }

        // If expanding RIGHT would clip (mouse near right edge)
        if (mousePos.x + size.x > Screen.width)
        {
            pivot.x = 1f; // anchor at right, expand left
        }

        rect.pivot = pivot;
        rect.position = mousePos + padding;

    }
}

