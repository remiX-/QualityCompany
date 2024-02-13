using HarmonyLib;
using QualityCompany.Manager.Saves;
using QualityCompany.Utils;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Disconnect")]
    private static void DisconnectPatch(GameNetworkManager __instance)
    {
        GameUtils.Reset();

        OnDisconnected(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("SaveGame")]
    private static void SaveGamePatch()
    {
        SaveManager.Save();
    }
}
