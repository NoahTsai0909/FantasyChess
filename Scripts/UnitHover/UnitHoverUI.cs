using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class UnitHoverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI abilityText;
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);

    [SerializeField] private Sprite backgroundCommon;
    [SerializeField] private Sprite backgroundUncommon;
    [SerializeField] private Sprite backgroundRare;
    [SerializeField] private Sprite backgroundEpic;

    [SerializeField] private StatWidget statWidgetPrefab;
    [SerializeField] private Transform statsContainer;

    [SerializeField] private Sprite attackIcon;
    [SerializeField] private Sprite shieldIcon;
    [SerializeField] private Sprite healIcon;
    [SerializeField] private Sprite poisonIcon;
    [SerializeField] private Sprite burnIcon;


    private Canvas canvas;
    private RectTransform rect;
    private Image image;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    void Update()
    {
        FollowMouse();
    }

    public void Show(UnitInstance unit)
    {
        if (unit == null || unit.Definition == null)
            return;

        unit.RecalculateStats();

        gameObject.SetActive(true);

        nameText.text = unit.Definition.unitName;

        foreach (Transform child in statsContainer)
            Destroy(child.gameObject);

        StatBlock stats = unit.Stats;

        if (stats.Attack > 0)
            AddStatWidget(attackIcon, stats.Attack);

        if (stats.Shield > 0)
            AddStatWidget(shieldIcon, stats.Shield);

        if (stats.Heal > 0)
            AddStatWidget(healIcon, stats.Heal);

        if (stats.Poison > 0)
            AddStatWidget(poisonIcon, stats.Poison);

        if (stats.Burn > 0)
            AddStatWidget(burnIcon, stats.Burn);


        SetRarityBackground(unit.CurrentRarity);
        abilityText.text = unit.GetAbilityDescription();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void FollowMouse()
    {
        Vector2 mouse = Mouse.current.position.ReadValue();

        Vector2 size = rect.rect.size;

        float x = mouse.x + padding.x;
        float y = mouse.y + padding.y;

        if (x + size.x > Screen.width)
            x = mouse.x - size.x - padding.x;

        if (y + size.y > Screen.height)
            y = mouse.y - size.y - padding.y;

        rect.position = new Vector2(x, y);
    }

    private void SetRarityBackground(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                image.sprite = backgroundCommon;
                break;

            case Rarity.Uncommon:
                image.sprite = backgroundUncommon;
                break;

            case Rarity.Rare:
                image.sprite = backgroundRare;
                break;

            case Rarity.Epic:
                image.sprite = backgroundEpic;
                break;

            default:
                image.sprite = backgroundCommon;
                break;
        }
    }

    void AddStatWidget(Sprite icon, int value)
    {
        StatWidget widget = Instantiate(statWidgetPrefab, statsContainer);
        widget.Set(icon, value);
    }


}

