using AdvancedCompany.Network;
using AdvancedCompany.Service;
using HarmonyLib;

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    private static readonly ACLogger _logger = new ACLogger(nameof(GameNetworkManagerPatch));

    [HarmonyPostfix]
    [HarmonyPatch("Disconnect")]
    private static void Disconnect()
    {
        _logger.LogDebug("Disconnect!!");
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
