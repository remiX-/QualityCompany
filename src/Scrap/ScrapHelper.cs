using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedCompany.Game;

namespace AdvancedCompany.Scrap;

public static class ScrapHelpers
{
    public static List<GrabbableObject> GetAllScrapInShip()
    {
        return GameUtils.ShipGameObject
            .GetComponentsInChildren<GrabbableObject>()
            .Where(scrap => scrap.CanSellItem())
            .ToList();
    }

    public static List<GrabbableObject> GetScrapForAmount(int amount)
    {
        // Sanity check that we actually have enough
        // Technically should never happen since terminal doesn't allow player to sell scrap if it won't meet the amount
        var totalScrapValue = GetTotalScrapValueInShip();
        if (totalScrapValue < amount)
        {
            Logger.LogMessage($"Cannot reach required amount of {amount}! Total value: {totalScrapValue}, total num scrap: {CountAllScrapInShip()}");
            return new List<GrabbableObject>();
        }

        var nextScrapIndex = 0;
        var allScrap = GetAllScrapInShip()
            .OrderByDescending(scrap => scrap.itemProperties.twoHanded)
            .ThenByDescending(scrap => scrap.scrapValue)
            // .ThenBy(scrap => scrap.NetworkObjectId)
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
            // .ThenBy(scrap => scrap.NetworkObjectId)
            .ToList();
        nextScrapIndex = 0;

        // Check if sell amount request is smaller than the lowest value scrap
        if (amountNeeded < allScrap.Last().ActualSellValue())
        {
            scrapForQuota.Add(allScrap.Last());
            return scrapForQuota;
        }

        while (amountNeeded > 0)
        {
            var sums = new List<(GrabbableObject first, GrabbableObject second, int sum)>();
            for (var i = nextScrapIndex; i < allScrap.Count; i++)
            {
                for (var j = i + 1; j < allScrap.Count; j++)
                {
                    // Starting second loop at i+1 lets us skip redundant sums
                    sums.Add((allScrap[i], allScrap[j], allScrap[i].ActualSellValue() + allScrap[j].ActualSellValue()));
                }
            }

            var foundSum = sums.FirstOrDefault(sum => sum.sum >= amountNeeded);
            if (foundSum != default)
            {
                scrapForQuota.Add(foundSum.first);
                scrapForQuota.Add(foundSum.second);
                return scrapForQuota;
            }

            // If we haven't found a sum, we take the next scrap and continue our sums
            var nextScrap = allScrap[nextScrapIndex++];
            scrapForQuota.Add(nextScrap);
            amountNeeded -= nextScrap.ActualSellValue();
        }

        // Worst case scenario, we found no sums :(
        // Whatever we have will have to do
        Logger.LogMessage("Couldn't find a way to perfectly meet quota :(");
        return scrapForQuota;
    }

    public static int CountAllScrapInShip() => GetAllScrapInShip().Count;

    public static int GetTotalScrapValueInShip() => GetAllScrapInShip().Sum(scrap => scrap.ActualSellValue());

    public static int SumScrapListSellValue(IEnumerable<GrabbableObject> scraps) => scraps.Where(item => item.itemProperties.isScrap).Sum(scrap => scrap.ActualSellValue());

    public static bool CanSellItem(this GrabbableObject item)
    {
        var canSell = item.itemProperties.isScrap && item.scrapValue > 0 && !item.isHeld;
        var isInIgnoreList = Plugin.ConfigSellIgnoreList.Value
            .Split(",")
            .Select(x => x.Trim())
            .Select(x => Regex.Match(x, item.itemProperties.name, RegexOptions.IgnoreCase))
            .Any(match => match.Success);

        return canSell && !isInIgnoreList;
    }

    public static int ActualSellValue(this GrabbableObject scrap)
    {
        var actualSellValue = scrap.scrapValue * StartOfRound.Instance.companyBuyingRate;
        return (int)Math.Round(actualSellValue);
    }
}
