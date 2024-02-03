using System;
using System.Collections.Generic;
using System.Linq;

namespace QualityCompany.Manager.ShipTerminal;

public class TerminalCommandBuilder
{
    public Func<string> Action;
    public bool IsSimpleCommand;
    public string ActionEvent;
    public readonly TerminalNode node = new();
    public readonly TerminalKeyword nodeKeyword = new();
    private readonly List<TerminalSubCommandBuilder> SubCommandsBuilders = new();
    public List<TerminalSubCommand> SubCommands = new();
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
        description = desc + AdvancedTerminal.EndOfMessage;

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
        SubCommandsBuilders.Add(builder);

        return this;
    }

    public TerminalCommandBuilder WithText(string text)
    {
        node.displayText = text + AdvancedTerminal.EndOfMessage;
        return this;
    }

    public TerminalCommandBuilder EnableConfirmDeny(string confirmMessage, string denyMessage)
    {
        ConfirmMessage = confirmMessage + AdvancedTerminal.EndOfMessage;
        DenyMessage = denyMessage + AdvancedTerminal.EndOfMessage;

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
            displayText = displayText + AdvancedTerminal.EndOfMessage,
            clearPreviousText = true
        };
        specialNodes.Add((specialNode, condition));

        return this;
    }

    internal TerminalKeyword[] Build(TerminalKeyword confirmKeyword, TerminalKeyword denyKeyword)
    {
        ActionEvent = $"{node.name}_event";

        if (SubCommandsBuilders.Any())
        {
            SubCommands = SubCommandsBuilders
                .Select(x => x.Build(node.name, confirmKeyword, denyKeyword))
                .ToList();
            nodeKeyword.isVerb = SubCommands.Any();
            nodeKeyword.compatibleNouns = SubCommands.Select(keyword => new CompatibleNoun
            {
                noun = keyword.Keyword,
                result = keyword.Node
            }).ToArray();
        }

        if (node.isConfirmationNode)
        {
            node.displayText += "[confirmOrDeny]";
            node.terminalOptions = new[]
            {
                new CompatibleNoun
                {
                    noun = confirmKeyword,
                    result = new TerminalNode
                    {
                        name = $"{node.name}_confirm",
                        displayText = ConfirmMessage + AdvancedTerminal.EndOfMessage,
                        clearPreviousText = true,
                        terminalEvent = $"{node.name}_event"
                    }
                },
                new CompatibleNoun
                {
                    noun = denyKeyword,
                    result = new TerminalNode
                    {
                        name = $"{node.name}_deny",
                        displayText = DenyMessage + AdvancedTerminal.EndOfMessage,
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
            .Concat(SubCommands.Select(x => x.Keyword))
            .ToArray();
    }
}