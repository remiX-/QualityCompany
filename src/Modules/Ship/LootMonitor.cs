using QualityCompany.Service;
using QualityCompany.Utils;
using System.Collections;
using UnityEngine;

namespace QualityCompany.Modules.Ship;

internal class LootMonitor : BaseMonitor
{
    public static LootMonitor? Instance;

    protected override void PostStart()
    {
        Instance = this;
        Logger = new ModLogger(nameof(LootMonitor));

        StartCoroutine(RefreshMonitor());
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    internal static void UpdateMonitor()
    {
        if (GameUtils.ShipGameObject == null)
        {
            Instance?.Logger.LogError("ShipGameObject is null");
            return;
        }

        var totalInShip = ScrapUtils.GetShipSettledTotalRawScrapValue();
        var groupCredits = GameUtils.Terminal.groupCredits;
        Instance?.UpdateMonitorText($"LOOT:\n${totalInShip}\nCREDITS:\n${groupCredits}");
    }

    // ReSharper disable once FunctionRecursiveOnAllPaths
    private IEnumerator RefreshMonitor()
    {
        UpdateMonitor();
        yield return new WaitForSeconds(5.0f);
        StartCoroutine(RefreshMonitor());
    }
}