using GameNetcodeStuff;

namespace QualityCompany.Service;

public class GameEvents
{
    private static readonly ACLogger _logger = new(nameof(GameEvents));

    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public delegate void PlayerControllerEvent(PlayerControllerB instance);
    public delegate void PlayerControllerRpcEvent(PlayerControllerB instance, bool isLocalPlayerInstance);
    // Player rpc actions
    public static event PlayerControllerRpcEvent PlayerGrabObjectClientRpc;
    public static event PlayerControllerRpcEvent PlayerSwitchToItemSlot; // seems this is actually is a Rpc call in the background
    public static event PlayerControllerRpcEvent PlayerThrowObjectClientRpc;
    // Player non-rpc actions (triggers on local client only) - well hopefully
    public static event PlayerControllerEvent PlayerDropAllHeldItems;
    public static event PlayerControllerEvent PlayerDiscardHeldObject;

    // Player life? actions
    public static event PlayerControllerEvent PlayerDeath;

    // Player item actions
    public static event PlayerControllerEvent PlayerShotgunShoot;
    public static event PlayerControllerEvent PlayerShotgunReload;

    public static event DisconnectEvent Disconnected;
    public delegate void DisconnectEvent(GameNetworkManager instance);


    public static void OnHudManagerStart(HUDManager instance)
    {
        _logger.LogDebug("OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    public static void OnPlayerGrabObjectClientRpc(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerGrabObjectClientRpc");
        PlayerGrabObjectClientRpc?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    public static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerSwitchToItemSlot");
        PlayerSwitchToItemSlot?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    public static void OnPlayerThrowObjectClientRpc(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerThrowObjectClientRpc?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    public static void OnPlayerDropAllHeldItems(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDropAllHeldItems");
        PlayerDropAllHeldItems?.Invoke(instance);
    }

    public static void OnPlayerDiscardHeldObject(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerDiscardHeldObject?.Invoke(instance);
    }

    public static void OnPlayerShotgunShoot(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerShootShotgun");
        PlayerShotgunShoot?.Invoke(instance);
    }

    public static void OnPlayerDeath(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDeath");
        PlayerDeath?.Invoke(instance);
    }

    public static void OnPlayerShotgunReload(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerShotgunReload");
        PlayerShotgunReload?.Invoke(instance);
    }

    public static void OnDisconnected(GameNetworkManager instance)
    {
        _logger.LogDebug("OnDisconnected");
        Disconnected?.Invoke(instance);
    }
}

