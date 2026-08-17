#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class DatabaseUpdater : EditorWindow
{
    // This creates a new button in the Unity menu bar!
    [MenuItem("Tools/Update All Databases")]
    public static void UpdateDatabases()
    {
        UpdateTacticDatabase();
        UpdateUnitDatabase();

        // Force Unity to save the changes to the disk
        AssetDatabase.SaveAssets();

        Debug.Log("<color=green><b>Successfully updated all databases!</b></color>");
    }

    private static void UpdateTacticDatabase()
    {
        // 1. Find the Tactic Database asset in your project
        string[] dbGuids = AssetDatabase.FindAssets("t:TacticDatabase");
        if (dbGuids.Length == 0)
        {
            Debug.LogWarning("Could not find a TacticDatabase in the project.");
            return;
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
        TacticDatabase tacticDB = AssetDatabase.LoadAssetAtPath<TacticDatabase>(dbPath);

        // 2. Find ALL TacticDefinitions in the entire project
        string[] defGuids = AssetDatabase.FindAssets("t:TacticDefinition");
        List<TacticDefinition> foundTactics = new List<TacticDefinition>();

        foreach (string guid in defGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TacticDefinition def = AssetDatabase.LoadAssetAtPath<TacticDefinition>(path);

            // Optional: If you add an 'isDraft' boolean to your definitions, you can exclude them here!
            // if (def.isDraft) continue;

            foundTactics.Add(def);
        }

        // 3. Assign the list and mark the database as "Dirty" (meaning it needs to be saved)
        tacticDB.allTactics = foundTactics;
        EditorUtility.SetDirty(tacticDB);

        Debug.Log($"Found and added {foundTactics.Count} Tactics to the TacticDatabase.");
    }

    private static void UpdateUnitDatabase()
    {
        // 1. Find the Unit Database asset
        string[] dbGuids = AssetDatabase.FindAssets("t:UnitDatabase");
        if (dbGuids.Length == 0)
        {
            Debug.LogWarning("Could not find a UnitDatabase in the project.");
            return;
        }

        string dbPath = AssetDatabase.GUIDToAssetPath(dbGuids[0]);
        UnitDatabase unitDB = AssetDatabase.LoadAssetAtPath<UnitDatabase>(dbPath);

        // 2. Find ALL UnitDefinitions
        string[] defGuids = AssetDatabase.FindAssets("t:UnitDefinition");
        List<UnitDefinition> foundUnits = new List<UnitDefinition>();

        foreach (string guid in defGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            UnitDefinition def = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            foundUnits.Add(def);
        }

        // 3. Assign and mark dirty
        unitDB.allUnits = foundUnits;
        EditorUtility.SetDirty(unitDB);

        Debug.Log($"Found and added {foundUnits.Count} Units to the UnitDatabase.");
    }
}
#endif
