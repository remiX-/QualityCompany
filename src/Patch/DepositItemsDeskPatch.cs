using AdvancedCompany.Components;
using AdvancedCompany.Network;
using AdvancedCompany.Service;
using GameNetcodeStuff;
using HarmonyLib;

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(DepositItemsDesk))]
internal class DepositItemsDeskPatch
{
    private static readonly ACLogger _logger = new(nameof(DepositItemsDeskPatch));

    [HarmonyPostfix]
    [HarmonyPatch("PlaceItemOnCounter")]
    private static void PlaceItemOnCounterPatch(ref PlayerControllerB playerWhoTriggered)
    {
        _logger.LogDebug("PlaceItemOnCounter");

        CompanyNetworkHandler.Instance.SyncDepositDeskTotalValueServerRpc();
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.IsHost}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.name}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerSteamId}");
        // Logger.LogInfo($"PlaceItemOnCounter: {playerWhoTriggered.playerUsername}");
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddObjectToDeskClientRpc")]
    private static void CalculateTotalOnDesk()
    {
        _logger.LogDebug("AddObjectToDeskClientRpc");

        OvertimeMonitor.UpdateMonitor();
    }
}

