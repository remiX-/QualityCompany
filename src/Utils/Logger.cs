namespace AdvancedCompany.Utils;

internal class Logger
{
    public static void LogDebug(object message)
    {
#if DEBUG
        Plugin.Instance.ACLogger.LogDebug(message);
#endif
    }

    public static void LogMessage(object message)
    {
        Plugin.Instance.ACLogger.LogMessage(message);
    }
}
