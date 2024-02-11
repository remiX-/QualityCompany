using GameNetcodeStuff;

namespace QualityCompany.Service;

public class GameEvents
{
    private static readonly ACLogger _logger = new(nameof(GameEvents));

    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public static event TimeUpdateEvent GameTimeUpdate;
    public delegate void TimeUpdateEvent();

    public delegate void PlayerControllerEvent(PlayerControllerB instance);
    public delegate void PlayerControllerRpcEvent(PlayerControllerB instance, bool isLocalPlayerInstance);
    public delegate void PlayerControllerRpcWithGOEvent(PlayerControllerB instance, bool isLocalPlayerInstance, GrabbableObject gameObject);
    // Player rpc actions
    public static event PlayerControllerRpcEvent PlayerGrabObjectClientRpc;
    public static event PlayerControllerRpcWithGOEvent PlayerGrabObjectClientRpc2;
    public static event PlayerControllerRpcEvent PlayerSwitchToItemSlot; // seems this is actually is a Rpc call in the background
    public static event PlayerControllerRpcEvent PlayerThrowObjectClientRpc;
    public static event PlayerControllerRpcEvent PlayerDropAllHeldItems; // seems this is actually is a Rpc call in the background
    // Player non-rpc actions (triggers on local client only) - well hopefully
    public static event PlayerControllerEvent PlayerDiscardHeldObject;

    // Player life? actions
    public static event PlayerControllerEvent PlayerDeath;

    // Player item actions
    public static event PlayerControllerEvent PlayerShotgunShoot;
    public static event PlayerControllerEvent PlayerShotgunReload;

    public static event DisconnectEvent Disconnected;
    public delegate void DisconnectEvent(GameNetworkManager instance);

    internal static void OnHudManagerStart(HUDManager instance)
    {
        _logger.LogDebug("OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    internal static void OnPlayerGrabObjectClientRpc(PlayerControllerB instance, GrabbableObject go)
    {
        _logger.LogDebug("OnPlayerGrabObjectClientRpc");
        PlayerGrabObjectClientRpc?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
        PlayerGrabObjectClientRpc2?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController, go);
    }

    internal static void OnPlayerSwitchToItemSlot(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerSwitchToItemSlot");
        PlayerSwitchToItemSlot?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    internal static void OnPlayerThrowObjectClientRpc(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerThrowObjectClientRpc?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    internal static void OnPlayerDropAllHeldItems(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDropAllHeldItems");
        PlayerDropAllHeldItems?.Invoke(instance, instance == GameNetworkManager.Instance.localPlayerController);
    }

    internal static void OnPlayerDiscardHeldObject(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDiscardHeldObject");
        PlayerDiscardHeldObject?.Invoke(instance);
    }

    internal static void OnPlayerShotgunShoot(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerShootShotgun");
        PlayerShotgunShoot?.Invoke(instance);
    }

    internal static void OnPlayerDeath(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerDeath");
        PlayerDeath?.Invoke(instance);
    }

    internal static void OnPlayerShotgunReload(PlayerControllerB instance)
    {
        _logger.LogDebug("OnPlayerShotgunReload");
        PlayerShotgunReload?.Invoke(instance);
    }

    internal static void OnDisconnected(GameNetworkManager instance)
    {
        _logger.LogDebug("OnDisconnected");
        Disconnected?.Invoke(instance);

        // HUD
        HudManagerStart = null;
        GameTimeUpdate = null;

        // Player
        PlayerGrabObjectClientRpc = null;
        PlayerSwitchToItemSlot = null;
        PlayerThrowObjectClientRpc = null;
        PlayerDropAllHeldItems = null;
        PlayerDiscardHeldObject = null;
        PlayerDeath = null;

        PlayerShotgunShoot = null;
        PlayerShotgunReload = null;

        // Game
        Disconnected = null;
    }

    internal static void OnGameTimeUpdate()
    {
        GameTimeUpdate?.Invoke();
    }
}