using HarmonyLib;
using QualityCompany.Components;
using QualityCompany.Network;
using QualityCompany.Service;
using QualityCompany.Utils;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    private static readonly ACLogger _logger = new(nameof(TimeOfDayPatch));

    [HarmonyPostfix]
    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    private static void SyncNewProfitQuotaClientRpcPatch(TimeOfDay __instance)
    {
        CompanyNetworkHandler.Instance.SaveData.TotalLootValue = ScrapUtils.GetShipTotalRawScrapValue();
        CompanyNetworkHandler.Instance.SaveData.TotalDaysPlayedForCurrentQuota = 0;

        if (__instance.IsHost)
        {
            CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
        }

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    private static void MoveTimeOfDayPatch()
    {
        TimeMonitor.UpdateMonitor();
        OnGameTimeUpdate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTimePatch()
    {
        OvertimeMonitor.UpdateMonitor();
    }
}

