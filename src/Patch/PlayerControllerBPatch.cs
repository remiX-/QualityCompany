using GameNetcodeStuff;
using HarmonyLib;
using QualityCompany.Components;
using QualityCompany.Service;
using Unity.Netcode;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatch
{
    private static readonly ACLogger _logger = new(nameof(PlayerControllerBPatch));

    [HarmonyPostfix]
    [HarmonyPatch("ConnectClientToPlayerObject")]
    private static void OnPlayerConnect()
    {
        _logger.LogMessage("ConnectClientToPlayerObject");

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SwitchToItemSlot")]
    private static void SwitchToItemSlotPatch(PlayerControllerB __instance)
    {
        OnPlayerSwitchToItemSlot(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("GrabObjectClientRpc")]
    private static void RefreshLootOnPickupClient(PlayerControllerB __instance, ref NetworkObjectReference grabbedObject)
    {
        _logger.LogDebug("GrabObjectClientRpc");

        OnPlayerGrabObjectClientRpc(__instance);

        if (!grabbedObject.TryGet(out var networkObject)) return;

        var componentInChildren = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (componentInChildren.isInShipRoom || componentInChildren.isInElevator)
        {
            LootMonitor.UpdateMonitor();
            OvertimeMonitor.UpdateMonitor();
        }

        CreditMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ThrowObjectClientRpc")]
    private static void RefreshLootOnThrowClient(PlayerControllerB __instance, bool droppedInElevator, bool droppedInShipRoom)
    {
        _logger.LogDebug("ThrowObjectClientRpc");
        OnPlayerThrowObjectClientRpc(__instance);

        if (droppedInShipRoom || droppedInElevator)
        {
            LootMonitor.UpdateMonitor();
            OvertimeMonitor.UpdateMonitor();
        }

        CreditMonitor.UpdateMonitor();
    }
}

