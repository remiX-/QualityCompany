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
    internal Action PreConditionAction;
    internal Func<string, bool> VariablePreAction;
    internal string ActionEvent;
    internal Action Action;

    internal bool IsVariableCommand;
    internal string VariableRegexMatchPattern;

    internal string ConfirmMessage = "Confirmed!";
    internal string DenyMessage = "Cancelled!";
}