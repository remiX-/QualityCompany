using System;
using System.Collections.Generic;

namespace QualityCompany.Manager.ShipTerminal;

internal class TerminalSubCommand
{
    internal string Id;
    internal string Name;
    internal string Description;
    internal string Message;
    internal TerminalKeyword Keyword;
    internal TerminalNode Node;
    internal List<string> Conditions = new();
    internal Func<string?>? PreConditionAction;
    internal Func<string, string?>? VariablePreAction;
    internal string ActionEvent;
    // internal Action? Action;
    internal Func<string>? ActionResult;
    // internal Action<string>? ActionWithInput;
    internal Func<string, string>? ActionInputResult;

    internal bool IsVariableCommand;
    internal string VariableRegexMatchPattern;

    internal string ConfirmMessage = "Confirmed!";
    internal string DenyMessage = "Cancelled!";
}