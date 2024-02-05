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
        LootMonitor.UpdateMonitor();
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
        OnPlayerGrabObjectClientRpc(__instance);

        if (!grabbedObject.TryGet(out var networkObject)) return;

        var componentInChildren = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        if (componentInChildren.isInShipRoom || componentInChildren.isInElevator)
        {
            OvertimeMonitor.UpdateMonitor();
        }

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ThrowObjectClientRpc")]
    private static void RefreshLootOnThrowClient(PlayerControllerB __instance, bool droppedInElevator, bool droppedInShipRoom)
    {
        OnPlayerThrowObjectClientRpc(__instance);

        if (droppedInShipRoom || droppedInElevator)
        {
            OvertimeMonitor.UpdateMonitor();
        }

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.DiscardHeldObject))]
    private static void DiscardHeldObjectPatch(PlayerControllerB __instance)
    {
        // this seems to trigger 3 times for host? ...
        // also does not seem to work for updating UI
        OnPlayerDiscardHeldObject(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    private static void KillPlayerPatch(PlayerControllerB __instance)
    {
        OnPlayerDeath(__instance);
    }

    // This seems bad... it triggers a lot, such as just placing an item on the deposit desk triggers this
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.DropAllHeldItems))]
    private static void DropAllHeldItemsPatch(PlayerControllerB __instance)
    {
        OnPlayerDropAllHeldItems(__instance);
    }
}

