using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QualityCompany.Manager.ShipTerminal;

/// <summary>
/// This registry allows for registration of custom <see cref="QualityCompany"/> Advanced Terminal Commands.
/// </summary>
public class AdvancedTerminalRegistry
{
    internal static List<InternalCommand> Commands { get; } = new();

    private static readonly ACLogger _logger = new(nameof(AdvancedTerminalRegistry));

    /// <summary>
    /// Call this with your current assembly with <see cref="Assembly.GetExecutingAssembly" />
    /// </summary>
    /// <param name="assembly">The assembly to register <see cref="Module" /> attributes.</param>
    public static void Register(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        _logger.LogMessage($"Registering Terminal Commands in {assemblyName}");

        var assemblyCommandsCount = 0;
        foreach (var type in assembly.GetTypes())
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attribute = FindMethodInfoFor<TerminalCommand>(method);
                if (attribute is null) continue;

                assemblyCommandsCount++;

                var cmd = new InternalCommand
                {
                    Name = type.Name,
                    AssemblyName = assemblyName,
                    Run = method
                };
                Commands.Add(cmd);
            }
        }

        _logger.LogMessage($" > Found {assemblyCommandsCount} terminal commands");
    }

    private static T FindMethodInfoFor<T>(ICustomAttributeProvider member) where T : Attribute
    {
        var attributes = member.GetCustomAttributes(typeof(T), false);
        if (attributes.Length == 0) return null;
        return (T)attributes[0];
    }
}

internal class InternalCommand
{
    public string Name { get; set; }
    public string AssemblyName { get; set; }
    public MethodInfo Run { get; set; }
}