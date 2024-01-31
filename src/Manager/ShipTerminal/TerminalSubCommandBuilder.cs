using BepInEx;
using System;
using System.Linq;

namespace AdvancedCompany.Manager.ShipTerminal;

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

    internal TerminalSubCommand Build(string rootCommandName, TerminalKeyword confirmKeyword, TerminalKeyword denyKeyword)
    {
        subCommand.Id = $"{rootCommandName}_{subCommand.Name}";
        subCommand.Node.name = subCommand.Id;
        subCommand.Node.displayText = subCommand.Message + AdvancedTerminal.EndOfMessage;
        subCommand.ActionEvent = $"{rootCommandName}_{subCommand.Name}_event";

        if (subCommand.VariableRegexMatchPattern is not null) subCommand.VariableRegexMatchPattern = $"^{rootCommandName} {subCommand.VariableRegexMatchPattern}";

        if (!subCommand.Node.isConfirmationNode) return subCommand;

        subCommand.Node.displayText += "[confirmOrDeny]";
        subCommand.Node.terminalOptions = new[]
        {
            new CompatibleNoun
            {
                noun = confirmKeyword,
                result = new TerminalNode
                {
                    name = $"{rootCommandName}_{subCommand.Name}_confirm",
                    displayText = subCommand.ConfirmMessage + AdvancedTerminal.EndOfMessage,
                    clearPreviousText = true,
                    terminalEvent = subCommand.ActionEvent
                }
            },
            new CompatibleNoun
            {
                noun = denyKeyword,
                result = new TerminalNode
                {
                    name = $"{rootCommandName}_{subCommand.Name}_deny",
                    displayText = subCommand.DenyMessage + AdvancedTerminal.EndOfMessage,
                    clearPreviousText = true
                }
            }
        };

        return subCommand;
    }
}