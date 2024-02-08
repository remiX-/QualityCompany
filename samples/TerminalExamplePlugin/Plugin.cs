using BepInEx;
using HarmonyLib;
using QualityCompany.Manager.ShipTerminal;
using System.Reflection;

namespace TerminalExamplePlugin;

[BepInPlugin(PluginMetadata.PLUGIN_GUID, PluginMetadata.PLUGIN_NAME, PluginMetadata.PLUGIN_VERSION)]
[BepInDependency("umno.QualityCompany")]
public class TerminalExamplePlugin : BaseUnityPlugin
{
    private readonly Harmony harmony = new(PluginMetadata.PLUGIN_GUID);

    private void Awake()
    {
        // Plugin patch logic
        Patch();

        // Loaded
        Logger.LogMessage($"Plugin {PluginMetadata.PLUGIN_NAME} v{PluginMetadata.PLUGIN_VERSION} is loaded!");
    }

    private void Patch()
    {
        AdvancedTerminalRegistry.Register(Assembly.GetExecutingAssembly());
    }
}