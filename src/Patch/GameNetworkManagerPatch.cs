using HarmonyLib;
using QualityCompany.Manager.Saves;
using QualityCompany.Utils;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class GameNetworkManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void AwakePatch(GameNetworkManager __instance)
    {
        OnGameNetworkManagerAwake(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    public static void StartPatch(GameNetworkManager __instance)
    {
        OnGameNetworkManagerStart(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Disconnect")]
    private static void DisconnectPatch(GameNetworkManager __instance)
    {
        OnDisconnected(__instance);

        GameUtils.Reset();
    }

    [HarmonyPrefix]
    [HarmonyPatch("SaveGame")]
    private static void SaveGamePatch(GameNetworkManager __instance)
    {
        SaveManager.Save();

        OnSaveGame(__instance);
    }
}
