using BepInEx;
using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace QualityCompany.Utils;

/// <summary>
/// A scrap utilities class
/// </summary>
public static class ScrapUtils
{
    private static readonly ModLogger Logger = new(nameof(ScrapUtils));

    /// <summary>
    /// Get all scrap items within the ship
    /// </summary>
    /// <returns></returns>
    public static List<GrabbableObject> GetAllScrapInShip()
    {
        try
        {
            return GameUtils.ShipGameObject
                .GetComponentsInChildren<GrabbableObject>()
                .Where(scrap => scrap.itemProperties.isScrap && scrap.scrapValue > 0)
                .ToList();
        }
        catch
        {
            return new List<GrabbableObject>();
        }
    }

    /// <summary>
    /// Get all sellable scrap items within the ship<br />
    /// This takes into account items ignored by passed in includeList
    /// </summary>
    /// <returns></returns>
    public static List<GrabbableObject> GetAllIncludedScrapInShip(string includeList)
    {
        return GetAllScrapInShip()
            .Where(scrap => scrap.CanIncludeItem(includeList))
            .ToList();
    }

    public static int GetShipTotalRawScrapValue()
    {
        return GetAllScrapInShip()
            .ScrapValueOfCollection();
    }

    public static int GetShipTotalIncludedScrapValue(string includeList)
    {
        return GetAllIncludedScrapInShip(includeList)
            .ActualScrapValueOfCollection();
    }

    public static int GetShipSettledTotalRawScrapValue()
    {
        return GetAllScrapInShip()
            .Where(scrap => !scrap.isHeld)
            .ScrapValueOfCollection();
    }

    public static int SumScrapListSellValue(IEnumerable<GrabbableObject> scraps)
    {
        return scraps.Where(item => item.itemProperties.isScrap).ActualScrapValueOfCollection();
    }

    /// <summary>
    /// Retrieves the sell value of a scrap item according to the current buying rate of The Company<br />
    /// This should not be used to calculate the total sum of a list of scrap items. Use <see cref="ActualScrapValueOfCollection"/> instead
    /// </summary>
    /// <param name="scrap">The scrap item to evaluate</param>
    /// <returns>The scrap value of a scrap item</returns>
    public static int ActualSellValue(this GrabbableObject scrap)
    {
        var actualSellValue = scrap.scrapValue * GameUtils.StartOfRound.companyBuyingRate;
        return (int)Math.Round(actualSellValue);
    }

    /// <summary>
    /// Retrieves the raw total scrap value of a collection, not taking into account the current buying rate of The Company<br />
    /// </summary>
    /// <param name="scraps">The scrap items to evaluate</param>
    /// <returns>The total raw scrap value of all scrap items in the collection</returns>
    public static int ScrapValueOfCollection(this IEnumerable<GrabbableObject> scraps)
    {
        return scraps.Sum(go => go.scrapValue);
    }

    /// <summary>
    /// Retrieves the total scrap value of a collection, taking into account the current buying rate of The Company<br />
    /// This is the preferred way to calculate the sum of a collection of items as opposed to calling <see cref="ActualSellValue"/> on each individual scrap item
    /// </summary>
    /// <param name="scraps">The scrap items to evaluate</param>
    /// <returns>The total scrap value of all scrap items in the collection</returns>
    public static int ActualScrapValueOfCollection(this IEnumerable<GrabbableObject> scraps)
    {
        return (int)(scraps.Sum(go => go.scrapValue) * GameUtils.StartOfRound.companyBuyingRate);
    }

    public static bool CanIncludeItem(this GrabbableObject? item, string includeList)
    {
        if (item is null)
        {
            Logger.LogWarning("CanIncludeItem: Trying to evaluate a null item");
            return false;
        }

        if (includeList.IsNullOrWhiteSpace())
        {
            Logger.LogWarning("CanIncludeItem: includeList is empty");
            return false;
        }

        var canSell = item.itemProperties.isScrap && item is { scrapValue: > 0, isHeld: false };
        var isInIgnoreList = includeList
            .Split(",")
            .Select(x => x.Trim())
            .Select(x => Regex.Match(item.itemProperties.name, x, RegexOptions.IgnoreCase))
            .Any(match => match.Success);

        return canSell && !isInIgnoreList;
    }

    #region Internal use only
    internal static List<GrabbableObject> GetAllSellableScrapInShip()
    {
        return GetAllIncludedScrapInShip(Plugin.Instance.PluginConfig.SellIgnoreList);
    }

    internal static int GetShipTotalSellableScrapValue() => GetAllSellableScrapInShip().ActualScrapValueOfCollection();

    internal static List<GrabbableObject> GetScrapForAmount(int amount)
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
    #endregion
}
