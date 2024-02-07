using HarmonyLib;
using QualityCompany.Network;
using QualityCompany.Utils;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Disconnect")]
    private static void Disconnect(GameNetworkManager __instance)
    {
        GameUtils.Reset();

        OnDisconnected(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("SaveGame")]
    private static void SaveGame(GameNetworkManager __instance)
    {
        if (!__instance.isHostingGame) return;

        CompanyNetworkHandler.Instance.ServerSaveFileServerRpc();
    }
}
