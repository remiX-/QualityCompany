using HarmonyLib;
using QualityCompany.Manager.Saves;
using QualityCompany.Modules.Ship;
using QualityCompany.Utils;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(TimeOfDay))]
internal class TimeOfDayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
    private static void SyncNewProfitQuotaClientRpcPatch()
    {
        SaveManager.SaveData.TotalShipLootAtStartForCurrentQuota = ScrapUtils.GetShipTotalRawScrapValue();
        SaveManager.SaveData.TotalDaysPlayedForCurrentQuota = 0;
        SaveManager.Save();

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
        InfoMonitor.UpdateMonitor();
    }
}

