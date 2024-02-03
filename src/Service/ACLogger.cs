namespace QualityCompany.Service;

internal class ACLogger
{
    private readonly string _moduleName;

    public ACLogger(string moduleName)
    {
        _moduleName = moduleName;
    }

    public void LogDebug(object message)
    {
#if DEBUG
        Plugin.Instance.ACLogger.LogDebug($"[{_moduleName}] {message}");
#endif
    }

    public void LogMessage(object message)
    {
        Plugin.Instance.ACLogger.LogMessage($"[{_moduleName}] {message}");
    }

    public void LogWarning(object message)
    {
        Plugin.Instance.ACLogger.LogWarning($"[{_moduleName}] {message}");
    }

    public void LogInfo(object message)
    {
        Plugin.Instance.ACLogger.LogInfo($"[{_moduleName}] {message}");
    }

    public void LogError(object message)
    {
        Plugin.Instance.ACLogger.LogError($"[{_moduleName}] {message}");
    }

    public void LogFatal(object message)
    {
        Plugin.Instance.ACLogger.LogFatal($"[{_moduleName}] {message}");
    }

}
