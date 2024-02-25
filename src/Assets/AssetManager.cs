using QualityCompany.Service;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QualityCompany.Assets;

internal class AssetManager
{
    private static readonly ModLogger Logger = new(nameof(AssetManager));

    internal static AssetBundle CustomAssets;

    private static string _modRoot;
    private static readonly Dictionary<string, string> AssetPaths = new();
    internal static readonly Dictionary<string, GameObject> Prefabs = new();

    internal static void LoadModBundle(string root)
    {
        _modRoot = root;
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(_modRoot, "modnetworkhandlerbundle"));
        if (CustomAssets is null)
        {
            Logger.LogError("Failed to load custom assets!");
        }
    }

    public static T LoadBundleAsset<T>(string itemName) where T : Object
    {
        if (AssetPaths.TryGetValue(itemName, out var path))
        {
            var asset = TryLoadBundleAsset<T>(ref CustomAssets, path);
            return asset;
        }

        Logger.LogError($"{itemName} was not present in the asset or sample dictionary!");
        return default;
    }

    internal static T TryLoadBundleAsset<T>(ref AssetBundle bundle, string path) where T : Object
    {
        var result = bundle.LoadAsset<T>(path);

        if (result is null)
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
