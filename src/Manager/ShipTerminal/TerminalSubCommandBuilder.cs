using BepInEx;
using System;
using System.Linq;

namespace QualityCompany.Manager.ShipTerminal;

public class TerminalSubCommandBuilder
{
    private readonly TerminalSubCommand _subCommand;

    public TerminalSubCommandBuilder(string name)
    {
        _subCommand = new TerminalSubCommand
        {
            Name = name,
            Keyword = new TerminalKeyword
            {
                name = name,
                word = name
            },
            Node = TerminalUtils.CreateNodeEmpty()
        };
    }

    public TerminalSubCommandBuilder WithDescription(string text)
    {
        _subCommand.Description = text;
        return this;
    }

    public TerminalSubCommandBuilder WithMessage(string text)
    {
        _subCommand.Message = text;
        return this;
    }

    public TerminalSubCommandBuilder EnableConfirmDeny(string confirmMessage = null, string denyMessage = null)
    {
        _subCommand.Node.isConfirmationNode = true;
        _subCommand.Node.overrideOptions = true;

        if (!confirmMessage.IsNullOrWhiteSpace()) _subCommand.ConfirmMessage = confirmMessage;
        if (!denyMessage.IsNullOrWhiteSpace()) _subCommand.DenyMessage = denyMessage;

        return this;
    }

    public TerminalSubCommandBuilder WithConditions(params string[] conditions)
    {
        _subCommand.Conditions = conditions.ToList();

        return this;
    }

    public TerminalSubCommandBuilder WithPreAction(Action action)
    {
        _subCommand.PreConditionAction = action;

        return this;
    }

    public TerminalSubCommandBuilder WithPreAction(Func<string, bool> action)
    {
        _subCommand.VariablePreAction = action;

        return this;
    }

    public TerminalSubCommandBuilder WithAction(Action action)
    {
        _subCommand.Action = action;

        return this;
    }

    public TerminalSubCommandBuilder WithInputMatch(string patternSuffix)
    {
        _subCommand.IsVariableCommand = true;
        _subCommand.VariableRegexMatchPattern = patternSuffix;

        return this;
    }

    internal TerminalSubCommand Build(string rootCommandName, TerminalKeyword confirmKeyword, TerminalKeyword denyKeyword)
    {
        _subCommand.Id = $"{rootCommandName}_{_subCommand.Name}";
        _subCommand.Node.name = _subCommand.Id;
        _subCommand.Node.displayText = _subCommand.Message + AdvancedTerminal.EndOfMessage;
        _subCommand.ActionEvent = $"{rootCommandName}_{_subCommand.Name}_event";
        // _subCommand.Node.terminalEvent = $"{rootCommandName}_{_subCommand.Name}_event";
        _subCommand.Keyword.name = _subCommand.Id;

        if (!_subCommand.Node.isConfirmationNode) return _subCommand;

        _subCommand.Node.displayText += "[confirmOrDeny]";
        _subCommand.Node.terminalOptions = new[]
        {
            new CompatibleNoun
            {
                noun = confirmKeyword,
                result = TerminalUtils.CreateConfirmNode($"{rootCommandName}_{_subCommand.Name}", _subCommand.ConfirmMessage, _subCommand.ActionEvent)
            },
            new CompatibleNoun
            {
                noun = denyKeyword,
                result = TerminalUtils.CreateDenyNode($"{rootCommandName}_{_subCommand.Name}", _subCommand.DenyMessage)
            }
        };

        return _subCommand;
    }
}