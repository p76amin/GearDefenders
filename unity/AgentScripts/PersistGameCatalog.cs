using UnityEditor;
using UnityEngine;
using GearDefenders;

public static class PersistGameCatalog
{
    public static string Run()
    {
        const string root = "Assets/Data";
        EnsureFolder(root);
        EnsureFolder(root + "/UnitDefinitions");
        EnsureFolder(root + "/BoostDefinitions");
        EnsureFolder(root + "/EnemyDefinitions");
        EnsureFolder(root + "/RoundDefinitions");

        var catalog = RuntimeCatalogFactory.Create();
        catalog.board = Save(catalog.board, root + "/BoardConfig.asset");

        for (int i = 0; i < catalog.shopUnits.Length; i++)
        {
            var u = catalog.shopUnits[i];
            catalog.shopUnits[i] = SaveChain(u, root + "/UnitDefinitions");
        }

        for (int i = 0; i < catalog.shopBoosts.Length; i++)
            catalog.shopBoosts[i] = SaveChain(catalog.shopBoosts[i], root + "/BoostDefinitions");

        for (int i = 0; i < catalog.enemies.Length; i++)
            catalog.enemies[i] = Save(catalog.enemies[i], root + "/EnemyDefinitions/" + catalog.enemies[i].id + ".asset");

        for (int i = 0; i < catalog.rounds.Length; i++)
        {
            var r = catalog.rounds[i];
            catalog.rounds[i] = Save(r, root + "/RoundDefinitions/Round_" + r.roundIndex.ToString("00") + ".asset");
        }

        var saved = Save(catalog, root + "/GameCatalog.asset");
        AssetDatabase.SaveAssets();
        return AssetDatabase.GetAssetPath(saved);
    }

    static UnitDefinition SaveChain(UnitDefinition unit, string folder)
    {
        var current = unit;
        UnitDefinition first = null;
        UnitDefinition prev = null;
        while (current != null)
        {
            var saved = Save(current, folder + "/" + current.id + ".asset");
            if (first == null) first = saved;
            if (prev != null)
            {
                prev.mergesInto = saved;
                EditorUtility.SetDirty(prev);
            }
            prev = saved;
            current = current.mergesInto;
        }
        return first;
    }

    static BoostDefinition SaveChain(BoostDefinition boost, string folder)
    {
        var current = boost;
        BoostDefinition first = null;
        BoostDefinition prev = null;
        while (current != null)
        {
            var saved = Save(current, folder + "/" + current.id + ".asset");
            if (first == null) first = saved;
            if (prev != null)
            {
                prev.mergesInto = saved;
                EditorUtility.SetDirty(prev);
            }
            prev = saved;
            current = current.mergesInto;
        }
        return first;
    }

    static T Save<T>(T obj, string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null)
        {
            EditorUtility.CopySerialized(obj, existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }
        AssetDatabase.CreateAsset(obj, path);
        return obj;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
        var name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent))
            AssetDatabase.CreateFolder(parent, name);
    }
}
