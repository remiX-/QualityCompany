using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Core;
using QualityCompany.Utils;
using System.Linq;
using UnityEngine;

namespace QualityCompany.Modules.ShipTerminal;

[Module]
internal class ScanFixModule
{
    [ModuleOnLoad]
    private static void Handle()
    {
        if (!Plugin.Instance.PluginConfig.TerminalPatchFixScanEnabled) return;

        var list = GameUtils.Terminal.terminalNodes.allKeywords.ToList();
        var scanKeyword = list.Find(keyword => keyword.word == "scan");
        if (scanKeyword is null)
        {
            return;
        }

        scanKeyword.specialKeywordResult = TerminalUtils.CreateNode("scan", "[qc__scanForItemsFix]");

        AdvancedTerminal.AddGlobalTextReplacement("[qc__scanForItemsFix]", () =>
        {
            var allObjectsInDungeon = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None)
                .Where(go => go.itemProperties.isScrap && !go.isInShipRoom && !go.isInElevator)
                .OrderBy(go => go.scrapValue)
                .ToList();
            var allObjectInDungeonTotalScrapValue = allObjectsInDungeon.Sum(go => go.scrapValue);

            if (allObjectInDungeonTotalScrapValue > 0)
            {
                var scrapText = allObjectsInDungeon
                    .Select(x => $"{x.itemProperties.name}: ${x.scrapValue}")
                    .Aggregate((first, next) => $"{first}\n{next}");
                return $"There are {allObjectsInDungeon.Count} objects outside the ship, totalling at an exact value of {allObjectInDungeonTotalScrapValue}.\nItems:\n{scrapText}";
            }

            var allInShip = ScrapUtils.GetAllScrapInShip();
            var allInShipTotalScrapValue = allInShip.Sum(go => go.scrapValue);
            return $"There are {allInShip.Count} objects inside the ship, totalling at an exact value of {allInShipTotalScrapValue}.";
        });
    }
}

