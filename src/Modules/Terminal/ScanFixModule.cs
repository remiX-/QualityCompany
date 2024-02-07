using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Core;
using QualityCompany.Utils;
using System.Linq;
using UnityEngine;

namespace QualityCompany.Modules.Terminal;

[Module]
internal class ScanFixModule
{
    private static bool hasInit;

    [ModuleOnStart]
    private static void Handle()
    {
        if (!Plugin.Instance.PluginConfig.TerminalPatchFixScanEnabled || hasInit) return;
        hasInit = true;

        var list = AdvancedTerminal.Terminal.terminalNodes.allKeywords.ToList();
        var scanKeyword = list.Find(keyword => keyword.word == "scan");
        if (scanKeyword is null)
        {
            // _logger.LogError("Failed to find can terminal keyword.");
            return;
        }

        scanKeyword.specialKeywordResult = new TerminalNode
        {
            name = "scan",
            displayText = "[scanForItemsFix]",
            clearPreviousText = true
        };

        AdvancedTerminal.AddGlobalTextReplacement("[scanForItemsFix]", () =>
        {
            var allObjectsInDungeon = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None)
                .Where(go => go.itemProperties.isScrap && !go.isInShipRoom && !go.isInElevator)
                .ToList();
            var allObjectInDungeonTotalScrapValue = allObjectsInDungeon.Sum(go => go.scrapValue);

            if (allObjectInDungeonTotalScrapValue > 0)
            {
                return $"There are {allObjectsInDungeon.Count} objects outside the ship, totalling at an exact value of {allObjectInDungeonTotalScrapValue}.";
            }

            var allInShip = ScrapUtils.GetAllScrapInShip();
            var allInShipTotalScrapValue = allInShip.Sum(go => go.scrapValue);
            return $"There are {allInShip.Count} objects inside the ship, totalling at an exact value of {allInShipTotalScrapValue}.";
        });

        // _logger.LogDebug("Loaded");
    }
}

