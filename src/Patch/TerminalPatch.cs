using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedCompany.Manager;
using AdvancedCompany.Network;
#pragma warning disable IDE0060
#pragma warning disable Harmony003

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(Terminal))]
public class TerminalPatch
{
    // Hack so we can display how much the scrap sold for without recalculating it
    public static List<GrabbableObject> recommendedScraps = new();
    public static int sellScrapFor;

    // private static TerminalNode sellAmountNode;
    // private static TerminalNode specialQuotaAlreadyMetNode;
    // private static TerminalNode specialNotEnoughScrapNode;
    // private static TerminalNode specialCanOnlySellAtCompanyNode;
    private static bool patchedTerminal;

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void Awake(Terminal __instance)
    {
        Logger.LogDebug("TerminalPatch.Awake");
        if (patchedTerminal)
        {
            return;
        }

        TerminalManager.SetTerminalInstance(__instance);

        TerminalManager.AddGlobalTextReplacement("[companyBuyingRateWarning]", () => Math.Abs(StartOfRound.Instance.companyBuyingRate - 1f) == 0f
            ? ""
            : $"WARNING: Company buying rate is currently at {StartOfRound.Instance.companyBuyingRate:P0}\n\n");
        TerminalManager.AddGlobalTextReplacement("[confirmOrDeny]", () => "\n\nPlease CONFIRM or DENY.\n\n\n");

        TerminalManager.ApplyToTerminal();

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

        patchedTerminal = true;
    }


    // public static bool TryParseSellAmount(string input)
    // {
    //     // Logger.LogDebug($"TryParseSellAmount: {__instance.screenText.text} | {__instance.textAdded}");
    //     // var terminalInput = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
    //
    //     var regex = new Regex(@"^sell (\d+$)$");
    //     var match = regex.Match(input.ToLower());
    //     if (!match.Success) return __result;
    //     sellScrapFor = Convert.ToInt32(match.Groups[1].Value);
    //
    //     if (sellScrapFor <= 0) return __result;
    //
    //     recommendedScraps = ScrapHelpers.GetScrapForAmount(sellScrapFor);
    //
    //     // Nothing found, return notEnoughScrapNode
    //     if (recommendedScraps.Count == 0) return specialNotEnoughScrapNode;
    //
    //     // A combination has been found, return info with confirm/deny node
    //     return sellAmountNode;
    // }

    [HarmonyPostfix]
    [HarmonyPatch("TextPostProcess")]
    public static string ProcessCustomText(string __result)
    {
        Logger.LogDebug($"TerminalPatch.ProcessCustomText: {__result}");
        foreach (var (key, func) in TerminalManager._textReplacements)
        {
            if (!__result.Contains(key)) continue;

            __result = __result.Replace(key, func());
        }

        foreach (var command in TerminalManager.commands)
        {
            foreach (var (key, func) in command.textProcessPlaceholders)
            {
                if (!__result.Contains(key)) continue;

                __result = __result.Replace(key, func());
            }
        }

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ParsePlayerSentence")]
    public static TerminalNode TryReturnSpecialNodes(TerminalNode __result, Terminal __instance)
    {
        if (__result is null) return null;

        Logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes: {__result?.name}");
        Logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes.2");

        foreach (var command in TerminalManager.commands)
        {
            Logger.LogDebug($"command: {command.node.name}");

            if (command.IsSimpleCommand)
            {
                Logger.LogDebug(" > executing SimpleCommand");
                Logger.LogDebug($"  > Checking {command.node.name} | e: /{__result.terminalEvent}/");
                if (command.node.name != __result.name) continue;

                return ExecuteSimpleCommand(command, __result, __instance);
            }

            Logger.LogDebug(" > executing ComplexCommand");
            // if (command.node.name != __result.name) continue;
            var resNode = ExecuteMultiCommand(command, __result, __instance);
            if (resNode != null) return resNode;
        }

        Logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes.end");
        return __result;
    }

    private static TerminalNode ExecuteSimpleCommand(TerminalCommandBuilder command, TerminalNode __result, Terminal __instance)
    {
        Logger.LogDebug("   > checking conditions");
        foreach (var (node, condition) in command.specialNodes)
        {
            Logger.LogDebug($"    > condition: {node.name}");

            if (!condition()) continue;

            Logger.LogDebug($"     > FAILED");
            return node;
        }

        return __result;
        // return command.node;
    }

    private static TerminalNode ExecuteMultiCommand(TerminalCommandBuilder command, TerminalNode __result, Terminal __instance)
    {
        foreach (var keyword in command.nodeSubKeywords)
        {
            Logger.LogDebug($" > kw: {keyword.Id}");

            if (keyword.IsVariableCommand)
            {
                var terminalInput = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                Logger.LogDebug($"  > INPUT: |{terminalInput}|");
                // Logger.LogDebug("~~~");
                // Logger.LogDebug($"  > IsVariableCommand: {__instance.screenText.text} | {__instance.textAdded}");
                // Logger.LogDebug("~~~");
                var regex = new Regex(keyword.VariableRegexMatchPattern);
                Logger.LogDebug($"REGEX: {regex}");
                var match = regex.Match(terminalInput.ToLower());
                if (!match.Success) continue;
                Logger.LogDebug(" > MATCH! VariablePreAction()");
                keyword.VariablePreAction(match.Groups[1].Value);
            }
            else
            {
                Logger.LogDebug(" > ELSE");
                if (__result.name != keyword.Id) continue;
                Logger.LogDebug("  > preAction()");
                keyword.PreConditionAction();
            }

            // command.specialNodes.FirstOrDefault(x => x.condition(__result.name));
            Logger.LogDebug(" > checking special conditions");
            foreach (var cc in keyword.Conditions)
            {
                Logger.LogDebug($"  > {cc}");
                var specialCondition = command.specialNodes.FirstOrDefault(x => x.node.name == cc);
                if (specialCondition == default)
                {
                    Logger.LogError($"Keyword {keyword.Node.name} has special criteria for '{cc}' but it is not found as part of the commands' special conditions list.");
                    break;
                }

                Logger.LogDebug($"  > {cc} = {specialCondition.condition()}");

                if (specialCondition.condition()) continue;

                Logger.LogDebug($" > FAILED: {specialCondition.node.name}");
                return specialCondition.node;
            }

            Logger.LogDebug($"keyword.IsVariableCommand => {keyword.IsVariableCommand}");
            if (keyword.IsVariableCommand)
            {
                return keyword.Node;
            }

            // break;
        }

        return null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.RunTerminalEvents))]
    public static void RunTerminalEvents(TerminalNode node)
    {
        Logger.LogDebug($"TerminalPatch.RunTerminalEvents: {node} | {node.terminalEvent}");

        if (node.terminalEvent.IsNullOrWhiteSpace()) return;

        foreach (var command in TerminalManager.commands)
        {
            Logger.LogDebug($"  > Checking {command.node.name}");

            if (command.IsSimpleCommand && command.ActionEvent == node.terminalEvent)
            {
                Logger.LogDebug("  > IsSimpleCommand, SKIP??");
                Logger.LogDebug($" > EXEC simple command: {command.node.name}");
                var res = command.Action();
                node.displayText = res + TerminalManager.EndOfMessage;

                break;
            }

            var match = command.nodeSubKeywords.FirstOrDefault(x => x.ActionEvent == node.terminalEvent);
            if (match == default) continue;

            Logger.LogDebug($"  > Found {match.Node.name}");
            match.Action();

            break;
        }

        // if (node.terminalEvent == "giveLootHack")
        // {
        //     for (int i = 0; i < 50; i++)
        //     {
        //         Random rand = new Random();
        //         int nextScrap = rand.Next(16, 68);
        //         GameObject scrap = Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[nextScrap].spawnPrefab, GameNetworkManager.Instance.localPlayerController.transform.position, Quaternion.identity);
        //         scrap.GetComponent<GrabbableObject>().fallTime = 0f;
        //         int scrapValue = rand.Next(20, 120);
        //         scrap.AddComponent<ScanNodeProperties>().scrapValue = scrapValue;
        //         scrap.GetComponent<GrabbableObject>().scrapValue = scrapValue;
        //         scrap.GetComponent<NetworkObject>().Spawn();
        //         RoundManager.Instance.scrapCollectedThisRound.Add(scrap.GetComponent<GrabbableObject>());
        //     }
        // }
    }
}
