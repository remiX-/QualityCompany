using HarmonyLib;
using QualityCompany.Network;
using QualityCompany.Service;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

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
