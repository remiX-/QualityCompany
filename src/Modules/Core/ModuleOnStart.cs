using System;

namespace QualityCompany.Modules.Core;

/// <summary>
/// The class-level Module attribute for module identification.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Module : Attribute
{
    /// <summary>
    /// <para>Whether this module should have a delayed start or not.</para>
    /// This delay is defined in this mod's config.
    /// </summary>
    public bool Delayed { get; set; }
}

/// <summary>
/// This method will trigger upon loading of modules at the start of the game.<br />This method MUST be static.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnStart : Attribute
{ }

/// <summary>
/// <para>Optional: this method will trigger when the module is loaded / instantiated. Triggers immediately after OnStart.</para>
/// Useful for attaching / subscribing to Mod API events.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnAttach : Attribute
{ }

/// <summary>
/// <para>Optional: this method will trigger when the module is unloaded (upon quitting).</para>
/// Useful for detaching / unsubscribing from Mod API events.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnDetach : Attribute
{ }