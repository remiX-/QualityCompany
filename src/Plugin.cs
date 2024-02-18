using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLib.Extras;
using LethalLib.Modules;
using QualityCompany.Assets;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Core;
using QualityCompany.Patch;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QualityCompany;

[BepInPlugin(PluginMetadata.PLUGIN_GUID, PluginMetadata.PLUGIN_NAME, PluginMetadata.PLUGIN_VERSION)]
[BepInDependency("evaisa.lethallib")]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new(PluginMetadata.PLUGIN_GUID);

    internal static Plugin Instance;

    internal ManualLogSource ACLogger;

    internal PluginConfig PluginConfig;

    internal string PluginPath;

    private void Awake()
    {
        Instance = this;
        ACLogger = BepInEx.Logging.Logger.CreateLogSource(PluginMetadata.PLUGIN_NAME);

        // Asset Bundles
        PluginPath = Path.GetDirectoryName(Info.Location)!;

        // Config
        PluginConfig = new PluginConfig();
        PluginConfig.Bind(Config);

        // Plugin patch logic
        NetcodePatcher();
        Patch();
        LoadAssets();

        // Loaded
        ACLogger.LogMessage($"Plugin {PluginMetadata.PLUGIN_NAME} v{PluginMetadata.PLUGIN_VERSION} is loaded!");
    }

    private void Patch()
    {
        AdvancedTerminalRegistry.Register(Assembly.GetExecutingAssembly());
        ModuleRegistry.Register(Assembly.GetExecutingAssembly());

        harmony.PatchAll(Assembly.GetExecutingAssembly());
        harmony.PatchAll(typeof(MenuPatch));
    }

    private static void NetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    private void LoadAssets()
    {
        AssetManager.LoadModBundle(PluginPath);

        var atmItem = AssetManager.GetItemObject("ATM");
        
        NetworkPrefabs.RegisterNetworkPrefab(atmItem);
        AssetManager.AddPrefab("ATM", atmItem);

        var itemInfoNode = ScriptableObject.CreateInstance<TerminalNode>();
        itemInfoNode.clearPreviousText = true;
        itemInfoNode.name = "atm_itemInfo";
        itemInfoNode.displayText = "atm_test";

        Unlockables.RegisterUnlockable(
            new UnlockableItemDef
            {
                storeType = StoreType.ShipUpgrade,
                unlockable = new UnlockableItem
                {
                    unlockableName = "atm",
                    spawnPrefab = true,
                    prefabObject = AssetManager.Prefabs["ATM"],
                    IsPlaceable = true,
                    alwaysInStock = true,
                    unlockableType = 1
                }
            }, StoreType.ShipUpgrade,
            itemInfo: itemInfoNode,
            price: 1
        );


        // var prop = atmItem.spawnPrefab.AddComponent<PlaceableShipObject>();
        // NetworkManager.Singleton.AddNetworkPrefab(atmItem.spawnPrefab);
        // AssetBundleLoader.AddPrefab("ATM", atmItem.spawnPrefab);
        // prop.itemProperties = atmItem;
        // prop.itemProperties.minValue = 1;
        // prop.itemProperties.maxValue = 3;

        // var creditCardItem = AssetBundleLoader.GetItemObject("CreditCard");
        // var prop2 = atmItem.spawnPrefab.AddComponent<PhysicsProp>();
        // prop2.itemProperties = atmItem;
        // prop2.itemProperties.minValue = 100;
        // prop2.itemProperties.maxValue = 3;
        // NetworkManager.Singleton.AddNetworkPrefab(creditCardItem.spawnPrefab);
        // AssetBundleLoader.AddPrefab("CreditCard", creditCardItem.spawnPrefab);
    }
}