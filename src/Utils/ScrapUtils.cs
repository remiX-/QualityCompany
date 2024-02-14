using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QualityCompany.Utils;

public static class ScrapUtils
{
    private static readonly ACLogger _logger = new(nameof(ScrapUtils));

    public static List<GrabbableObject> GetAllScrapInShip()
    {
        try
        {
            return GameUtils.ShipGameObject
                .GetComponentsInChildren<GrabbableObject>()
                .ToList();
        }
        catch
        {
            return new List<GrabbableObject>();
        }
    }

    public static List<GrabbableObject> GetAllSellableScrapInShip()
    {
        return GetAllScrapInShip()
            .Where(scrap => scrap.CanSellItem())
            .ToList();
    }

    public static List<GrabbableObject> GetScrapForAmount(int amount)
    {
        var totalScrapValue = GetShipTotalSellableScrapValue();
        if (totalScrapValue < amount)
        {
            return new List<GrabbableObject>();
        }

        var nextScrapIndex = 0;
        var allScrap = GetAllSellableScrapInShip()
            .OrderByDescending(scrap => scrap.itemProperties.twoHanded)
            .ThenByDescending(scrap => scrap.scrapValue)
            .ToList();

        var scrapToSell = new List<GrabbableObject>();

        while (amount > 300) // arbitrary amount until it starts to specifically look for a perfect match, favouring 2handed scrap first
        {
            var nextScrap = allScrap[nextScrapIndex++];
            scrapToSell.Add(nextScrap);
            amount -= nextScrap.ActualSellValue();
        }

        // Time to actually be precise
        allScrap = allScrap.Skip(nextScrapIndex)
            .OrderBy(scrap => scrap.scrapValue)
            .ToList();
        nextScrapIndex = 0;

        // When trying last few OR a very low amount (eg sell 10), just see if it's less than the cheapest item in 'allScrap' list
        if (amount < allScrap.Last().ActualSellValue())
        {
            scrapToSell.Add(allScrap.Last());
            return scrapToSell;
        }

        while (amount > 0)
        {
            var scrapCombinations = new List<(GrabbableObject First, GrabbableObject Second)>();
            for (var currentIndex = nextScrapIndex; currentIndex < allScrap.Count; currentIndex++)
            {
                for (var nextIndex = currentIndex + 1; nextIndex < allScrap.Count; nextIndex++)
                {
                    scrapCombinations.Add((allScrap[currentIndex], allScrap[nextIndex]));
                }
            }

            var matchingSumForAmountRemaining = scrapCombinations.FirstOrDefault(combo => combo.First.ActualSellValue() + combo.Second.ActualSellValue() >= amount);
            if (matchingSumForAmountRemaining != default)
            {
                scrapToSell.Add(matchingSumForAmountRemaining.First);
                scrapToSell.Add(matchingSumForAmountRemaining.Second);
                return scrapToSell;
            }

            var nextScrap = allScrap[nextScrapIndex++];
            scrapToSell.Add(nextScrap);
            amount -= nextScrap.ActualSellValue();
        }

        return scrapToSell;
    }

    public static int GetShipSellableScrapCount() => GetAllSellableScrapInShip().Count;

    public static int GetShipTotalSellableScrapValue() => GetAllSellableScrapInShip().Sum(scrap => scrap.ActualSellValue());

    public static int GetShipTotalRawScrapValue()
    {
        return GetAllScrapInShip()
            .Where(scrap => scrap.itemProperties.isScrap && scrap.scrapValue > 0)
            .Sum(scrap => scrap.scrapValue);
    }

    public static int GetShipSettledTotalRawScrapValue()
    {
        return GetAllScrapInShip()
               .Where(scrap => scrap.itemProperties.isScrap && scrap.scrapValue > 0 && !scrap.isHeld)
               .Sum(scrap => scrap.scrapValue);
    }

    public static int SumScrapListSellValue(IEnumerable<GrabbableObject> scraps) => scraps.Where(item => item.itemProperties.isScrap).Sum(scrap => scrap.ActualSellValue());

    public static bool CanSellItem(this GrabbableObject item)
    {
        if (item is null)
        {
            _logger.LogError("CanSellItem: Trying to evaluate a null item");
            return false;
        }

        var canSell = item.itemProperties.isScrap && item.scrapValue > 0 && !item.isHeld;
        var isInIgnoreList = Plugin.Instance.PluginConfig.SellIgnoreList
            .Split(",")
            .Select(x => x.Trim())
            .Select(x => Regex.Match(item.itemProperties.name, x, RegexOptions.IgnoreCase))
            .Any(match => match.Success);

        return canSell && !isInIgnoreList;
    }

    public static int ActualSellValue(this GrabbableObject scrap)
    {
        var actualSellValue = scrap.scrapValue * StartOfRound.Instance.companyBuyingRate;
        return (int)Math.Round(actualSellValue); // Not sure if Game ceils/floors/rounds, so might be off by 1 at most
    }
}
