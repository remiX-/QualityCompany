using BepInEx;
using HarmonyLib;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Manager.ShipTerminal;

/// <summary>
/// A super duper advanced Terminal API ;-)
/// </summary>
public class AdvancedTerminal
{
    internal static readonly Dictionary<string, Func<string>> GlobalTextReplacements = new();
    internal static readonly List<TerminalCommandBuilder> Commands = new();

    private static readonly ACLogger Logger = new(nameof(AdvancedTerminal));

    private static TerminalKeyword _terminalConfirmKeyword;
    private static TerminalKeyword _terminalDenyKeyword;
    private static TerminalNode _otherCategoryTerminalNode;
    private static TerminalNode _helpTerminalNode;

    internal static string EndOfMessage => "\n\n\n";

    /// <summary>
    /// Add global text replacements. Try not to use this and text replacements should be on a mod-specific level.
    /// </summary>
    /// <param name="text">Text to replace</param>
    /// <param name="func">Replacement function that returns a string</param>
    public static void AddGlobalTextReplacement(string text, Func<string> func)
    {
        GlobalTextReplacements.Add(text, func);
    }

    internal static void ApplyToTerminal()
    {
        GameUtils.Terminal.terminalNodes.allKeywords = GameUtils.Terminal.terminalNodes.allKeywords.AddToArray(new TerminalKeyword
        {
            name = "QualityCompany",
            word = "qc",
            specialKeywordResult = new TerminalNode
            {
                clearPreviousText = true,
                displayText = "> QUALITY COMPANY\n\n\t:)"
            }
        });
        _terminalConfirmKeyword = GameUtils.Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Confirm");
        _terminalDenyKeyword = GameUtils.Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Deny");
        _otherCategoryTerminalNode = GameUtils.Terminal.terminalNodes.allKeywords.First(node => node.name == "Other").specialKeywordResult;
        _helpTerminalNode = GameUtils.Terminal.terminalNodes.allKeywords.First(node => node.name == "Help").specialKeywordResult;

        foreach (var cmd in AdvancedTerminalRegistry.Commands)
        {
            var res = cmd.Run.Invoke(null, null);
            if (res is not TerminalCommandBuilder builder) continue;

            Commands.Add(builder);
        }

        foreach (var cmdBuilder in Commands)
        {
            var keywords = cmdBuilder.Build(_terminalConfirmKeyword, _terminalDenyKeyword);
            GameUtils.Terminal.terminalNodes.allKeywords = GameUtils.Terminal.terminalNodes.allKeywords.AddRangeToArray(keywords);

            if (cmdBuilder.Description.IsNullOrWhiteSpace()) continue;

            _helpTerminalNode.displayText = _helpTerminalNode.displayText[..^1] + $"{cmdBuilder.Description}";
            _otherCategoryTerminalNode.displayText = _otherCategoryTerminalNode.displayText.Substring(0, _otherCategoryTerminalNode.displayText.Length - 1) + $"{cmdBuilder.Description}";
        }

        Disconnected += OnDisconnected;

        // foreach (var kw in GameUtils.Terminal.terminalNodes.allKeywords)
        // {
        //     Logger.LogDebug($"{kw.name} | {kw.word}");
        // }
    }

    private static void OnDisconnected(GameNetworkManager instance)
    {
        GlobalTextReplacements.Clear();
    }
}