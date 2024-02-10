using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Network;
using QualityCompany.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QualityCompany.TerminalCommands;

internal class SellCommands
{
    public static List<GrabbableObject> recommendedScraps = new();
    public static int sellScrapFor;
    public static int sellScrapActualTarget;

    [TerminalCommand]
    private static TerminalCommandBuilder Run()
    {
        if (!Plugin.Instance.PluginConfig.TerminalSellCommandsEnabled) return null;

        return new TerminalCommandBuilder("sell")
            .WithDescription(">SELL [ALL|QUOTA|TARGET|2h|<AMOUNT>|<ITEM>]\nTo sell items on the ship.")
            .WithText("Please enter [ALL|QUOTA|TARGET|2h|<AMOUNT>|<ITEM>]")
            .WithSubCommand(new TerminalSubCommandBuilder("all")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell ALL scrap ([shipTotalScrapCount]) for $[shipTotalScrapValue] credits.")
                .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for $[shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems")
                .WithPreAction(() => sellScrapFor = ScrapUtils.GetShipTotalSellableScrapValue())
                .WithAction(() =>
                {
                    NetworkHandler.Instance.SellAllScrapServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("quota")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current quota ($[sellScrapFor] credits) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for $[shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap", "quotaAlreadyMet")
                .WithPreAction(() =>
                {
                    sellScrapFor = TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;
                    recommendedScraps = ScrapUtils.GetScrapForAmount(sellScrapFor);
                })
                .WithAction(() =>
                {
                    foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                    {
                        NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    }

                    NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("target")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current target ($[sellScrapTarget], needing $[sellScrapFor]) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for $[shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("targetCommandDisabled", "landedAtCompany", "hasScrapItems", "notEnoughScrap", "targetAlreadyMet")
                .WithPreAction(() =>
                {
                    sellScrapActualTarget = OvertimeMonitor.targetTotalCredits;
                    sellScrapFor = OvertimeMonitor.targetNeeded;
                    recommendedScraps = ScrapUtils.GetScrapForAmount(sellScrapFor);
                })
                .WithAction(() =>
                {
                    foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                    {
                        NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    }

                    NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("<amount>")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to $[sellScrapFor] as possible...\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap")
                .WithInputMatch(@"^(\d+)$")
                .WithPreAction(input =>
                {
                    sellScrapFor = Convert.ToInt32(input);

                    if (sellScrapFor <= 0) return false;

                    recommendedScraps = ScrapUtils.GetScrapForAmount(sellScrapFor);

                    // Nothing found, return notEnoughScrapNode
                    if (recommendedScraps.Count == 0) return false;

                    // A combination has been found, return info with confirm/deny node
                    return true;
                })
                .WithAction(() =>
                {
                    foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                    {
                        NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    }

                    NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("2h")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell all two-handed scrap.\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap")
                .WithPreAction(() =>
                {
                    recommendedScraps = ScrapUtils.GetAllSellableScrapInShip()
                        .Where(x => x.itemProperties.twoHanded).ToList();
                })
                .WithAction(() =>
                {
                    foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                    {
                        NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    }

                    NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("<sell_item>")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell specified items.\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "hasMatchingScrapItems")
                .WithInputMatch(@"^(\w+)$")
                .WithPreAction(input =>
                {
                    recommendedScraps = ScrapUtils.GetAllScrapInShip()
                        .Where(x => x.itemProperties.name.ToLower().Contains(input))
                        .ToList();

                    return true;
                })
                .WithAction(() =>
                {
                    foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                    {
                        NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    }

                    NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .AddTextReplacement("[sellScrapFor]", () => sellScrapFor.ToString())
            .AddTextReplacement("[sellScrapTarget]", () => sellScrapActualTarget.ToString())
            .AddTextReplacement("[numScrapSold]", () => recommendedScraps.Count.ToString())
            .AddTextReplacement("[shipTotalScrapCount]", () => ScrapUtils.GetShipSellableScrapCount().ToString())
            .AddTextReplacement("[shipTotalScrapValue]", () => ScrapUtils.GetShipTotalSellableScrapValue().ToString())
            .AddTextReplacement("[sellScrapActualTotal]", () => ScrapUtils.SumScrapListSellValue(recommendedScraps).ToString())
            .AddTextReplacement("[companyBuyItemsCombo]", () => recommendedScraps?.Select(x => $"{x.itemProperties.name}: {x.ActualSellValue()}").Aggregate((first, next) => $"{first}\n{next}"))
            .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
            .WithCondition("hasScrapItems", "Bruh, you don't even have any items.", () => ScrapUtils.GetShipSellableScrapCount() > 0)
            .WithCondition("notEnoughScrap", "Not enough scrap to meet [sellScrapFor] credits.\nTotal value: [shipTotalScrapValue].", () => sellScrapFor < ScrapUtils.GetShipTotalSellableScrapValue())
            .WithCondition("quotaAlreadyMet", "Quota already met.", () => TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0)
            .WithCondition("hasMatchingScrapItems", "No matching items found for input.", () => recommendedScraps.Count > 0)
            .WithCondition("targetCommandDisabled", "Target command has been disabled", () => Plugin.Instance.PluginConfig.TerminalTargetCommandsEnabled);
        
    }
}

