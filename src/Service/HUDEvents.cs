namespace QualityCompany.Service;

public partial class GameEvents
{
    public static event HUDEvent HudManagerStart;
    public delegate void HUDEvent(HUDManager instance);

    internal static void OnHudManagerStart(HUDManager instance)
    {
        Logger.LogDebug("OnHudManagerStart");
        HudManagerStart?.Invoke(instance);
    }
}