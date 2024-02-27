namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event GameNetworkManagerEvent GameNetworkManagerAwake;
    public static event GameNetworkManagerEvent GameNetworkManagerStart;
    public static event GameNetworkManagerEvent SaveGame;

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
}