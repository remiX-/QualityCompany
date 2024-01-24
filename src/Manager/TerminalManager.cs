using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedCompany.TerminalCommands;

namespace AdvancedCompany.Manager;

public class TerminalManager
{
    private static List<TerminalNode> Nodes;
    private static Terminal Terminal;

    public static TerminalKeyword terminalConfirmKeyword;
    public static TerminalKeyword terminalDenyKeyword;
    private static TerminalNode otherCategoryTerminalNode;

    public static Dictionary<string, Func<string>> _textReplacements = new();
    public static List<TerminalCommandBuilder> commands = new();

    private static List<ITerminalSubscriber> Subscribers = new();

    public static string EndOfMessage => "\n\n\n";

    public static void Sub(ITerminalSubscriber sub)
    {
        Subscribers.Add(sub);
    }

    public static void SetTerminalInstance(Terminal terminal)
    {
        Terminal = terminal;

        terminalConfirmKeyword = Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Confirm");
        terminalDenyKeyword = Terminal.terminalNodes.allKeywords.First(kw => kw.name == "Deny");
        otherCategoryTerminalNode = Terminal.terminalNodes.allKeywords.First(node => node.name == "Other").specialKeywordResult;
    }

    public static void AddGlobalTextReplacement(string text, Func<string> func)
    {
        _textReplacements.Add(text, func);
    }

    public static void AddCommand(TerminalCommandBuilder builder)
    {
        commands.Add(builder);
    }

    public static void ApplyToTerminal()
    {
        foreach (var sub in Subscribers)
        {
            Logger.LogDebug($"Running terminal subscribe: {sub.GetType()}");

            sub.Run();
        }

        foreach (var cmdBuilder in commands)
        {
            var keywords = cmdBuilder.Build();
            Terminal.terminalNodes.allKeywords = Terminal.terminalNodes.allKeywords.AddRangeToArray(keywords);

            if (cmdBuilder.description.IsNullOrWhiteSpace()) continue;

            otherCategoryTerminalNode.displayText = otherCategoryTerminalNode.displayText.Substring(0, otherCategoryTerminalNode.displayText.Length - 1) + $"{cmdBuilder.description}";
        }

        foreach (var kw in Terminal.terminalNodes.allKeywords)
        {
            Logger.LogDebug($"{kw.name} | {kw.word}");
        }
    }
}

public class TerminalCommandBuilder
{
    public Func<string> Action;
    public bool IsSimpleCommand;
    public string ActionEvent;
    public readonly TerminalNode node = new();
    public readonly TerminalKeyword nodeKeyword = new();
    public readonly List<TerminalSubCommand> nodeSubKeywords = new();
    public readonly Dictionary<string, Func<string>> textProcessPlaceholders = new();
    public readonly List<(TerminalNode node, Func<bool> condition)> specialNodes = new();

    public string description = "";

    public string ConfirmMessage = "Confirmed!";
    public string DenyMessage = "Cancelled!";

    public TerminalCommandBuilder(string name)
    {
        node.name = name;
        node.clearPreviousText = true;
        nodeKeyword = new TerminalKeyword
        {
            name = name,
            word = name,
            specialKeywordResult = node
        };
    }

    public TerminalCommandBuilder WithDescription(string desc)
    {
        description = desc + TerminalManager.EndOfMessage;

        return this;
    }

    public TerminalCommandBuilder WithAction(Action action)
    {
        IsSimpleCommand = true;
        Action = () =>
        {
            action();
            return node.displayText;
        };
        node.terminalEvent = $"{node.name}_event";

        return this;
    }

    public TerminalCommandBuilder WithAction(Func<string> action)
    {
        IsSimpleCommand = true;
        Action = action;
        // nodeKeyword.specialKeywordResult = node;

        return this;
    }

    public TerminalCommandBuilder WithSubCommand(TerminalSubCommandBuilder builder)
    {
        nodeSubKeywords.Add(builder.Build(node.name));

        return this;
    }

    public TerminalCommandBuilder WithText(string text)
    {
        node.displayText = text + TerminalManager.EndOfMessage;
        return this;
    }

    public TerminalCommandBuilder EnableConfirmDeny(string confirmMessage, string denyMessage)
    {
        ConfirmMessage = confirmMessage + TerminalManager.EndOfMessage;
        DenyMessage = denyMessage + TerminalManager.EndOfMessage;

        node.isConfirmationNode = true;
        node.overrideOptions = true;

        return this;
    }

    public TerminalCommandBuilder SetConfirmMessage(string message)
    {
        ConfirmMessage = message;

        return this;
    }

    public TerminalCommandBuilder AddTextReplacement(string replacementKey, Func<string> action)
    {
        textProcessPlaceholders.Add(replacementKey, action);

        return this;
    }

    /// <summary>
    /// A condition is an action that returns a bool specifying whether this command may proceed.
    /// Occurs before <see cref="Action"/> and after <see cref="TerminalSubCommand.PreConditionAction"/>.
    /// <example>Example: A simple example would be adding a condition to be able to run a command after 1PM in-game time.</example>
    /// </summary>
    /// <param name="name">The unique name for this overall command.</param>
    /// <param name="displayText">Text to be displayed if this condition resolves to <value>false</value></param>
    /// <param name="condition">The condition action to run that must resolve to <value>true</value> to pass this condition. If not, it will stop at this condition and display its displayText.</param>
    /// <returns></returns>
    public TerminalCommandBuilder WithCondition(string name, string displayText, Func<bool> condition)
    {
        var specialNode = new TerminalNode
        {
            name = name,
            displayText = displayText + TerminalManager.EndOfMessage,
            clearPreviousText = true
        };
        specialNodes.Add((specialNode, condition));

        return this;
    }

    internal TerminalKeyword[] Build()
    {
        ActionEvent = $"{node.name}_event";
        nodeKeyword.isVerb = nodeSubKeywords.Any();
        // nodeKeyword.isVerb = true;
        nodeKeyword.compatibleNouns = nodeSubKeywords.Select(keyword => new CompatibleNoun
        {
            noun = keyword.Keyword,
            result = keyword.Node
        }).ToArray();

        if (node.isConfirmationNode)
        {
            node.displayText += "[confirmOrDeny]";
            node.terminalOptions = new[]
            {
                new CompatibleNoun
                {
                    noun = TerminalManager.terminalConfirmKeyword,
                    result = new TerminalNode
                    {
                        name = $"{node.name}_confirm",
                        displayText = ConfirmMessage + TerminalManager.EndOfMessage,
                        clearPreviousText = true,
                        terminalEvent = $"{node.name}_event"
                    }
                },
                new CompatibleNoun
                {
                    noun = TerminalManager.terminalDenyKeyword,
                    result = new TerminalNode
                    {
                        name = $"{node.name}_deny",
                        displayText = DenyMessage + TerminalManager.EndOfMessage,
                        clearPreviousText = true
                    }
                }
            };
        }
        else
        {
            node.terminalEvent = $"{node.name}_event";
        }

        return new List<TerminalKeyword> { nodeKeyword }
            .Concat(nodeSubKeywords.Select(x => x.Keyword))
            .ToArray();
    }
}

public class TerminalSubCommandBuilder
{
    private readonly TerminalSubCommand subCommand;

    public TerminalSubCommandBuilder(string name)
    {
        subCommand = new TerminalSubCommand
        {
            Name = name,
            Keyword = new TerminalKeyword
            {
                name = name,
                word = name
            },
            Node = new TerminalNode
            {
                clearPreviousText = true
            }
        };
    }

    public TerminalSubCommandBuilder EnableConfirmDeny(string confirmMessage = null, string denyMessage = null)
    {
        subCommand.Node.isConfirmationNode = true;
        subCommand.Node.overrideOptions = true;

        if (!confirmMessage.IsNullOrWhiteSpace()) subCommand.ConfirmMessage = confirmMessage;
        if (!denyMessage.IsNullOrWhiteSpace()) subCommand.DenyMessage = denyMessage;

        return this;
    }

    public TerminalSubCommandBuilder WithMessage(string text)
    {
        subCommand.Message = text;
        return this;
    }

    public TerminalSubCommandBuilder WithConditions(params string[] conditions)
    {
        subCommand.Conditions = conditions.ToList();

        return this;
    }

    public TerminalSubCommandBuilder WithPreAction(Action action)
    {
        subCommand.PreConditionAction = action;

        return this;
    }

    public TerminalSubCommandBuilder WithPreAction(Func<string, bool> action)
    {
        subCommand.VariablePreAction = action;

        return this;
    }

    public TerminalSubCommandBuilder WithAction(Action action)
    {
        subCommand.Action = action;

        return this;
    }

    public TerminalSubCommandBuilder WithInputMatch(string patternSuffix)
    {
        subCommand.IsVariableCommand = true;
        subCommand.VariableRegexMatchPattern = patternSuffix;

        return this;
    }

    internal TerminalSubCommand Build(string rootCommandName)
    {
        subCommand.Id = $"{rootCommandName}_{subCommand.Name}";
        subCommand.Node.name = subCommand.Id;
        subCommand.Node.displayText = subCommand.Message + TerminalManager.EndOfMessage;
        subCommand.ActionEvent = $"{rootCommandName}_{subCommand.Name}_event";

        if (subCommand.VariableRegexMatchPattern is not null) subCommand.VariableRegexMatchPattern = $"^{rootCommandName} {subCommand.VariableRegexMatchPattern}";
        Logger.LogDebug($"Build: {subCommand.VariableRegexMatchPattern}");

        if (!subCommand.Node.isConfirmationNode) return subCommand;

        subCommand.Node.displayText += "[confirmOrDeny]";
        subCommand.Node.terminalOptions = new[]
        {
            new CompatibleNoun
            {
                noun = TerminalManager.terminalConfirmKeyword,
                result = new TerminalNode
                {
                    name = $"{rootCommandName}_{subCommand.Name}_confirm",
                    displayText = subCommand.ConfirmMessage + TerminalManager.EndOfMessage,
                    clearPreviousText = true,
                    terminalEvent = subCommand.ActionEvent
                }
            },
            new CompatibleNoun
            {
                noun = TerminalManager.terminalDenyKeyword,
                result = new TerminalNode
                {
                    name = $"{rootCommandName}_{subCommand.Name}_deny",
                    displayText = subCommand.DenyMessage + TerminalManager.EndOfMessage,
                    clearPreviousText = true
                }
            }
        };

        return subCommand;
    }
}

public class TerminalSubCommand
{
    public string Id;
    public string Name;
    public string Message;
    public TerminalKeyword Keyword;
    public TerminalNode Node;
    public List<string> Conditions;
    public Action PreConditionAction;
    public Func<string, bool> VariablePreAction;
    public string ActionEvent;
    public Action Action;

    public bool IsVariableCommand;
    public string VariableRegexMatchPattern;

    public string ConfirmMessage = "Confirmed!";
    public string DenyMessage = "Cancelled!";
}