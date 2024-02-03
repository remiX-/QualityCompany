using AdvancedCompany.Service;
using HarmonyLib;
using static AdvancedCompany.Service.GameEvents;

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(ShotgunItem))]
internal class ShotgunItemPatch
{
    private static readonly ACLogger _logger = new(nameof(ShotgunItemPatch));

    [HarmonyPostfix]
    [HarmonyPatch("ShootGun")]
    private static void ShootGunPatch()
    {
        _logger.LogDebug("ShootGun");

        OnPlayerShotgunShoot(GameNetworkManager.Instance.localPlayerController);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReloadGunEffectsClientRpc")]
    private static void ReloadGunEffectsClientRpcPatch()
    {
        _logger.LogDebug("ReloadGunEffectsClientRpcPatch");

        OnPlayerShotgunReload(GameNetworkManager.Instance.localPlayerController);
    }
}