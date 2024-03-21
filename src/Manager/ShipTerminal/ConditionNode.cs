using System;

namespace QualityCompany.Manager.ShipTerminal;

internal class ConditionNode
{
    public TerminalNode Node { get; set; }

    public Func<bool> Condition { get; set; }

    public ConditionNode(TerminalNode node, Func<bool> condition)
    {
        Node = node;
        Condition = condition;
    }
}
