using HarmonyLib;
using QualityCompany.Components;
using QualityCompany.Network;
using QualityCompany.Service;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    private static readonly ACLogger _logger = new(nameof(DepositItemsDeskPatch));

    [HarmonyPostfix]
    [HarmonyPatch("PlaceItemOnCounter")]
    private static void PlaceItemOnCounterPatch()
    {
        _logger.LogDebug("PlaceItemOnCounter");

        CompanyNetworkHandler.Instance.SyncDepositDeskTotalValueServerRpc();
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddObjectToDeskClientRpc")]
    private static void CalculateTotalOnDesk()
    {
        _logger.LogDebug("AddObjectToDeskClientRpc");

        OvertimeMonitor.UpdateMonitor();
    }
}

