using QualityCompany.Service;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QualityCompany.Assets;

internal class AssetBundleLoader
{
    private static readonly ACLogger Logger = new(nameof(AssetBundleLoader));

    internal static AssetBundle CustomAssets;

    private static string _modRoot;
    private static readonly Dictionary<string, string> AssetPaths = new() { { "ATM", "ATM1" }, { "CreditCard", "CreditCard" } };
    internal static readonly Dictionary<string, GameObject> Prefabs = new();

    internal static void LoadModBundle(string root)
    {
        _modRoot = root;
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(_modRoot, "qualitycompanyhandlerbundle"));
        if (CustomAssets is null)
        {
            Logger.LogError("Failed to load custom assets!");
        }
    }

    public static GameObject GetItemObject(string itemName)
    {
        if (AssetPaths.TryGetValue(itemName, out var path))
        {
            return TryLoadItemAsset(ref CustomAssets, path);
        }

        Logger.LogError($"{itemName} was not present in the asset or sample dictionary!");
        return null;
    }

    internal static GameObject TryLoadItemAsset(ref AssetBundle bundle, string path)
    {
        var result = bundle.LoadAsset<GameObject>(path);

        if (result == null)
        {
            Logger.LogError($"An error has occurred trying to load asset from {path}");
            return null;
        }

        Logger.LogDebug($"Loaded asset located in {path}");
        return result;
    }

    internal static void AddPrefab(string name, GameObject prefab)
    {
        Prefabs.Add(name, prefab);
    }
}
