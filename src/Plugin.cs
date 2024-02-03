using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.TerminalCommands;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QualityCompany;

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

        // Plugin patch logic
        NetcodePatcher();
        Patch();

        // Asset Bundles
        var dllFolderPath = Path.GetDirectoryName(Info.Location);
        CustomAssets = AssetBundle.LoadFromFile(Path.Combine(dllFolderPath!, "modnetworkhandlerbundle"));
        if (CustomAssets is null)
        {
            ACLogger.LogError("Failed to load custom assets!");
        }

        // Config
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
}