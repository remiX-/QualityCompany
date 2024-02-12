using QualityCompany.Manager;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QualityCompany.TerminalCommands;

internal class SellCommands
{
    private static List<GrabbableObject> _recommendedScraps = new();
    private static int _sellScrapFor;
    private static int _sellScrapActualTarget;

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
                .WithPreAction(() => _sellScrapFor = ScrapUtils.GetShipTotalSellableScrapValue())
                .WithAction(() =>
                {
                    TargetManager.SellAllScrap();
                    // NetworkHandler.Instance.SellAllScrapServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("quota")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current quota ($[sellScrapFor] credits) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for $[shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap", "quotaAlreadyMet")
                .WithPreAction(() =>
                {
                    _sellScrapFor = TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;
                    _recommendedScraps = ScrapUtils.GetScrapForAmount(_sellScrapFor);
                })
                .WithAction(() =>
                {
                    TargetManager.SellAllTargetedScrap(_recommendedScraps);
                    // foreach (var scrapNetworkObjectId in _recommendedScraps.Select(x => x.NetworkObjectId))
                    // {
                    //     NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                    // }
                    //
                    // NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("target")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current target ($[sellScrapTarget], needing $[sellScrapFor]) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for $[shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("targetCommandDisabled", "landedAtCompany", "hasScrapItems", "notEnoughScrap", "targetAlreadyMet")
                .WithPreAction(() =>
                {
                    _sellScrapActualTarget = OvertimeMonitor.targetTotalCredits;
                    _sellScrapFor = OvertimeMonitor.targetNeeded;
                    _recommendedScraps = ScrapUtils.GetScrapForAmount(_sellScrapFor);
                })
                .WithAction(() =>
                {
                    TargetManager.SellAllTargetedScrap(_recommendedScraps);
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("<amount>")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to $[sellScrapFor] as possible...\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap")
                .WithInputMatch(@"^(\d+)$")
                .WithPreAction(input =>
                {
                    _sellScrapFor = Convert.ToInt32(input);

                    if (_sellScrapFor <= 0) return false;

                    _recommendedScraps = ScrapUtils.GetScrapForAmount(_sellScrapFor);

                    // Nothing found, return notEnoughScrapNode
                    if (_recommendedScraps.Count == 0) return false;

                    // A combination has been found, return info with confirm/deny node
                    return true;
                })
                .WithAction(() =>
                {
                    TargetManager.SellAllTargetedScrap(_recommendedScraps);
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("2h")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell all two-handed scrap.\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap")
                .WithPreAction(() =>
                {
                    _recommendedScraps = ScrapUtils.GetAllSellableScrapInShip()
                        .Where(x => x.itemProperties.twoHanded).ToList();
                })
                .WithAction(() =>
                {
                    TargetManager.SellAllTargetedScrap(_recommendedScraps);
                })
            )
            .WithSubCommand(new TerminalSubCommandBuilder("<sell_item>")
                .WithMessage("[companyBuyingRateWarning]Requesting to sell specified items.\n\nThe Company wants the follow items for a total of $[sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for $[sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                .WithConditions("landedAtCompany", "hasScrapItems", "hasMatchingScrapItems")
                .WithInputMatch(@"^(\w+)$")
                .WithPreAction(input =>
                {
                    _recommendedScraps = ScrapUtils.GetAllScrapInShip()
                        .Where(x => x.itemProperties.name.ToLower().Contains(input))
                        .ToList();

                    return true;
                })
                .WithAction(() =>
                {
                    TargetManager.SellAllTargetedScrap(_recommendedScraps);
                })
            )
            .AddTextReplacement("[sellScrapFor]", () => _sellScrapFor.ToString())
            .AddTextReplacement("[sellScrapTarget]", () => _sellScrapActualTarget.ToString())
            .AddTextReplacement("[numScrapSold]", () => _recommendedScraps.Count.ToString())
            .AddTextReplacement("[shipTotalScrapCount]", () => ScrapUtils.GetShipSellableScrapCount().ToString())
            .AddTextReplacement("[shipTotalScrapValue]", () => ScrapUtils.GetShipTotalSellableScrapValue().ToString())
            .AddTextReplacement("[sellScrapActualTotal]", () => ScrapUtils.SumScrapListSellValue(_recommendedScraps).ToString())
            .AddTextReplacement("[companyBuyItemsCombo]", () => _recommendedScraps?.Select(x => $"{x.itemProperties.name}: {x.ActualSellValue()}").Aggregate((first, next) => $"{first}\n{next}"))
            .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
            .WithCondition("hasScrapItems", "Bruh, you don't even have any items.", () => ScrapUtils.GetShipSellableScrapCount() > 0)
            .WithCondition("notEnoughScrap", "Not enough scrap to meet [sellScrapFor] credits.\nTotal value: [shipTotalScrapValue].", () => _sellScrapFor < ScrapUtils.GetShipTotalSellableScrapValue())
            .WithCondition("quotaAlreadyMet", "Quota already met.", () => TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0)
            .WithCondition("hasMatchingScrapItems", "No matching items found for input.", () => _recommendedScraps.Count > 0)
            .WithCondition("targetCommandDisabled", "Target command has been disabled", () => Plugin.Instance.PluginConfig.TerminalTargetCommandsEnabled);

    }
}

