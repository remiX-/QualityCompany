using HarmonyLib;
using QualityCompany.Components;
using QualityCompany.Network;
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
        _logger.LogDebug("HudManagerStartPatch");

        OnHudManagerStart(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ApplyPenalty")]
    private static void ApplyPenalty()
    {
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayCreditsEarning")]
    private static void DisplayCreditsEarning()
    {
        OvertimeMonitor.UpdateMonitor();
        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayDaysLeft")]
    private static void DisplayDaysLeft(TimeOfDay __instance)
    {
        _logger.LogDebug("DisplayDaysLeft");

        CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota++;

        if (__instance.IsHost)
        {
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }

        OvertimeMonitor.UpdateMonitor();
    }
}

