using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QualityCompany.Modules.Core;

/// <summary>
/// <para>The Module Registry allows for registration of static and instanced-based classes that do specific kind of logic.<br />
/// This may not suit everyone's need but helps abide to the Single Responsibility Principle.</para>
/// <para>Examples include: any sort of HUD improvements, leveling system, etc.<br />See the ModuleExamplePlugin in the samples directory for a basic HUD example.</para>
/// </summary>
public class ModuleRegistry
{
    internal static List<InternalModule> Modules { get; } = new();

    private static readonly ModLogger Logger = new(nameof(ModuleRegistry));

    /// <summary>
    /// Call this with your current assembly with <see cref="Assembly.GetExecutingAssembly" />
    /// </summary>
    /// <param name="assembly">The assembly to register <see cref="Module" /> attributes.</param>
    public static void Register(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        Logger.LogDebug($"Registering Modules in {assemblyName}");

        foreach (var type in assembly.GetTypes())
        {
            var moduleAttribute = FindMethodInfoFor<Module>(type, true);
            if (moduleAttribute is null) continue;

            var module = new InternalModule
            {
                Name = type.Name,
                AssemblyName = assemblyName,
                DelayedStart = moduleAttribute.Delayed
            };

            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            Logger.LogDebug($" > {module.Name}");
            foreach (var method in methods)
            {
                var onLoad = FindMethodInfoFor<ModuleOnLoad>(method);
                if (onLoad is not null)
                {
                    Logger.LogDebug($"  > onLoad: {method.Name}");
                    module.OnLoad = method;
                    continue;
                }

                var onAttach = FindMethodInfoFor<ModuleOnAttach>(method);
                if (onAttach is not null)
                {
                    Logger.LogDebug($"  > OnAttach: {method.Name}");
                    module.OnAttach = method;
                    continue;
                }

                var onDetach = FindMethodInfoFor<ModuleOnDetach>(method);
                if (onDetach is not null)
                {
                    Logger.LogDebug($"  > OnDetach: {method.Name}");
                    module.OnDetach = method;
                }
            }

            Modules.Add(module);
        }

        Logger.LogDebug(" > Done!");
    }

    private static T FindMethodInfoFor<T>(ICustomAttributeProvider member, bool inherit = false) where T : Attribute
    {
        var attributes = member.GetCustomAttributes(typeof(T), inherit);
        if (attributes.Length == 0) return null;
        return (T)attributes[0];
    }
}

internal class InternalModule
{
    public string Name { get; set; }
    public string AssemblyName { get; set; }

    public object Instance { get; set; }

    public MethodInfo OnLoad { get; set; }
    public bool DelayedStart { get; set; }

    public MethodInfo OnAttach { get; set; }

    public MethodInfo OnDetach { get; set; }
}