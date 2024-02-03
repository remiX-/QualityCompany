using HarmonyLib;
using QualityCompany.Components;
using QualityCompany.Network;
using QualityCompany.Service;
using QualityCompany.Utils;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    private static readonly ACLogger _logger = new(nameof(TimeOfDayPatch));

    [HarmonyPostfix]
    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    private static void OvertimeBonus(TimeOfDay __instance)
    {
        _logger.LogMessage("SyncNewProfitQuotaClientRpc");

        CompanyNetworkHandler.Instance.SaveData.TotalLootValue = ScrapUtils.GetShipTotalRawScrapValue();
        CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota = 0;

        if (__instance.IsHost)
        {
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    private static void RefreshClock()
    {
        TimeMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTime()
    {
        OvertimeMonitor.UpdateMonitor();
    }
}

