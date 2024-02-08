using QualityCompany.Service;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace QualityCompany.Modules.Core;

public static class ModuleRegistry
{
    internal static List<InternalModule> Modules { get; } = new();

    private static readonly ACLogger _logger = new(nameof(ModuleRegistry));

    public static void Register(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;
        _logger.LogDebug($"Registering Modules in {assemblyName}");

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
            _logger.LogDebug($" > {module.Name}");
            foreach (var method in methods)
            {
                var onStart = FindMethodInfoFor<ModuleOnStart>(method);
                if (onStart is not null)
                {
                    _logger.LogDebug($"  > OnStart: {method.Name} | {method.ReturnType}");
                    module.OnStart = method;
                    continue;
                }

                var onAttach = FindMethodInfoFor<ModuleOnAttach>(method);
                if (onAttach is not null)
                {
                    _logger.LogDebug($"  > OnAttach: {method.Name}");
                    module.OnAttach = method;
                    continue;
                }

                var onDetach = FindMethodInfoFor<ModuleOnDetach>(method);
                if (onDetach is not null)
                {
                    _logger.LogDebug($"  > OnDetach: {method.Name}");
                    module.OnDetach = method;
                }
            }

            Modules.Add(module);
        }
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
    public MethodInfo OnStart { get; set; }
    public bool DelayedStart { get; set; }
    public MethodInfo OnAttach { get; set; }
    public MethodInfo OnDetach { get; set; }
}