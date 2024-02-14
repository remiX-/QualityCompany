using System;

namespace QualityCompany.Manager.ShipTerminal;

/// <summary>
/// This method will be registered as a custom Terminal Command. These will be run at game start up time.<br />This method MUST be static.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TerminalCommand : Attribute
{ }