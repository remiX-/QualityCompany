namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event GameNetworkManagerEvent GameNetworkManagerAwake;
    public static event GameNetworkManagerEvent GameNetworkManagerStart;

    public delegate void GameNetworkManagerEvent(GameNetworkManager instance);

    internal static void OnGameNetworkManagerAwake(GameNetworkManager instance)
    {
        GameNetworkManagerAwake?.Invoke(instance);
    }

    internal static void OnGameNetworkManagerStart(GameNetworkManager instance)
    {
        GameNetworkManagerStart?.Invoke(instance);
    }
}