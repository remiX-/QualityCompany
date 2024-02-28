using BepInEx;
using HarmonyLib;
using QualityCompany.Manager.ShipTerminal;
using QualityCompany.Modules.Ship;
using QualityCompany.Service;
using QualityCompany.Utils;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using static QualityCompany.Service.GameEvents;

#pragma warning disable IDE0060
#pragma warning disable Harmony003

namespace QualityCompany.Patch;

[HarmonyPatch(typeof(Terminal))]
internal class TerminalPatch
{
    private static readonly ModLogger Logger = new(nameof(TerminalPatch));

    private static TerminalCommandBuilder? _commandExecuting;
    private static TerminalSubCommand? _subCommandExecuting;
    private static bool cleanAfterNext;

    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void AwakePatch(Terminal __instance)
    {
        GameUtils.Terminal = __instance;

        // TODO: this should be auto via attribute maybe?
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
        // if (!__result.name.StartsWith("qc:")) return __result;
        // if name ends with '_help' as defined by QualityCompany help nodes, then skip
        if (__result.terminalEvent is not null && __result.terminalEvent.StartsWith("qc:") && __result.terminalEvent.EndsWith("_help")) return __result;
        if (__result.name.EndsWith("_confirm") || __result.name.EndsWith("_deny"))
        {
            cleanAfterNext = true;
        }

        var terminalInput = __instance.screenText.text[^__instance.textAdded..].ToLower().Trim();
        var inputCommand = terminalInput.Contains(" ") ? terminalInput[..terminalInput.LastIndexOf(' ')] : terminalInput;
        var inputCommandArgs = terminalInput.Contains(" ") ? terminalInput[(terminalInput.LastIndexOf(' ') + 1)..] : null;
        Logger.LogDebugMode($" > input: {terminalInput} | {inputCommand} | {inputCommandArgs}");

        // Try to find the matching primary command first
        var filteredCommands = AdvancedTerminal.Commands
            .Where(x => inputCommand.Length < 3 ? x.CommandText == inputCommand : x.CommandText.StartsWith(inputCommand))
            .ToList();

        if (!filteredCommands.Any())
        {
            Logger.LogDebugMode($" > No commands found matching input '{terminalInput}' | '{inputCommand}' with args '{inputCommandArgs}'");
            cleanAfterNext = true;
            return __result;
        }

        if (filteredCommands.Count > 1)
        {
            Logger.LogError($" > Found multiple commands! HOW? Only using first one. Found: {filteredCommands.Select(x => x.CommandText).Aggregate((first, second) => $"{first}, {second}")}");
        }

        _commandExecuting = filteredCommands.First();
        Logger.LogDebugMode($" > command: {_commandExecuting.CommandText} | event: /{__result.terminalEvent ?? "empty"}/");

        if (_commandExecuting.IsSimpleCommand)
        {
            return ExecuteSimpleCommand(_commandExecuting);
        }

        return ExecuteComplexCommand(_commandExecuting, __result, inputCommand, inputCommandArgs);
    }

    private static TerminalNode ExecuteSimpleCommand(TerminalCommandBuilder command)
    {
        Logger.LogDebugMode(" > executing SimpleCommand");

        cleanAfterNext = true;

        Logger.LogDebugMode("  > checking conditions");
        foreach (var (node, condition) in command.SpecialNodes)
        {
            Logger.LogDebugMode($"   > condition: {node.name}");

            if (!condition()) continue;

            Logger.LogDebugMode($"    > FAILED");
            return node;
        }

        Logger.LogDebugMode($"  > returning {command.Node.name}");
        return command.Node;
    }

    private static TerminalNode ExecuteComplexCommand(TerminalCommandBuilder command, TerminalNode __result, string inputCommand, string inputCommandArgs)
    {
        Logger.LogDebugMode(" > executing ComplexCommand");

        // Try to find a matching non-variable (input) command
        _subCommandExecuting = command.SubCommands.FirstOrDefault(subCmd => !subCmd.IsVariableCommand && subCmd.Name == inputCommandArgs);
        if (_subCommandExecuting is not null)
        {
            // Found matching 'simple' sub command
            _subCommandExecuting.PreConditionAction();
        }
        else
        {
            // If not input args are provided, return primary command main node
            if (inputCommandArgs.IsNullOrWhiteSpace()) return command.Node;

            // Now try to find a matching variable input command
            _subCommandExecuting = command.SubCommands.FirstOrDefault(subCmd =>
            {
                if (!subCmd.IsVariableCommand) return false;

                Logger.LogDebugMode($"  > INPUT: '{inputCommand}' | '{inputCommandArgs}' | REGEX: {subCmd.VariableRegexMatchPattern}");
                var regex = new Regex(subCmd.VariableRegexMatchPattern);
                var match = regex.Match(inputCommandArgs);
                return match.Success;
            });
            _subCommandExecuting?.VariablePreAction(inputCommandArgs);
        }

        // If nothing was found, just return the main commands node
        if (_subCommandExecuting is null)
        {
            Logger.LogDebugMode($"  > No matching sub command found for input: '{inputCommand}' | '{inputCommandArgs}'");
            return command.Node;
        }

        Logger.LogDebugMode("  > checking special conditions");
        foreach (var conditionString in _subCommandExecuting.Conditions)
        {
            var specialCondition = command.SpecialNodes.FirstOrDefault(x => x.node.name.EndsWith(conditionString));
            if (specialCondition == default)
            {
                Logger.LogError($"> SubCommand {_subCommandExecuting.Name} has special criteria for '{conditionString}' but it is not found as part of the commands' special conditions list.");
                break;
            }

            Logger.LogDebugMode($"   > {conditionString} = {specialCondition.condition()}");

            if (specialCondition.condition()) continue;

            Logger.LogDebugMode($"   > FAILED: {specialCondition.node.name}");
            cleanAfterNext = true;
            return specialCondition.node;
        }

        // Finally, if all is well & conditions are happy, return the subCommands node.
        return _subCommandExecuting.Node;
    }

    [HarmonyPostfix]
    [HarmonyPatch("TextPostProcess")]
    public static string TextPostProcessPatch(string __result)
    {
        Logger.LogDebugMode("TextPostProcessPatch.start");
        if (_commandExecuting is null && _subCommandExecuting is null) return __result;

        foreach (var (key, func) in AdvancedTerminal.GlobalTextReplacements)
        {
            if (!__result.Contains(key)) continue;

            Logger.LogDebugMode($" > found global: {key}");
            __result = __result.Replace(key, func());
        }

        if (_commandExecuting is not null)
        {
            foreach (var (key, func) in _commandExecuting.TextProcessPlaceholders)
            {
                if (!__result.Contains(key)) continue;

                Logger.LogDebugMode($" > found command: {_commandExecuting.CommandText}.{key}");
                __result = __result.Replace(key, func());
            }
        }

        if (cleanAfterNext)
        {
            _commandExecuting = null;
            _subCommandExecuting = null;
            cleanAfterNext = false;
        }

        Logger.LogDebugMode("TextPostProcessPatch.end\n");

        return __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.RunTerminalEvents))]
    public static void RunTerminalEventsPatch(TerminalNode node)
    {
        Logger.LogDebugMode($"RunTerminalEvents: | " +
                            $"exec: {_commandExecuting?.CommandText ?? "null"}, {_subCommandExecuting?.Id ?? "null"} | " +
                            $"node.name: {node.name ?? "null"} | " +
                            $"node.terminalEvent: {node.terminalEvent ?? "empty"}");

        if (_commandExecuting is null) return;
        if (node.terminalEvent.IsNullOrWhiteSpace()) return;

        Logger.LogDebugMode($" > Checking {_commandExecuting.Node.name} | {_commandExecuting.ActionEvent}");

        if (_commandExecuting.IsSimpleCommand && _commandExecuting.ActionEvent == node.terminalEvent)
        {
            Logger.LogDebugMode("  > IsSimpleCommand, SKIP??");
            Logger.LogDebugMode($" > EXEC simple command: {_commandExecuting.Node.name}");
            var res = _commandExecuting.Action();
            node.displayText = res + AdvancedTerminal.EndOfMessage;

            return;
        }

        Logger.LogDebugMode(" > ComplexCommand");
        var match = _commandExecuting.SubCommands.FirstOrDefault(x =>
        {
            Logger.LogDebugMode($" > {x.ActionEvent} ==? {node.terminalEvent}");
            return x.ActionEvent == node.terminalEvent;
        });
        if (match is null) return;

        Logger.LogDebugMode($"  > Found {match.Node.name}");
        match.Action();

        // foreach (var command in AdvancedTerminal.Commands)
        // {
        //     Logger.LogDebugMode($"  > Checking {command.Node.name}");
        //
        //     if (command.IsSimpleCommand && command.ActionEvent == node.terminalEvent)
        //     {
        //         Logger.LogDebugMode("  > IsSimpleCommand, SKIP??");
        //         Logger.LogDebugMode($" > EXEC simple command: {command.Node.name}");
        //         var res = command.Action();
        //         node.displayText = res + AdvancedTerminal.EndOfMessage;
        //
        //         break;
        //     }
        //
        //     var match = command.SubCommands.FirstOrDefault(x => x.ActionEvent == node.terminalEvent);
        //     if (match == default) continue;
        //
        //     Logger.LogDebugMode($"  > Found {match.Node.name}");
        //     match.Action();
        //
        //     break;
        // }
    }
}
