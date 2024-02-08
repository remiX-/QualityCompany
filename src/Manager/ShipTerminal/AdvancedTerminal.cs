using BepInEx;
using HarmonyLib;
using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QualityCompany.Manager.ShipTerminal;

public class AdvancedTerminal
{
    internal static readonly Dictionary<string, Func<string>> GlobalTextReplacements = new();
    internal static readonly List<TerminalCommandBuilder> Commands = new();

    private static readonly ACLogger Logger = new(nameof(AdvancedTerminal));
    public static Terminal Terminal;

    private static TerminalKeyword terminalConfirmKeyword;
    private static TerminalKeyword terminalDenyKeyword;
    private static TerminalNode otherCategoryTerminalNode;
    private static TerminalNode helpTerminalNode;

    public static string EndOfMessage => "\n\n\n";

    public static void AddGlobalTextReplacement(string text, Func<string> func)
    {
        GlobalTextReplacements.Add(text, func);
    }

    internal static void ApplyToTerminal(Terminal terminal)
    {
        Terminal = terminal;

        Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddToArray(new TerminalKeyword
        {
            name = "QualityCompany",
            word = "qc",
            specialKeywordResult = new TerminalNode
            {
                clearPreviousText = true,
                displayText = "> QUALITY COMPANY\n\n\t:)"
            }
        });
        terminalConfirmKeyword = Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Confirm");
        terminalDenyKeyword = Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Deny");
        otherCategoryTerminalNode = Terminal.terminalNodes.allKeywords.First(node => node.name == "Other").specialKeywordResult;
        helpTerminalNode = Terminal.terminalNodes.allKeywords.First(node => node.name == "Help").specialKeywordResult;

        foreach (var cmd in AdvancedTerminalRegistry.Commands)
        {
            var res = cmd.Run.Invoke(null, null);
            if (res is null) continue;

            Commands.Add((TerminalCommandBuilder)res);
        }

        foreach (var cmdBuilder in Commands)
        {
            var keywords = cmdBuilder.Build(terminalConfirmKeyword, terminalDenyKeyword);
            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddRangeToArray(keywords);

            if (cmdBuilder.description.IsNullOrWhiteSpace()) continue;

            helpTerminalNode.displayText = helpTerminalNode.displayText[..^1] + $"{cmdBuilder.description}";
            otherCategoryTerminalNode.displayText = otherCategoryTerminalNode.displayText.Substring(0, otherCategoryTerminalNode.displayText.Length - 1) + $"{cmdBuilder.description}";
        }

        foreach (var kw in Terminal.terminalNodes.allKeywords)
        {
            Logger.LogDebug($"{kw.name} | {kw.word}");
        }
    }
}