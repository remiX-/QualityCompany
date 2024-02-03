using GameNetcodeStuff;

namespace AdvancedCompany.Service;

public class GameEvents
{
    private static readonly ACLogger _logger = new(nameof(GameEvents));

    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public static event PlayerControllerEvent PlayerGrabObjectClientRpc;
    public static event PlayerControllerEvent PlayerSwitchToItemSlot;
    public static event PlayerControllerEvent PlayerThrowObjectClientRpc;
    public static event PlayerControllerEvent PlayerShotgunShoot;
    public static event PlayerControllerEvent PlayerShotgunReload;
    public delegate void PlayerControllerEvent(PlayerControllerB instance);

    public static event DisconnectEvent Disconnected;
    public delegate void DisconnectEvent(GameNetworkManager instance);


    public static void OnHudManagerStart(HUDManager instance)
    {
        _logger.LogDebug("[GameEvents] OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    public static void OnPlayerGrabObjectClientRpc(PlayerControllerB instance)
    {
        _logger.LogDebug("[GameEvents] OnPlayerGrabObjectClientRpc");
        PlayerGrabObjectClientRpc?.Invoke(instance);
    }

    public static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        _logger.LogDebug("[GameEvents] OnPlayerSwitchToItemSlot");
        PlayerSwitchToItemSlot?.Invoke(instance);
    }

    public static void OnPlayerThrowObjectClientRpc(PlayerControllerB instance)
    {
        _logger.LogDebug("[GameEvents] OnPlayerDiscardHeldObject");
        PlayerThrowObjectClientRpc?.Invoke(instance);
    }

    public static void OnPlayerShotgunShoot(PlayerControllerB instance)
    {
        _logger.LogDebug("[GameEvents] OnPlayerShootShotgun");
        PlayerShotgunShoot?.Invoke(instance);
    }

    public static void OnPlayerShotgunReload(PlayerControllerB instance)
    {
        _logger.LogDebug("[GameEvents] OnPlayerShotgunReload");
        PlayerShotgunReload?.Invoke(instance);
    }

    public static void OnDisconnected(GameNetworkManager instance)
    {
        _logger.LogDebug("[GameEvents] OnDisconnected");
        Disconnected?.Invoke(instance);
    }
}

