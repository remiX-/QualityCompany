using HarmonyLib;
using Newtonsoft.Json;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Core;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using QualityCompany.Utils;
using UnityEngine;
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

        SaveManager.Load();
        GameUtils.Init();

        // TODO see if better place?
        var moduleLoaderGameObject = new GameObject("QualityCompanyLoader");
        moduleLoaderGameObject.AddComponent<ModuleLoader>();

        // Temp for now as HUDManager starts a little bit later than StartOfRound
        // TODO: maybe move into a DebugModule?
        _logger.LogDebug(JsonConvert.SerializeObject(SaveManager.SaveData));
        Plugin.Instance.PluginConfig.DebugPrintConfig(_logger);

        QualityCompany.TerminalApi.TestApi.Hello();
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

