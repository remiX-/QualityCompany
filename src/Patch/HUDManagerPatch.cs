using HarmonyLib;
using Newtonsoft.Json;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(HUDManager))]
internal class HUDManagerPatch
{
    private static readonly ACLogger _logger = new(nameof(HUDManagerPatch));

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void Start(HUDManager __instance)
    {
        OnHudManagerStart(__instance);

        // Temp for now as HUDManager starts a little bit later than StartOfRound
        // TODO: maybe move into a DebugModule?
        _logger.LogDebug(JsonConvert.SerializeObject(SaveManager.SaveData));
        Plugin.Instance.PluginConfig.DebugPrintConfig(_logger);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ApplyPenalty")]
    private static void ApplyPenalty()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayCreditsEarning")]
    private static void DisplayCreditsEarning()
    {
        InfoMonitor.UpdateMonitor();
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayDaysLeft")]
    private static void DisplayDaysLeft(TimeOfDay __instance)
    {
        _logger.LogDebug("DisplayDaysLeft");

        SaveManager.SaveData.TotalDaysPlayedForCurrentQuota++;
        SaveManager.Save();

        InfoMonitor.UpdateMonitor();
    }
}

