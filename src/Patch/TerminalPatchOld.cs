namespace AdvancedCompany.Patch;

public class TerminalPatchOld
{
    public static void Awake()
    {
        #region old
        // specialQuotaAlreadyMetNode = new TerminalNode
        // {
        //     name = "quotaAlreadyMet",
        //     displayText = "Quota already met.\n\n\n",
        //     clearPreviousText = true
        // };
        // specialNotEnoughScrapNode = new TerminalNode
        // {
        //     name = "notEnoughScrap",
        //     displayText = "Not enough scrap to meet [sellScrapFor] credits.\nTotal value: [shipTotalScrapValue]\n\n\n",
        //     clearPreviousText = true
        // };
        // specialCanOnlySellAtCompanyNode = new TerminalNode
        // {
        //     name = "onlySellAtCompany",
        //     displayText = "ERROR: Usage of this feature is only permitted within Company bounds\n\nPlease land at 71-Gordion and repeat command.\n\n\n",
        //     clearPreviousText = true
        // };
        //
        // var sellQuotaConfirmNode = new TerminalNode
        // {
        //     name = "sellQuotaConfirm",
        //     displayText = "Transaction complete.\nSold [numScrapSold] scrap for [sellScrapFor] credits.\n\nThe company is not responsible for any calculation errors.\n\n\n",
        //     clearPreviousText = true,
        //     terminalEvent = "sellQuota"
        // };
        // var sellQuotaDenyNode = new TerminalNode
        // {
        //     name = "sellQuotaDeny",
        //     displayText = "Transaction cancelled.\n\n\n",
        //     clearPreviousText = true
        // };
        // var sellQuotaNode = new TerminalNode
        // {
        //     name = "sellQuota",
        //     displayText = "[companyBuyingRateWarning]Requesting to sell scrap as close to current quota ([sellScrapFor] credits) as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo][confirmOrDeny]",
        //     isConfirmationNode = true,
        //     clearPreviousText = true,
        //     overrideOptions = true,
        //     terminalOptions = new[] {
        //             new CompatibleNoun {
        //                 noun = confirmKeyword,
        //                 result = sellQuotaConfirmNode
        //             },
        //             new CompatibleNoun {
        //                 noun = denyKeyword,
        //                 result = sellQuotaDenyNode
        //             }
        //         }
        // };
        //
        // var sellAllConfirmNode = new TerminalNode
        // {
        //     name = "sellAllConfirm",
        //     displayText = "Transaction complete. Sold [shipTotalScrapCount] scrap for [shipTotalScrapValue] credits.\n\nThe company is not responsible for any calculation errors.\n\n\n",
        //     clearPreviousText = true,
        //     terminalEvent = "sellAll"
        // };
        // var sellAllDenyNode = new TerminalNode
        // {
        //     name = "sellAllConfirm",
        //     displayText = "Transaction cancelled.\n\n\n",
        //     clearPreviousText = true
        // };
        // var sellAllNode = new TerminalNode
        // {
        //     name = "sellAll",
        //     displayText = "[companyBuyingRateWarning]Requesting to sell ALL scrap ([shipTotalScrapCount]) for [shipTotalScrapValue] credits.[confirmOrDeny]",
        //     isConfirmationNode = true,
        //     clearPreviousText = true,
        //     overrideOptions = true,
        //     terminalOptions = new[] {
        //             new CompatibleNoun {
        //                 noun = confirmKeyword,
        //                 result = sellAllConfirmNode
        //             },
        //             new CompatibleNoun {
        //                 noun = denyKeyword,
        //                 result = sellAllDenyNode
        //             }
        //         }
        // };
        //
        // var sellAmountConfirmNode = new TerminalNode
        // {
        //     name = "sellAmountConfirm",
        //     displayText = "Sold [numScrapSold] scrap for [sellScrapActualTotal].\n\nThe company is not responsible for any calculation errors.\n\n\n",
        //     clearPreviousText = true,
        //     terminalEvent = "sellAmount"
        // };
        // var sellAmountDenyNode = new TerminalNode
        // {
        //     name = "sellAllConfirm",
        //     displayText = "Transaction cancelled.\n\n\n",
        //     clearPreviousText = true
        // };
        // sellAmountNode = new TerminalNode
        // {
        //     name = "sellAmount",
        //     displayText = "[companyBuyingRateWarning]Requesting to sell scrap as close to [sellScrapFor] credits as possible...\nThe Company wants the follow items for a total of [sellScrapActualTotal]:\n[companyBuyItemsCombo][confirmOrDeny]",
        //     isConfirmationNode = true,
        //     clearPreviousText = true,
        //     overrideOptions = true,
        //     terminalOptions = new[] {
        //             new CompatibleNoun {
        //                 noun = confirmKeyword,
        //                 result = sellAmountConfirmNode
        //             },
        //             new CompatibleNoun {
        //                 noun = denyKeyword,
        //                 result = sellAmountDenyNode
        //             }
        //         }
        // };
        //
        // var allKeyword = new TerminalKeyword
        // {
        //     name = "All",
        //     word = "all"
        // };
        //
        // var quotaKeyword = new TerminalKeyword
        // {
        //     name = "Quota",
        //     word = "quota"
        // };
        //
        // var sellKeyword = new TerminalKeyword
        // {
        //     name = "Sell",
        //     word = "sell",
        //     isVerb = true,
        //     compatibleNouns = new[] {
        //             new CompatibleNoun {
        //                 noun = allKeyword,
        //                 result = sellAllNode
        //             },
        //             new CompatibleNoun {
        //                 noun = quotaKeyword,
        //                 result = sellQuotaNode
        //             }
        //         }
        // };
        //
        // allKeyword.defaultVerb = sellKeyword;
        // quotaKeyword.defaultVerb = sellKeyword;
        //
        // __instance.terminalNodes.allKeywords = __instance.terminalNodes.allKeywords.AddRangeToArray(new[] { sellKeyword, allKeyword, quotaKeyword });
        //
        // var otherCommandsNode = __instance.terminalNodes.allKeywords.First(node => node.name == "Other").specialKeywordResult;
        // otherCommandsNode.displayText = otherCommandsNode.displayText.Substring(0, otherCommandsNode.displayText.Length - 1) + ">SELL [ALL|QUOTA|<AMOUNT>]\nTo sell items on the ship.\n\n\n";
        #endregion
    }
}
