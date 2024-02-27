using HarmonyLib;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Ship;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(HUDManager))]
internal class HudManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void Start(HUDManager __instance)
    {
        OnHudManagerStart(__instance);
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

