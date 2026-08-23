using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TooltipEntry
{
    [Tooltip("The ID used in the <link=id> tag (e.g., 'burn' or 'poison'). Keep this lowercase!")]
    public string id;

    [Tooltip("The title displayed at the top of the tooltip.")]
    public string title;

    [TextArea(3, 5)]
    [Tooltip("The explanation of the mechanic.")]
    public string description;
}

[CreateAssetMenu(fileName = "TooltipDatabase", menuName = "Database/Tooltip Database")]
public class TooltipDatabaseSO : ScriptableObject
{
    public List<TooltipEntry> entries = new List<TooltipEntry>();

    private Dictionary<string, TooltipEntry> entryDictionary;

    /// <summary>
    /// Fetches a tooltip entry by its ID. 
    /// Converts the list to a dictionary on the first call for ultra-fast lookups.
    /// </summary>
    public TooltipEntry GetEntry(string id)
    {
        // Build the dictionary once for performance
        if (entryDictionary == null || entryDictionary.Count != entries.Count)
        {
            entryDictionary = new Dictionary<string, TooltipEntry>();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.id) && !entryDictionary.ContainsKey(entry.id))
                {
                    entryDictionary.Add(entry.id.ToLower(), entry);
                }
            }
        }

        // Return the entry if it exists
        if (entryDictionary.TryGetValue(id.ToLower(), out TooltipEntry foundEntry))
        {
            return foundEntry;
        }

        Debug.LogWarning($"TooltipDatabase: No entry found for ID '{id}'!");
        return null;
    }
}
