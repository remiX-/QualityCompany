namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event StartOfRoundEvent EndOfGame;
    public delegate void StartOfRoundEvent(StartOfRound instance);

    internal static void OnEndOfGame(StartOfRound instance)
    {
        Logger.LogDebug("OnEndOfGame");
        EndOfGame?.Invoke(instance);
    }
}