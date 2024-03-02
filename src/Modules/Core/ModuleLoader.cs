using QualityCompany.Service;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static QualityCompany.Events.GameEvents;

namespace QualityCompany.Modules.Core;

internal class ModuleLoader : MonoBehaviour
{
    private readonly ModLogger Logger = new(nameof(ModuleLoader));

    private void Start()
    {
        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        StartCoroutine(LoadModulesCoroutine());

        Disconnected += DetachAllModules;
    }

    private IEnumerator LoadModulesCoroutine()
    {
        var delay = Math.Max(3.0f, Plugin.Instance.PluginConfig.InventoryStartupDelay);
        Logger.TryLogDebug($"Loading up modules with a {delay} seconds delay...");

        foreach (var internalModule in ModuleRegistry.Modules.Where(x => !x.DelayedStart))
        {
            Logger.TryLogDebug($"Starting up {internalModule.Name}");
            var instance = internalModule.OnLoad?.Invoke(null, null);
            if (instance is null) continue;

            internalModule.Instance = instance;
            internalModule.OnAttach?.Invoke(instance, null);
        }

        yield return new WaitForSeconds(delay);

        foreach (var internalModule in ModuleRegistry.Modules.Where(x => x.DelayedStart))
        {
            Logger.TryLogDebug($"Starting up {internalModule.Name}");
            var instance = internalModule.OnLoad?.Invoke(null, null);
            if (instance is null) continue;

            internalModule.Instance = instance;
            internalModule.OnAttach?.Invoke(instance, null);
        }

        Logger.TryLogDebug("Internal modules loaded!");
    }

    private void DetachAllModules(GameNetworkManager _)
    {
        Logger.TryLogDebug("DetachAllModules");

        foreach (var internalModule in ModuleRegistry.Modules)
        {
            if (internalModule.Instance is null) continue;

            Logger.TryLogDebug($"Detaching {internalModule.Name}");
            internalModule.OnDetach?.Invoke(internalModule.Instance, null);
            internalModule.Instance = null;
        }
    }
}