namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event StartOfRoundEvent StartOfRoundAwake;
    public static event StartOfRoundEvent StartOfRoundStart;
    public static event StartOfRoundEvent EndOfGame;
    public static event StartOfRoundEvent PlayersFired;

    public delegate void StartOfRoundEvent(StartOfRound instance);

    internal static void OnEndOfGame(StartOfRound instance)
    {
        EndOfGame?.Invoke(instance);
    }

    internal static void OnStartOfRoundAwake(StartOfRound instance)
    {
        StartOfRoundAwake?.Invoke(instance);
    }

    internal static void OnStartOfRoundStart(StartOfRound instance)
    {
        StartOfRoundStart?.Invoke(instance);
    }

    internal static void OnPlayersFired(StartOfRound instance)
    {
        PlayersFired?.Invoke(instance);
    }
}