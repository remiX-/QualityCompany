using JetBrains.Annotations;
using UnityEngine;

namespace QualityCompany.Manager.ShipTerminal;

internal class TerminalUtils
{
    private static readonly string ConfirmMessage = "Confirmed!";
    private static readonly string DenyMessage = "Cancelled!";

    internal static TerminalNode CreateNodeEmpty(bool clear = true)
    {
        var node = ScriptableObject.CreateInstance<TerminalNode>();

        node.clearPreviousText = clear;

        return node;
    }

    internal static TerminalNode CreateNode(
        string name,
        string? text = null,
        string? terminalEvent = null,
        bool clear = true,
        bool confirmOrDeny = false)
    {
        var node = ScriptableObject.CreateInstance<TerminalNode>();

        node.name = $"qc:{name}";
        node.clearPreviousText = clear;
        node.displayText = (text ?? "Empty") + AdvancedTerminal.EndOfMessage;
        node.terminalEvent = terminalEvent is not null ? $"qc:{terminalEvent}" : $"qc:{name}_event";
        node.isConfirmationNode = confirmOrDeny;

        return node;
    }

    public static TerminalKeyword CreateKeyword(
        string name,
        string? text = null,
        string? terminalEvent = null,
        bool clear = true,
        bool confirmOrDeny = false)
    {
        var keyword = ScriptableObject.CreateInstance<TerminalKeyword>();

        // keyword.name = name;
        // keyword.word = word;
        // keyword.specialKeywordResult = node;

        return keyword;
    }

    internal static TerminalNode CreateConfirmNode(string name, string? confirmText, string? terminalEvent = null)
    {
        return CreateNode($"{name}_confirm", confirmText ?? ConfirmMessage, terminalEvent ?? $"{name}_event");
    }

    internal static TerminalNode CreateDenyNode(string name, string? denyText)
    {
        return CreateNode($"{name}_deny", denyText ?? ConfirmMessage);
    }
}
