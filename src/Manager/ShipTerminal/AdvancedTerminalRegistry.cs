using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QualityCompany.Manager.ShipTerminal;

/// <summary>
/// This registry allows for registration of custom <see cref="QualityCompany"/> Advanced Terminal Commands.
/// </summary>
public class AdvancedTerminalRegistry
{
    internal static Dictionary<string, ModConfiguration> Commands { get; } = new();

    private static readonly ACLogger Logger = new(nameof(AdvancedTerminalRegistry));

    /// <summary>
    /// Call this with your current assembly with <see cref="Assembly.GetExecutingAssembly" />
    /// </summary>
    /// <param name="assembly">The assembly to register <see cref="Module" /> attributes.</param>
    /// <param name="createPrimaryCommand">Whether to create a primary command for the incoming assembly.</param>
    /// <param name="addToHelp">Whether your command should be added to the 'help' command.</param>
    /// <param name="commandName">Will default to the incoming assembly.</param>
    /// <param name="commandKeyword">Will default to abbreviated letters of the incoming assembly.</param>
    /// <param name="description">The description of the mod that will display when entering it into the terminal.</param>
    public static void Register(Assembly assembly, bool createPrimaryCommand = true, bool addToHelp = true, string commandName = null, string commandKeyword = null, string description = null)
    {
        var assemblyName = assembly.GetName().Name;

        if (Commands.ContainsKey(assemblyName))
        {
            Logger.LogError($"Assembly / Mod has already been registered: {assemblyName}, ignoring");
            return;
        }

        Logger.LogDebug($"Registering Terminal Commands in {assemblyName}");

        var commands = new List<InternalCommand>();

        foreach (var type in assembly.GetTypes())
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            foreach (var method in methods)
            {
                var attribute = FindMethodInfoFor<TerminalCommand>(method);
                if (attribute is null) continue;

                var cmd = new InternalCommand
                {
                    Name = type.Name,
                    Run = method
                };
                commands.Add(cmd);
            }
        }

        Commands.Add(assemblyName, new ModConfiguration
        {
            CreatePrimaryCommand = createPrimaryCommand,
            AddToHelp = addToHelp,
            PrimaryCommandName = commandName ?? assemblyName,
            PrimaryCommandKeyword = commandKeyword ?? GetAbbreviatedAssemblyName(assemblyName),
            Description = description,
            Commands = commands
        });

        Logger.LogDebug($" > Found {commands.Count} terminal commands");
    }

    private static T FindMethodInfoFor<T>(ICustomAttributeProvider member) where T : Attribute
    {
        var attributes = member.GetCustomAttributes(typeof(T), false);
        if (attributes.Length == 0) return null;
        return (T)attributes[0];
    }

    private static string GetAbbreviatedAssemblyName(string name)
    {
        return string
            .Concat(
                Regex
                    .Matches(name, "[A-Z]")
                    .Select(match => match.Value)
                )
            .ToLower();
    }
}

internal struct ModConfiguration
{
    public bool CreatePrimaryCommand { get; internal set; }
    public bool AddToHelp { get; internal set; }
    public string PrimaryCommandName { get; internal set; }
    public string PrimaryCommandKeyword { get; internal set; }
    public string Description { get; internal set; }
    public IReadOnlyList<InternalCommand> Commands { get; internal set; }
}

internal class InternalCommand
{
    public string Name { get; set; }
    // public string AssemblyName { get; set; }
    public MethodInfo Run { get; set; }
}
