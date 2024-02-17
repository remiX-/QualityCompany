namespace QualityCompany.Service;

public partial class GameEvents
{
    private static readonly ACLogger Logger = new(nameof(GameEvents));

    public static event TimeUpdateEvent GameTimeUpdate;
    public delegate void TimeUpdateEvent();

    public static event DisconnectEvent Disconnected;
    public delegate void DisconnectEvent(GameNetworkManager instance);

    internal static void OnDisconnected(GameNetworkManager instance)
    {
        Logger.LogDebug("OnDisconnected");
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

        // Terminal
        TerminalAwakeEvent = null;

        // Game
        Disconnected = null;
    }

    internal static void OnGameTimeUpdate()
    {
        GameTimeUpdate?.Invoke();
    }
}