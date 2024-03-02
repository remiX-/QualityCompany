using HarmonyLib;
using QualityCompany.Modules.Ship;
using QualityCompany.Network;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("PlaceItemOnCounter")]
    private static void PlaceItemOnCounterPatch()
    {
        if (!Plugin.Instance.PluginConfig.NetworkingEnabled) return;

        CompanyNetworkHandler.Instance.SyncDepositDeskTotalValueServerRpc();
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddObjectToDeskClientRpc")]
    private static void CalculateTotalOnDesk()
    {
        InfoMonitor.UpdateMonitor();
    }
}

