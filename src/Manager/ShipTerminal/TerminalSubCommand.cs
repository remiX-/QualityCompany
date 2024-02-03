using System;
using System.Collections.Generic;

namespace QualityCompany.Manager.ShipTerminal;

public class TerminalSubCommand
{
    public string Id;
    public string Name;
    public string Message;
    public TerminalKeyword Keyword;
    public TerminalNode Node;
    public List<string> Conditions = new();
    public Action PreConditionAction;
    public Func<string, bool> VariablePreAction;
    public string ActionEvent;
    public Action Action;

    public bool IsVariableCommand;
    public string VariableRegexMatchPattern;

    public string ConfirmMessage = "Confirmed!";
    public string DenyMessage = "Cancelled!";
}