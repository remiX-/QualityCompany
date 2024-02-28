namespace QualityCompany.Service;

internal class ModLogger
{
    private readonly string _moduleName;

    internal ModLogger(string moduleName)
    {
        _moduleName = moduleName;
    }

    internal void TryLogDebug(object message)
    {
        if (!Plugin.Instance.PluginConfig.ShowDebugLogs) return;

        Plugin.Instance.Log.LogDebug($"[{_moduleName}] {message}");
    }

    internal void LogDebug(object message)
    {
        Plugin.Instance.Log.LogDebug($"[{_moduleName}] {message}");
    }

    internal void LogDebugMode(object message)
    {
#if DEBUG
        Plugin.Instance.Log.LogDebug($"[{_moduleName}] {message}");
#endif
    }

    internal void LogMessage(object message)
    {
        Plugin.Instance.Log.LogMessage($"[{_moduleName}] {message}");
    }

    internal void LogWarning(object message)
    {
        Plugin.Instance.Log.LogWarning($"[{_moduleName}] {message}");
    }

    internal void LogInfo(object message)
    {
        Plugin.Instance.Log.LogInfo($"[{_moduleName}] {message}");
    }

    internal void LogError(object message)
    {
        Plugin.Instance.Log.LogError($"[{_moduleName}] {message}");
    }

    internal void LogFatal(object message)
    {
        Plugin.Instance.Log.LogFatal($"[{_moduleName}] {message}");
    }
}
