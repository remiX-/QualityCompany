namespace QualityCompany.Events;

public partial class GameEvents
{
    public static event GameNetworkManagerEvent? GameNetworkManagerAwake;
    public static event GameNetworkManagerEvent? GameNetworkManagerStart;
    public static event GameNetworkManagerEvent? SaveGame;
    public static event GameNetworkManagerEvent? Disconnected;

    public delegate void GameNetworkManagerEvent(GameNetworkManager instance);

    internal static void OnGameNetworkManagerAwake(GameNetworkManager instance)
    {
        Logger.LogDebugMode("OnGameNetworkManagerAwake");
        GameNetworkManagerAwake?.Invoke(instance);
    }

    internal static void OnGameNetworkManagerStart(GameNetworkManager instance)
    {
        Logger.LogDebugMode("OnGameNetworkManagerStart");
        GameNetworkManagerStart?.Invoke(instance);
    }

    internal static void OnSaveGame(GameNetworkManager instance)
    {
        Logger.LogDebugMode("OnSaveGame");
        SaveGame?.Invoke(instance);
    }

    internal static void OnDisconnected(GameNetworkManager instance)
    {
        Logger.LogDebugMode("OnDisconnected");
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

        // StartOfRound
        // StartOfRoundAwake = null;
        // StartOfRoundStart = null;
        EndOfGame = null;
        PlayersFired = null;

        // Terminal
        TerminalAwakeEvent = null;

        // Game
        GameNetworkManagerAwake = null;
        GameNetworkManagerStart = null;
        SaveGame = null;
        Disconnected = null;
    }
}