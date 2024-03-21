using BepInEx;
using HarmonyLib;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using QualityCompany.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static QualityCompany.Events.GameEvents;
using static Unity.Audio.Handle;

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    private static readonly ModLogger Logger = new(nameof(TerminalPatch));

    private static string? _currentInput;

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void AwakePatch(Terminal __instance)
    {
        GameUtils.Terminal = __instance;

        AdvancedTerminal.Init();

        OnTerminalAwakeEvent(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SyncGroupCreditsClientRpc")]
    private static void SyncGroupCreditsClientRpcPatch()
    {
        LootMonitor.UpdateMonitor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ParsePlayerSentence")]
    public static TerminalNode ParsePlayerSentencePatch(TerminalNode __result, Terminal __instance)
    {
        Logger.LogDebugMode("ParsePlayerSentencePatch.start");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (__result == null) return null!;
        Logger.LogDebugMode($" > {__result} | {__result.name} | {__result.terminalEvent}");

        // If the name does not start with 'qc:' prefix as defined by QualityCompany nodes, then skip
        if (__result.terminalEvent is not null &&
            __result.terminalEvent.StartsWith("qc:") &&
            __result.terminalEvent.EndsWith("_help"))
        {
            return __result;
        }

        var terminalInput = __instance.screenText.text[^__instance.textAdded..].ToLower().Trim();
        var inputCommand = terminalInput.Contains(" ") ? terminalInput[..terminalInput.LastIndexOf(' ')] : terminalInput;
        _currentInput = terminalInput.Contains(" ") ? terminalInput[(terminalInput.LastIndexOf(' ') + 1)..] : null;
        Logger.LogDebugMode($" > input: {terminalInput} | {inputCommand} | {_currentInput}");

        // Try to find the matching primary command first
        var filteredCommands = AdvancedTerminal.Commands
            .Where(x => inputCommand.Length < 3 ? x.CommandText == inputCommand : x.CommandText.StartsWith(inputCommand))
            .ToList();

        if (!filteredCommands.Any())
        {
            Logger.LogDebugMode($" > No commands found matching input '{terminalInput}' | '{inputCommand}' with args '{_currentInput}'");
            return __result;
        }

        if (filteredCommands.Count > 1)
        {
            Logger.LogError($" > Found multiple commands! HOW? Only using first one. Found: {filteredCommands.Select(x => x.CommandText).Aggregate((first, second) => $"{first}, {second}")}");
        }

        var cmd = filteredCommands.First();
        Logger.LogDebugMode($" > command: {cmd.CommandText} | event: /{__result.terminalEvent ?? "empty"}/");

        return cmd.IsSimpleCommand
            ? ExecuteSimpleCommand(cmd)
            : ExecuteComplexCommand(cmd, __result, inputCommand, _currentInput!);
    }

    private static TerminalNode ExecuteSimpleCommand(TerminalCommandBuilder command)
    {
        Logger.LogDebugMode(" > executing SimpleCommand");

        // _cleanAfterNext = true;

        Logger.LogDebugMode("  > checking conditions");
        foreach (var conditionNode in command.ConditionNodes)
        {
            Logger.LogDebugMode($"   > condition: {conditionNode.Node.name}");

            if (!conditionNode.Condition()) continue;

            Logger.LogDebugMode("    > FAILED");
            return conditionNode.Node;
        }

        Logger.LogDebugMode($"  > returning {command.Node.name}");
        return command.Node;
    }

    private static TerminalNode ExecuteComplexCommand(TerminalCommandBuilder command, TerminalNode __result, string inputCommand, string inputCommandArgs)
    {
        Logger.LogDebugMode(" > executing ComplexCommand");

        // Try to find a matching non-variable (input) command
        var subCmd = command.SubCommands.FirstOrDefault(subCmd => !subCmd.IsVariableCommand && subCmd.Name == inputCommandArgs);
        if (subCmd is not null)
        {
            // Found matching 'simple' sub command
            var r = subCmd.PreConditionAction?.Invoke();
            if (r is not null)
            {
                __result.displayText = r;
                return __result;
            }
        }
        else
        {
            // If not input args are provided, return primary command main node
            if (inputCommandArgs.IsNullOrWhiteSpace()) return command.Node;

            // Now try to find a matching variable input command
            subCmd = command.SubCommands.FirstOrDefault(subCmd =>
            {
                if (!subCmd.IsVariableCommand) return false;

                Logger.LogDebugMode($"  > INPUT: '{inputCommand}' | '{inputCommandArgs}' | REGEX: {subCmd.VariableRegexMatchPattern}");
                var regex = new Regex(subCmd.VariableRegexMatchPattern);
                var match = regex.Match(inputCommandArgs);
                return match.Success;
            });
            var r = subCmd?.VariablePreAction?.Invoke(inputCommandArgs);
            if (r is not null)
            {
                __result.displayText = r;
                return __result;
            }
        }

        // If nothing was found, just return the main commands node
        if (subCmd is null)
        {
            Logger.LogDebugMode($"  > No matching sub command found for input: '{inputCommand}' | '{inputCommandArgs}'");
            return command.Node;
        }

        Logger.LogDebugMode("  > checking special conditions");
        foreach (var conditionString in subCmd.Conditions)
        {
            var conditionNode = command.ConditionNodes.FirstOrDefault(x => x.Node.name.EndsWith(conditionString));
            if (conditionNode is null)
            {
                Logger.LogError($"> SubCommand {subCmd.Name} has special criteria for '{conditionString}' but it is not found as part of the commands' special conditions list.");
                break;
            }

            Logger.LogDebugMode($"   > {conditionString} = {conditionNode.Condition()}");

            if (conditionNode.Condition()) continue;

            Logger.LogDebugMode($"   > FAILED: {conditionNode.Node.name}");
            // _cleanAfterNext = true;
            return conditionNode.Node;
        }

        // Finally, if all is well & conditions are happy, return the subCommands node.
        return subCmd.Node;
    }

    // private static TerminalNode UpdateDisplayText(TerminalNode node, Dictionary<string, Func<string>> replacements)
    // {
    //     return node;
    //     var returnNode = node;
    //     var text = node.displayText;
    //     foreach (var (key, func) in AdvancedTerminal.GlobalTextReplacements)
    //     {
    //         if (!text.Contains(key)) continue;
    //
    //         Logger.LogDebugMode($" > found global: {key}");
    //         text = text.Replace(key, func());
    //     }
    //
    //     foreach (var (key, func) in replacements)
    //     {
    //         if (!text.Contains(key)) continue;
    //
    //         Logger.LogDebugMode($" > found replacement: {key}");
    //         text = text.Replace(key, func());
    //     }
    //
    //     returnNode.displayText = text;
    //     return returnNode;
    // }

    [HarmonyPostfix]
    [HarmonyPatch("TextPostProcess")]
    public static string TextPostProcessPatch(string mod, TerminalNode node, string __result)
    {
        Logger.LogDebugMode($"TextPostProcessPatch.start: {node.name} | {node.terminalEvent}");
        if (node.terminalEvent is null) return __result;
        if (!node.terminalEvent.StartsWith("qc:")) return __result;

        foreach (var (key, func) in AdvancedTerminal.GlobalTextReplacements)
        {
            if (!__result.Contains(key)) continue;

            Logger.LogDebugMode($" > found global: {key}");
            __result = __result.Replace(key, func());
        }

        var foundCommand = TryGetCommandByEvent(node.terminalEvent, out var command, out var subCommand);
        if (!foundCommand) return __result;

        foreach (var (key, func) in command.TextProcessPlaceholders)
        {
            if (!__result.Contains(key)) continue;

            Logger.LogDebugMode($" > found replacement: {command.CommandText}.{key}");
            __result = __result.Replace(key, func());
        }

        Logger.LogDebugMode("TextPostProcessPatch.end\n");

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.RunTerminalEvents))]
    public static void RunTerminalEventsPatch(TerminalNode node)
    {
        Logger.LogDebugMode($"RunTerminalEvents: | " +
                            $"node.name: {node.name ?? "null"} | " +
                            $"node.terminalEvent: {node.terminalEvent ?? "empty"} | " +
                            $"node.isConfirm: {node.isConfirmationNode}");

        if (ShouldSkipNode(node)) return;

        Logger.LogDebugMode($" > Checking {node.name}");

        var foundCommand = TryGetCommandByEvent(node.terminalEvent!, out var command, out var subCommand);
        if (!foundCommand)
        {
            Logger.LogError($"Passed terminal conditions but failed to find an internal command for {node.name} | {node.terminalEvent}");
            return;
        }

        Logger.LogDebugMode($"  > Checking {command.Node.name}");

        if (command.IsSimpleCommand)
        {
            Logger.LogDebugMode("  > IsSimpleCommand, SKIP??");
            Logger.LogDebugMode($" > EXEC simple command: {command.Node.name}");
            var res = command.Action();
            node.displayText = res + AdvancedTerminal.EndOfMessage;

            return;
        }

        if (subCommand is null)
        {
            Logger.LogError($"Have a complex command but failed to find sub command for {command.Node.name}");
            return;
        }

        Logger.LogDebugMode($"  > Found {subCommand.Node.name}");
        Logger.LogDebugMode($"   > ActionInputResult {subCommand.ActionInputResult is null}");
        Logger.LogDebugMode($"   > ActionResult {subCommand.ActionResult is null}");

        // if (subCommand.ActionWithInput is not null)
        // {
        //     subCommand.ActionWithInput(_currentInput!);
        // }
         if (subCommand.ActionInputResult is not null)
        {
            var res = subCommand.ActionInputResult(_currentInput!);
            if (res is not null) node.displayText = res + AdvancedTerminal.EndOfMessage;
        }
        // else if (subCommand.Action is not null)
        // {
        //     subCommand.Action();
        // }
        else if (subCommand.ActionResult is not null)
        {
            var res = subCommand.ActionResult();
            if (res is not null) node.displayText = res + AdvancedTerminal.EndOfMessage;
        }
    }

    private static bool ShouldSkipNode(TerminalNode node)
    {
        if (node.terminalEvent is null) return true;
        if (!node.terminalEvent.StartsWith("qc:")) return true;
        if (node.isConfirmationNode) return true;

        // Skip condition nodes
        if (node.terminalEvent.EndsWith("_condition")) return true;

        // Skip help nodes
        if (node.terminalEvent.EndsWith("_help")) return true;

        // Skip deny_event nodes
        if (node.terminalEvent.EndsWith("_deny_event")) return true;

        return false;
    }

    private static bool TryGetCommandByEvent(string terminalEvent, out TerminalCommandBuilder command, out TerminalSubCommand? subCommand)
    {
        command = null;
        subCommand = null;

        foreach (var cmd in AdvancedTerminal.Commands)
        {
            if (cmd.IsSimpleCommand && cmd.ActionEvent == terminalEvent)
            {
                command = cmd;
                return true;
            }

            var match = cmd.SubCommands.FirstOrDefault(x => x.ActionEvent == terminalEvent);
            if (match is not null)
            {
                command = cmd;
                subCommand = match;
                return true;
            }

            // search via condition nodes
            var m = cmd.ConditionNodes.FirstOrDefault(x => x.Node.terminalEvent == terminalEvent);
            if (m is not null)
            {
                command = cmd;
                return true;
            }
        }

        return false;
    }
}
