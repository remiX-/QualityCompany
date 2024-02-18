using BepInEx;
using HarmonyLib;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static QualityCompany.Service.GameEvents;

namespace QualityCompany.Manager.ShipTerminal;

internal class AdvancedTerminal
{
    internal static readonly Dictionary<string, Func<string>> GlobalTextReplacements = new();
    internal static readonly List<TerminalCommandBuilder> Commands = new();

    private static readonly ACLogger Logger = new(nameof(AdvancedTerminal));

    private static TerminalKeyword _terminalConfirmKeyword;
    private static TerminalKeyword _terminalDenyKeyword;
    private static TerminalNode _helpTerminalNode;

    internal static string EndOfMessage => "\n\n\n";

    internal static void AddGlobalTextReplacement(string text, Func<string> func)
    {
        GlobalTextReplacements.TryAdd(text, func);
    }

    internal static void Init()
    {
        TerminalAwakeEvent += ApplyToTerminal;

        Disconnected += _ =>
        {
            GlobalTextReplacements.Clear();
            Commands.Clear();
        };
    }

    internal static void ApplyToTerminal(Terminal terminal)
    {
        AddGlobalTextReplacement("[companyBuyingRateWarning]", () => Math.Abs(GameUtils.StartOfRound.companyBuyingRate - 1f) == 0f
            ? ""
            : $"WARNING: Company buying rate is currently at {GameUtils.StartOfRound.companyBuyingRate:P0}\n\n");
        AddGlobalTextReplacement("[confirmOrDeny]", () => "Please CONFIRM or DENY.\n\n\n");

        _terminalConfirmKeyword = terminal.terminalNodes.allKeywords.First(kw => kw.name == "Confirm");
        _terminalDenyKeyword = terminal.terminalNodes.allKeywords.First(kw => kw.name == "Deny");
        _helpTerminalNode = terminal.terminalNodes.allKeywords.First(node => node.name == "Help").specialKeywordResult;

        foreach (var (modName, config) in AdvancedTerminalRegistry.Commands)
        {
            var commandTexts = LoadCommands(terminal, modName, config);
            CreateModPrimaryCommand(terminal, config, commandTexts);
            AddToHelp(config);
        }

        foreach (var kw in terminal.terminalNodes.allKeywords)
        {
            Logger.LogDebug($"{kw.name} | {kw.word}");
        }
    }

    private static string LoadCommands(Terminal terminal, string modName, ModConfiguration config)
    {
        Logger.LogDebug($"Loading commands for: {modName}");

        var primaryCommandText = "";

        foreach (var command in config.Commands)
        {
            var res = command.Run.Invoke(null, null);
            if (res is not TerminalCommandBuilder builder) continue;

            Commands.Add(builder);

            var keywords = builder.Build(_terminalConfirmKeyword, _terminalDenyKeyword);

            // if it already has the first keyword, it has already applied it, so just skip
            if (terminal.terminalNodes.allKeywords.Any(kw => keywords.Any(kw2 => kw.name == kw2.name))) continue;

            terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddRangeToArray(keywords);

            if (builder.Description.IsNullOrWhiteSpace()) continue;

            primaryCommandText += $"{builder.Description}\n\n";
        }

        return primaryCommandText;
    }

    private static void CreateModPrimaryCommand(Terminal terminal, ModConfiguration modConfig, string commandTexts)
    {
        if (!modConfig.CreatePrimaryCommand) return;

        var builder = new StringBuilder();
        builder.AppendLine(modConfig.PrimaryCommandName);
        if (modConfig.Description is not null)
        {
            builder.AppendLine(modConfig.Description);
            builder.AppendLine();
        }
        builder.AppendLine(commandTexts);

        terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddToArray(new TerminalKeyword
        {
            name = modConfig.PrimaryCommandName,
            word = modConfig.PrimaryCommandKeyword,
            specialKeywordResult = new TerminalNode
            {
                clearPreviousText = true,
                displayText = builder.ToString()
            }
        });
    }

    private static void AddToHelp(ModConfiguration modConfig)
    {
        if (!modConfig.AddToHelp || !modConfig.CreatePrimaryCommand) return;

        _helpTerminalNode.displayText = _helpTerminalNode.displayText[..^1] + $"> {modConfig.PrimaryCommandKeyword}\n{modConfig.Description}\n\n";
    }
}