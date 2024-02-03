using AdvancedCompany.Manager.ShipTerminal;
using AdvancedCompany.TerminalCommands;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace AdvancedCompany;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    internal static Plugin Instance;
    internal static AssetBundle CustomAssets;

    internal ManualLogSource ACLogger;

    internal PluginConfig PluginConfig;

    private void Awake()
    {
        Instance = this;
        ACLogger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

        // GameUtils.Init();

        // Plugin patch logic
        NetcodePatcher();
        Patch();

        var dllFolderPath = Path.GetDirectoryName(Info.Location);
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(dllFolderPath, "modnetworkhandlerbundle"));
        if (CustomAssets is null)
        {
            ACLogger.LogError("Failed to load custom assets!");
        }

        // LoadConfigs();
        PluginConfig = new PluginConfig(Config);
        PluginConfig.Bind();

        // Loaded
        ACLogger.LogMessage($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Patch()
    {
        AdvancedTerminal.Sub(new SellCommands());
        AdvancedTerminal.Sub(new MiscCommands());
        AdvancedTerminal.Sub(new TargetCommands());

        harmony.PatchAll(Assembly.GetExecutingAssembly());
        // harmony.PatchAll(typeof(ScanFixModule));
        // harmony.PatchAll(typeof(TerminalPatch));
        // harmony.PatchAll(typeof(GameNetworkManagerPatch));
        // harmony.PatchAll(typeof(NetworkObjectManager));
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

    #region Config

    // public static ConfigEntry<int> ConfigExactAmountAllowance;
    public static ConfigEntry<string> ConfigSellIgnoreList;

    private void LoadConfigs()
    {
        ConfigSellIgnoreList = Config.Bind("Can Sell",
            "SellIgnoreList",
            "shotgun,gunammo,gift,pickle,airhorn",
            "A comma separated list of items to ignore in the ship. Does not have to be the exact name.");

        // ConfigExactAmountAllowance = Config.Bind("Misc",
        //     "ExactAmountAllowance",
        //     0,
        //     "The amount of allowance over the specified amount to grant. Ex: Setting this to 5 will make 'sell 50' also sell at 51, 52, 53, 54 and 55");
    }

    #endregion
}