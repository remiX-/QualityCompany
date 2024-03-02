namespace QualityCompany.Events;

public partial class GameEvents
{
    public static event HUDEvent? HudManagerStart;
    public static event HUDEvent? DisplayDaysLeft;

    public delegate void HUDEvent(HUDManager instance);

    public static event TimeUpdateEvent? GameTimeUpdate;

    public delegate void TimeUpdateEvent();

    internal static void OnHudManagerStart(HUDManager instance)
    {
        Logger.LogDebugMode("OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }

    internal static void OnDisplayDaysLeft(HUDManager instance)
    {
        Logger.LogDebugMode("OnDisplayDaysLeft");
        DisplayDaysLeft?.Invoke(instance);
    }

    internal static void OnGameTimeUpdate()
    {
        GameTimeUpdate?.Invoke();
    }
}