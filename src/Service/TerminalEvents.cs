namespace QualityCompany.Service;

/// <summary>
/// Terminal specific game events
/// </summary>
public partial class GameEvents
{
    public delegate void TerminalEvent(Terminal instance);

    /// <summary>
    /// 
    /// </summary>
    public static event TerminalEvent TerminalAwakeEvent;
    // public static event TerminalEvent TerminalAwakeEvent;

    internal static void OnTerminalAwakeEvent(Terminal instance)
    {
        Logger.LogDebug("TerminalAwakeEvent");
        TerminalAwakeEvent?.Invoke(instance);
    }
}
