using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class UnitStatBlockUI : MonoBehaviour
{
    [SerializeField] private Image unitIcon;
    [SerializeField] private TextMeshProUGUI statsText;

    public void Setup(UnitLifetimeStats stats, Sprite sprite, bool isMVP)
    {
        if (unitIcon != null && sprite != null)
        {
            unitIcon.sprite = sprite;
        }

        StringBuilder sb = new StringBuilder();

        // Add a special header for the MVP!
        if (isMVP)
        {
            sb.AppendLine("<color=#FFD700><b>MVP</b></color>");
        }
        sb.AppendLine($"<b>{stats.unitName}</b>");

        AppendStat(sb, "Total damage dealt", stats.totalDirectDamageDealt);
        AppendStat(sb, "Total burn damage dealt", stats.totalBurnDamageDealt);
        AppendStat(sb, "Total poison damage dealt", stats.totalPoisonDamageDealt);
        AppendStat(sb, "Total damage taken", stats.totalDamageTaken);
        AppendStat(sb, "Total healing done", stats.totalHealingDone);
        AppendStat(sb, "Total shielding done", stats.totalShieldingDone);
        AppendStat(sb, "Total slows applied", stats.totalSlowsApplied);
        AppendStat(sb, "Total hastes applied", stats.totalHastesApplied);
        AppendStat(sb, "Total advances given", stats.totalAdvancesGiven);

        if (statsText != null)
        {
            statsText.text = sb.ToString();
        }
    }

    private void AppendStat(StringBuilder sb, string label, int value)
    {
        if (value > 0)
        {
            sb.AppendLine($"{label}: {FormatNumber(value)}");
        }
    }

    private string FormatNumber(int num)
    {
        // Converts 14100 to "14.1k"
        if (num >= 1000)
        {
            return (num / 1000f).ToString("0.#") + "k";
        }
        return num.ToString();
    }
}
