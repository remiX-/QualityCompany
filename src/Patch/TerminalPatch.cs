using AdvancedCompany.Manager.ShipTerminal;
using AdvancedCompany.Service;
using AdvancedCompany.TerminalCommands;
using AdvancedCompany.Utils;
using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Text.RegularExpressions;

#pragma warning disable IDE0060
#pragma warning disable Harmony003

namespace AdvancedCompany.Patch;

[HarmonyPatch(typeof(Terminal))]
public class TerminalPatch
{
    private static readonly ACLogger _logger = new(nameof(TerminalPatch));

    private static bool patchedTerminal;

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void Awake(Terminal __instance)
    {
        _logger.LogDebug("TerminalPatch.Awake");
        if (patchedTerminal)
        {
            return;
        }

        AdvancedTerminal.AddGlobalTextReplacement("[companyBuyingRateWarning]", () => Math.Abs(StartOfRound.Instance.companyBuyingRate - 1f) == 0f
            ? ""
            : $"WARNING: Company buying rate is currently at {StartOfRound.Instance.companyBuyingRate:P0}\n\n");
        AdvancedTerminal.AddGlobalTextReplacement("[confirmOrDeny]", () => "Please CONFIRM or DENY.\n\n\n");

        AdvancedTerminal.ApplyToTerminal(__instance);

        patchedTerminal = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("TextPostProcess")]
    public static string ProcessCustomText(string __result)
    {
        _logger.LogDebug($"TerminalPatch.ProcessCustomText: {__result}");
        foreach (var (key, func) in AdvancedTerminal.GlobalTextReplacements)
        {
            if (!__result.Contains(key)) continue;

            __result = __result.Replace(key, func());
        }

        foreach (var command in AdvancedTerminal.Commands)
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

        _logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes: {__result?.name}");
        _logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes.2");

        foreach (var command in AdvancedTerminal.Commands)
        {
            _logger.LogDebug($"command: {command.node.name}");

            if (command.IsSimpleCommand)
            {
                _logger.LogDebug(" > executing SimpleCommand");
                _logger.LogDebug($"  > Checking {command.node.name} | e: /{__result.terminalEvent}/");
                if (command.node.name != __result.name) continue;

                return ExecuteSimpleCommand(command, __result, __instance);
            }

            _logger.LogDebug(" > executing ComplexCommand");
            // if (command.node.name != __result.name) continue;
            var resNode = ExecuteMultiCommand(command, __result, __instance);
            if (resNode != null) return resNode;
        }

        _logger.LogDebug($"TerminalPatch.TryReturnSpecialNodes.end");
        return __result;
    }

    private static TerminalNode ExecuteSimpleCommand(TerminalCommandBuilder command, TerminalNode __result, Terminal __instance)
    {
        _logger.LogDebug("   > checking conditions");
        foreach (var (node, condition) in command.specialNodes)
        {
            _logger.LogDebug($"    > condition: {node.name}");

            if (!condition()) continue;

            _logger.LogDebug($"     > FAILED");
            return node;
        }

        return __result;
    }

    private static TerminalNode ExecuteMultiCommand(TerminalCommandBuilder command, TerminalNode __result, Terminal __instance)
    {
        foreach (var keyword in command.SubCommands)
        {
            _logger.LogDebug($" > kw: {keyword.Id}");

            if (keyword.IsVariableCommand)
            {
                var terminalInput = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                _logger.LogDebug($"  > INPUT: |{terminalInput}|");
                var regex = new Regex(keyword.VariableRegexMatchPattern);
                _logger.LogDebug($"REGEX: {regex}");
                var match = regex.Match(terminalInput.ToLower());
                if (!match.Success) continue;
                _logger.LogDebug(" > MATCH! VariablePreAction()");
                keyword.VariablePreAction(match.Groups[1].Value);
            }
            else
            {
                _logger.LogDebug(" > ELSE");
                if (__result.name != keyword.Id) continue;
                _logger.LogDebug("  > preAction()");
                keyword.PreConditionAction();
            }

            _logger.LogDebug(" > checking special conditions");
            foreach (var cc in keyword.Conditions)
            {
                _logger.LogDebug($"  > {cc}");
                var specialCondition = command.specialNodes.FirstOrDefault(x => x.node.name == cc);
                if (specialCondition == default)
                {
                    _logger.LogError($"Keyword {keyword.Node.name} has special criteria for '{cc}' but it is not found as part of the commands' special conditions list.");
                    break;
                }

                _logger.LogDebug($"  > {cc} = {specialCondition.condition()}");

                if (specialCondition.condition()) continue;

                _logger.LogDebug($" > FAILED: {specialCondition.node.name}");
                return specialCondition.node;
            }

            _logger.LogDebug($"keyword.IsVariableCommand => {keyword.IsVariableCommand}");
            if (keyword.IsVariableCommand)
            {
                return keyword.Node;
            }
        }

        return null;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.RunTerminalEvents))]
    public static void RunTerminalEvents(TerminalNode node)
    {
        _logger.LogDebug($"TerminalPatch.RunTerminalEvents: {node} | {node.terminalEvent}");

        if (node.terminalEvent.IsNullOrWhiteSpace()) return;

        foreach (var command in AdvancedTerminal.Commands)
        {
            _logger.LogDebug($"  > Checking {command.node.name}");

            if (command.IsSimpleCommand && command.ActionEvent == node.terminalEvent)
            {
                _logger.LogDebug("  > IsSimpleCommand, SKIP??");
                _logger.LogDebug($" > EXEC simple command: {command.node.name}");
                var res = command.Action();
                node.displayText = res + AdvancedTerminal.EndOfMessage;

                break;
            }

            var match = command.SubCommands.FirstOrDefault(x => x.ActionEvent == node.terminalEvent);
            if (match == default) continue;

            _logger.LogDebug($"  > Found {match.Node.name}");
            match.Action();

            break;
        }
    }
}
