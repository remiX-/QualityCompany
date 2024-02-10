namespace QualityCompany.Service;

internal class ACLogger
{
    private readonly string _moduleName;

    internal ACLogger(string moduleName)
    {
        _moduleName = moduleName;
    }

    internal void LogDebug(object message)
    {
        if (!Plugin.Instance.PluginConfig.ShowDebugLogs) return;

        Plugin.Instance.ACLogger.LogDebug($"[{_moduleName}] {message}");
    }

    internal void LogMessage(object message)
    {
        Plugin.Instance.ACLogger.LogMessage($"[{_moduleName}] {message}");
    }

    internal void LogWarning(object message)
    {
        Plugin.Instance.ACLogger.LogWarning($"[{_moduleName}] {message}");
    }

    internal void LogInfo(object message)
    {
        Plugin.Instance.ACLogger.LogInfo($"[{_moduleName}] {message}");
    }

    internal void LogError(object message)
    {
        Plugin.Instance.ACLogger.LogError($"[{_moduleName}] {message}");
    }

    internal void LogFatal(object message)
    {
        Plugin.Instance.ACLogger.LogFatal($"[{_moduleName}] {message}");
    }

}
