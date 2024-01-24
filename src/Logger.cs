namespace AdvancedCompany;

public class Logger
{
    public static void LogDebug(object message)
    {
#if DEBUG
        Plugin.Instance.Logger.LogDebug(message);
#endif
    }

    public static void LogMessage(object message)
    {
        Plugin.Instance.Logger.LogMessage(message);
    }

    public static void LogWarning(object message)
    {
        Plugin.Instance.Logger.LogWarning(message);
    }

    public static void LogInfo(object message)
    {
        Plugin.Instance.Logger.LogInfo(message);
    }

    public static void LogError(object message)
    {
        Plugin.Instance.Logger.LogError(message);
    }

    public static void LogFatal(object message)
    {
        Plugin.Instance.Logger.LogFatal(message);
    }
}
