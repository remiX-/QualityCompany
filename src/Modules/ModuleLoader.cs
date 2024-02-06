using System;
using QualityCompany.Service;
using System.Collections;
using UnityEngine;

namespace QualityCompany.Modules;

internal class ModuleLoader : MonoBehaviour
{
    public static ModuleLoader Instance { get; private set; }

    private readonly ACLogger _logger = new(nameof(ModuleLoader));

    private void Start()
    {
        Instance = this;

        transform.position = Vector3.zero;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;

        StartCoroutine(LoadScrapValueUIModulesCoroutine());
    }

    private IEnumerator LoadScrapValueUIModulesCoroutine()
    {
        var delay = Math.Max(3.0f, Plugin.Instance.PluginConfig.InventoryStartupDelay);
        _logger.LogDebug($"Loading up internal modules with a {delay} seconds delay...");

        ScanFixModule.Handle();
        MonitorModule.Spawn();

        yield return new WaitForSeconds(delay);

        HUDScrapValueUIModule.Spawn();
        HUDShotgunAmmoUIModule.Spawn();

        _logger.LogDebug("Internal modules loaded!");
    }
}

