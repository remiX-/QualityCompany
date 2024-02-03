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
        // Sanity check that we actually have enough
        // Technically should never happen since terminal doesn't allow player to sell scrap if it won't meet the amount
        var totalScrapValue = GetShipTotalSellableScrapValue();
        if (totalScrapValue < amount)
        {
            _logger.LogMessage($"Cannot reach required amount of {amount}! Total value: {totalScrapValue}, total num scrap: {GetShipSellableScrapCount()}");
            return new List<GrabbableObject>();
        }

        var nextScrapIndex = 0;
        var allScrap = GetAllSellableScrapInShip()
            .OrderByDescending(scrap => scrap.itemProperties.twoHanded)
            .ThenByDescending(scrap => scrap.scrapValue)
            .ToList();

        var scrapForQuota = new List<GrabbableObject>();

        var amountNeeded = amount;
        // Highest value scrap is 210
        while (amountNeeded > 210)
        {
            var nextScrap = allScrap[nextScrapIndex++];
            scrapForQuota.Add(nextScrap);
            amountNeeded -= nextScrap.ActualSellValue();
        }

        // Time to actually be precise
        allScrap = allScrap.Skip(nextScrapIndex)
            .OrderBy(scrap => scrap.scrapValue)
            .ToList();
        nextScrapIndex = 0;

        if (amountNeeded < allScrap.Last().ActualSellValue())
        {
            scrapForQuota.Add(allScrap.Last());
            return scrapForQuota;
        }

        while (amountNeeded > 0)
        {
            var sums = new List<(GrabbableObject first, GrabbableObject second, int total)>();
            for (var i = nextScrapIndex; i < allScrap.Count; i++)
            {
                for (var j = i + 1; j < allScrap.Count; j++)
                {
                    sums.Add((allScrap[i], allScrap[j], allScrap[i].ActualSellValue() + allScrap[j].ActualSellValue()));
                }
            }

            var foundSum = sums.FirstOrDefault(sum => sum.total >= amountNeeded);
            if (foundSum != default)
            {
                scrapForQuota.Add(foundSum.first);
                scrapForQuota.Add(foundSum.second);
                return scrapForQuota;
            }

            var nextScrap = allScrap[nextScrapIndex++];
            scrapForQuota.Add(nextScrap);
            amountNeeded -= nextScrap.ActualSellValue();
        }

        return scrapForQuota;
    }

    public static int GetShipSellableScrapCount() => GetAllSellableScrapInShip().Count;

    public static int GetShipTotalSellableScrapValue() => GetAllSellableScrapInShip().Sum(scrap => scrap.ActualSellValue());

    public static int GetShipTotalRawScrapValue() => GetAllScrapInShip().Sum(scrap => scrap.scrapValue);

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
            _logger.LogDebug("CanSellItem: Trying to evaluate a null item");
            return false;
        }

        var canSell = item.itemProperties.isScrap && item.scrapValue > 0 && !item.isHeld;
        var isInIgnoreList = Plugin.Instance.PluginConfig.ConfigSellIgnoreList
            .Split(",")
            .Select(x => x.Trim())
            .Select(x => Regex.Match(item.itemProperties.name, x, RegexOptions.IgnoreCase))
            .Any(match => match.Success);

        return canSell && !isInIgnoreList;
    }

    public static int ActualSellValue(this GrabbableObject scrap)
    {
        var actualSellValue = scrap.scrapValue * StartOfRound.Instance.companyBuyingRate;
        return (int)Math.Round(actualSellValue);
    }
}
