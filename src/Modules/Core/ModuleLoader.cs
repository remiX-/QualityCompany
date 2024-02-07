using QualityCompany.Service;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Modules.Core;

internal class ModuleLoader : MonoBehaviour
{
    private readonly ACLogger _logger = new(nameof(ModuleLoader));

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        StartCoroutine(LoadScrapValueUIModulesCoroutine());

        Disconnected += DetachAllModules;
    }

    private IEnumerator LoadScrapValueUIModulesCoroutine()
    {
        var delay = Math.Max(3.0f, Plugin.Instance.PluginConfig.InventoryStartupDelay);
        _logger.LogDebug($"Loading up internal modules with a {delay} seconds delay...");

        // ScanFixModule.Handle();
        // MonitorModule.Spawn();

        foreach (var internalModule in ModuleRegistry.Modules.Where(x => !x.DelayedStart))
        {
            _logger.LogDebug($"Starting up {internalModule.Name}");
            var instance = internalModule.OnStart?.Invoke(null, null);
            if (instance is null) continue;

            internalModule.Instance = instance;
            internalModule.OnAttach?.Invoke(instance, null);
        }

        yield return new WaitForSeconds(delay);

        foreach (var internalModule in ModuleRegistry.Modules.Where(x => x.DelayedStart))
        {
            _logger.LogDebug($"Starting up {internalModule.Name}");
            var instance = internalModule.OnStart?.Invoke(null, null);
            if (instance is null) continue;

            internalModule.Instance = instance;
            internalModule.OnAttach?.Invoke(instance, null);
        }

        _logger.LogDebug("Internal modules loaded!");
    }

    private void DetachAllModules(GameNetworkManager _)
    {
        _logger.LogDebug("DetachAllModules");
        foreach (var internalModule in ModuleRegistry.Modules)
        {
            if (internalModule.Instance is null) continue;

            _logger.LogDebug($"Detaching {internalModule.Name}");
            internalModule.OnDetach?.Invoke(internalModule.Instance, null);
            internalModule.Instance = null;
        }

        Disconnected -= DetachAllModules;
    }
}