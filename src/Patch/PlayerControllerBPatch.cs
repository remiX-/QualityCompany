using GameNetcodeStuff;
using HarmonyLib;
using QualityCompany.Modules.Ship;
using Unity.Netcode;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatch
{
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
        if (!grabbedObject.TryGet(out var networkObject)) return;

        var grabbableObject = networkObject.gameObject.GetComponentInChildren<GrabbableObject>();
        OnPlayerGrabObjectClientRpc(__instance, grabbableObject);
        if (grabbableObject.isInShipRoom || grabbableObject.isInElevator)
        {
            InfoMonitor.UpdateMonitor();
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
            InfoMonitor.UpdateMonitor();
        }

        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.DiscardHeldObject))]
    private static void DiscardHeldObjectPatch(PlayerControllerB __instance)
    {
        OnPlayerDiscardHeldObject(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.KillPlayer))]
    private static void KillPlayerPatch(PlayerControllerB __instance)
    {
        OnPlayerDeath(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.DropAllHeldItems))]
    private static void DropAllHeldItemsPatch(PlayerControllerB __instance)
    {
        OnPlayerDropAllHeldItems(__instance);
    }
}

