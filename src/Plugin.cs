using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using AdvancedCompany.Manager;
// using TerminalApi.Classes;
using AdvancedCompany.Patch;
using AdvancedCompany.TerminalCommands;
using UnityEngine;
// using static TerminalApi.Events.Events;
// using static TerminalApi.TerminalApi;

namespace AdvancedCompany;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

    internal static Plugin Instance;
    internal static AssetBundle CustomAssets;

    internal ManualLogSource Logger;

    private void Awake()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);

        // GameUtils.Init();

        // Plugin patch logic
        Patch();

        var dllFolderPath = Path.GetDirectoryName(Info.Location);
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(dllFolderPath, "sellfromterminalbundle"));
        if (CustomAssets is null)
        {
            Logger.LogError("Failed to load custom assets!");
        }
        else
        {
            Logger.LogDebug("Loaded asset: modnetworkhandlerbundle");
        }

        LoadConfigs();

        // Loaded
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void Patch()
    {
        TerminalManager.Sub(new SellCommands());
        TerminalManager.Sub(new MiscCommands());
        TerminalManager.Sub(new TargetCommands());

        harmony.PatchAll(typeof(MonitorPatch));
        harmony.PatchAll(typeof(WeatherPatch));
        harmony.PatchAll(typeof(TerminalPatch));
        harmony.PatchAll(typeof(NetworkObjectManager));
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