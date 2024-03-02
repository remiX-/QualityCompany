namespace QualityCompany.Events;

public partial class GameEvents
{
    public static event StartOfRoundEvent? StartOfRoundAwake;
    public static event StartOfRoundEvent? StartOfRoundStart;
    public static event StartOfRoundEvent? EndOfGame;
    public static event StartOfRoundEvent? PlayersFired;

    public delegate void StartOfRoundEvent(StartOfRound instance);

    internal static void OnEndOfGame(StartOfRound instance)
    {
        Logger.LogDebugMode("OnEndOfGame");
        EndOfGame?.Invoke(instance);
    }

    internal static void OnStartOfRoundAwake(StartOfRound instance)
    {
        Logger.LogDebugMode($"OnStartOfRoundAwake -> {StartOfRoundAwake?.GetInvocationList().Length}");
        StartOfRoundAwake?.Invoke(instance);
    }

    internal static void OnStartOfRoundStart(StartOfRound instance)
    {
        Logger.LogDebugMode($"OnStartOfRoundStart -> {StartOfRoundStart?.GetInvocationList().Length}");
        StartOfRoundStart?.Invoke(instance);
    }

    internal static void OnPlayersFired(StartOfRound instance)
    {
        Logger.LogDebugMode("OnPlayersFired");
        PlayersFired?.Invoke(instance);
    }
}