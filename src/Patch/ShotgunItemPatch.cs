using HarmonyLib;
using QualityCompany.Service;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(ShotgunItem))]
internal class ShotgunItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("ShootGun")]
    private static void ShootGunPatch()
    {
        OnPlayerShotgunShoot(GameNetworkManager.Instance.localPlayerController);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReloadGunEffectsClientRpc")]
    private static void ReloadGunEffectsClientRpcPatch()
    {
        OnPlayerShotgunReload(GameNetworkManager.Instance.localPlayerController);
    }
}