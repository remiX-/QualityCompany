using System;

namespace QualityCompany.Modules.Core;

/// <summary>
/// Test
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class Module : Attribute
{
    public bool Delayed { get; set; }
}

/// <summary>
/// ModuleOnStart
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnStart : Attribute
{ }

/// <summary>
/// ModuleOnAttach
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnAttach : Attribute
{ }

/// <summary>
/// ModuleOnDetach
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class ModuleOnDetach : Attribute
{ }