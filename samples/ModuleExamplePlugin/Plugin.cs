using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using QualityCompany.Modules.Core;
using System.Reflection;

namespace ModuleExamplePlugin;

[BepInPlugin(PluginMetadata.PLUGIN_GUID, PluginMetadata.PLUGIN_NAME, PluginMetadata.PLUGIN_VERSION)]
[BepInDependency("umno.QualityCompany")]
public class ModuleExamplePlugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new(PluginMetadata.PLUGIN_GUID);

    internal static ManualLogSource Log;

    private void Awake()
    {
        Log = BepInEx.Logging.Logger.CreateLogSource(PluginMetadata.PLUGIN_NAME);

        // Plugin patch logic
        Patch();

        // Loaded
        Logger.LogMessage($"Plugin {PluginMetadata.PLUGIN_NAME} v{PluginMetadata.PLUGIN_VERSION} is loaded!");
    }

    private void Patch()
    {
        ModuleRegistry.Register(Assembly.GetExecutingAssembly());
    }
}