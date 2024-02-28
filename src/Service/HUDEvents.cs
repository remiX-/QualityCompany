namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event HUDEvent? HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    public static event TimeUpdateEvent? GameTimeUpdate;
    public delegate void TimeUpdateEvent();

    internal static void OnHudManagerStart(HUDManager instance)
    {
        Logger.LogDebugMode("OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    internal static void OnGameTimeUpdate()
    {
        GameTimeUpdate?.Invoke();
    }
}