using AdvancedCompany.Manager.ShipTerminal;
using AdvancedCompany.Service;
using AdvancedCompany.Utils;
using HarmonyLib;
using System.Linq;
using UnityEngine;

namespace AdvancedCompany.Modules;

internal class ScanFixModule
{
    private static readonly ACLogger _logger = new(nameof(ScanFixModule));

    private static bool hasPatched;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Terminal), "Awake")]
    internal static void ScanFixPatch(Terminal __instance)
    {
        if (hasPatched) return;

        hasPatched = true;

        var list = __instance.terminalNodes.allKeywords.ToList();
        var scanKeyword = list.Find(keyword => keyword.word == "scan");
        if (scanKeyword is null)
        {
            _logger.LogError("Failed to find can terminal keyword.");
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
            var allObjectsInDungeon = Object.FindObjectsOfType<GrabbableObject>()
                .Where(go => go.itemProperties.isScrap && !go.isInShipRoom && !go.isInElevator)
                .ToList();
            var allObjectInDungeonTotalScrapValue = allObjectsInDungeon.Sum(go => go.scrapValue);

            if (allObjectInDungeonTotalScrapValue > 0)
            {
                return $"FIX: There are {allObjectsInDungeon.Count} objects outside the ship, totalling at an exact value of {allObjectInDungeonTotalScrapValue}.";
            }

            var allInShip = ScrapUtils.GetAllScrapInShip();
            var allInShipTotalScrapValue = allInShip.Sum(go => go.scrapValue);
            return $"FIX: There are {allInShip.Count} objects inside the ship, totalling at an exact value of {allInShipTotalScrapValue}.";
        });

        _logger.LogDebug("Fixed scan terminal command");
    }
}

