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
internal class HudManagerPatch
{
    private static readonly ModLogger Logger = new(nameof(HudManagerPatch));

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
        Logger.LogDebug(JsonConvert.SerializeObject(SaveManager.SaveData));
        Plugin.Instance.PluginConfig.DebugPrintConfig(Logger);
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
        SaveManager.SaveData.TotalDaysPlayedForCurrentQuota++;
        SaveManager.Save();

        InfoMonitor.UpdateMonitor();
    }
}

