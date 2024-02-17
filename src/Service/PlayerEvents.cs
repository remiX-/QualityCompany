using GameNetcodeStuff;

namespace QualityCompany.Service;

public partial class GameEvents
{
    public delegate void PlayerControllerEvent(PlayerControllerB instance);
    public static event PlayerControllerEvent PlayerGrabObjectClientRpc;
    public static event PlayerControllerEvent PlayerSwitchToItemSlot;
    public static event PlayerControllerEvent PlayerThrowObjectClientRpc;
    public static event PlayerControllerEvent PlayerDropAllHeldItems;
    public static event PlayerControllerEvent PlayerDiscardHeldObject;

    public static event PlayerControllerEvent PlayerDeath;

    public static event PlayerControllerEvent PlayerShotgunShoot;
    public static event PlayerControllerEvent PlayerShotgunReload;

    internal static void OnPlayerGrabObjectClientRpc(PlayerControllerB instance, GrabbableObject go)
    {
        Logger.LogDebug("OnPlayerGrabObjectClientRpc");
        PlayerGrabObjectClientRpc?.Invoke(instance);
    }

    internal static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        Logger.LogDebug($"OnPlayerSwitchToItemSlot: Owner? {instance.IsOwner}");
        PlayerSwitchToItemSlot?.Invoke(instance);
    }

    internal static void OnPlayerThrowObjectClientRpc(PlayerControllerB instance)
    {
        Logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerThrowObjectClientRpc?.Invoke(instance);
    }

    internal static void OnPlayerDropAllHeldItems(PlayerControllerB instance)
    {
        Logger.LogDebug("OnPlayerDropAllHeldItems");
        PlayerDropAllHeldItems?.Invoke(instance);
    }

    internal static void OnPlayerDiscardHeldObject(PlayerControllerB instance)
    {
        Logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerDiscardHeldObject?.Invoke(instance);
    }

    internal static void OnPlayerShotgunShoot(PlayerControllerB instance)
    {
        Logger.LogDebug("OnPlayerShootShotgun");
        PlayerShotgunShoot?.Invoke(instance);
    }

    internal static void OnPlayerDeath(PlayerControllerB instance)
    {
        Logger.LogDebug($"OnPlayerDeath -> {instance.playerUsername}");
        PlayerDeath?.Invoke(instance);
    }

    internal static void OnPlayerShotgunReload(PlayerControllerB instance)
    {
        Logger.LogDebug("OnPlayerShotgunReload");
        PlayerShotgunReload?.Invoke(instance);
    }
}