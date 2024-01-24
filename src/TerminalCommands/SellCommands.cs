using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedCompany.Game;
using AdvancedCompany.Manager;
using AdvancedCompany.Network;
using AdvancedCompany.Scrap;

namespace AdvancedCompany.TerminalCommands;

internal class SellCommands : ITerminalSubscriber
{
    public static List<GrabbableObject> recommendedScraps = new();
    public static int sellScrapFor;

    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(Terminal), "Awake")]
    public void Run()
    {
        TerminalManager.AddCommand(
            new TerminalCommandBuilder("sell")
                .WithDescription(">SELL [ALL|QUOTA|<AMOUNT>]\nTo sell items on the ship.")
                .WithText("Please enter [all|quota|<amount>]")
                .SetConfirmMessage("This should never appear")
                .WithSubCommand(new TerminalSubCommandBuilder("all")
                    .WithMessage("[companyBuyingRateWarning]Requesting to sell ALL scrap ([shipTotalScrapCount]) for [shipTotalScrapValue] credits.")
                    .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for [shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                    .WithConditions("landedAtCompany", "hasScrapItems")
                    .WithPreAction(() => sellScrapFor = ScrapHelpers.GetTotalScrapValueInShip())
                    .WithAction(() =>
                    {
                        NetworkHandler.Instance.SellAllScrapServerRpc();
                    })
                )
                .WithSubCommand(new TerminalSubCommandBuilder("quota")
                    .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current quota ([sellScrapFor] credits) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                    .EnableConfirmDeny(confirmMessage: "Transaction complete. Sold [shipTotalScrapCount] scrap for [shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.")
                    .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap", "quotaAlreadyMet")
                    .WithPreAction(() =>
                    {
                        sellScrapFor = TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled;
                        recommendedScraps = ScrapHelpers.GetScrapForAmount(sellScrapFor);
                    })
                    .WithAction(() =>
                    {
                        Logger.LogDebug($"SELL CMD: NH.Inst => {NetworkHandler.Instance == null}");
                        foreach (var scrapNetworkObjectId in recommendedScraps.Select(x => x.NetworkObjectId))
                        {
                            NetworkHandler.Instance.TargetSellForNetworkObjectServerRpc(scrapNetworkObjectId);
                        }
                        
                        NetworkHandler.Instance.ExecuteSellAmountServerRpc();
                    })
                )
                .WithSubCommand(new TerminalSubCommandBuilder("<amount>")
                    .WithMessage("[companyBuyingRateWarning]Requesting to sell scrap as close to current quota ([sellScrapFor] credits) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo]")
                    .EnableConfirmDeny(confirmMessage: "Sold [numScrapSold] scrap for [sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.")
                    .WithConditions("landedAtCompany", "hasScrapItems", "notEnoughScrap")
                    .WithInputMatch(@"(\d+$)$")
                    .WithPreAction(input =>
                    {
                        sellScrapFor = Convert.ToInt32(input);
                
                        if (sellScrapFor <= 0) return false;
                
                        recommendedScraps = ScrapHelpers.GetScrapForAmount(sellScrapFor);
                
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
                .AddTextReplacement("[sellScrapFor]", () => sellScrapFor.ToString())
                .AddTextReplacement("[numScrapSold]", () => recommendedScraps.Count.ToString())
                .AddTextReplacement("[shipTotalScrapCount]", () => ScrapHelpers.CountAllScrapInShip().ToString())
                .AddTextReplacement("[shipTotalScrapValue]", () => ScrapHelpers.GetTotalScrapValueInShip().ToString())
                .AddTextReplacement("[sellScrapActualTotal]", () => ScrapHelpers.SumScrapListSellValue(recommendedScraps).ToString())
                .AddTextReplacement("[companyBuyItemsCombo]", () => recommendedScraps.Select(x => $"{x.itemProperties.name}: {x.ActualSellValue()}").Aggregate((first, next) => $"{first}\n{next}"))
                .WithCondition("landedAtCompany", "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.", GameUtils.IsLandedOnCompany)
                .WithCondition("hasScrapItems", "Bruh, you don't even have any items.", () => ScrapHelpers.CountAllScrapInShip() > 0)
                .WithCondition("notEnoughScrap", "Not enough scrap to meet [sellScrapFor] credits.\nTotal value: [shipTotalScrapValue].", () => sellScrapFor < ScrapHelpers.GetTotalScrapValueInShip())
                .WithCondition("quotaAlreadyMet", "Quota already met.", () => TimeOfDay.Instance.profitQuota - TimeOfDay.Instance.quotaFulfilled > 0)
        );
    }
}

