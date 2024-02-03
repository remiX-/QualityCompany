using AdvancedCompany.Network;
using AdvancedCompany.Service;
using HarmonyLib;
using static AdvancedCompany.Service.GameEvents;

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    private static readonly ACLogger _logger = new(nameof(GameNetworkManagerPatch));

    [HarmonyPostfix]
    [HarmonyPatch("Disconnect")]
    private static void Disconnect(GameNetworkManager __instance)
    {
        _logger.LogDebug("Disconnect!!");
        OnDisconnected(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("SaveGame")]
    private static void SaveGame(GameNetworkManager __instance)
    {
        if (!__instance.isHostingGame) return;
        _logger.LogDebug("Saving!!");
        CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
    }
}
