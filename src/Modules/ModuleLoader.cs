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
        _logger.LogDebug("Loading up internal modules with a 2 second delay...");

        yield return new WaitForSeconds(2.0f);

        HUDScrapValueUIModule.Spawn();
        HUDShotgunAmmoUIModule.Spawn();
        ScanFixModule.Handle();
        MonitorModule.Spawn();

        _logger.LogDebug("Internal modules loaded!");
    }
}

