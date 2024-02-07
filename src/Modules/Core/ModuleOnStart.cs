using System;

namespace QualityCompany.Modules.Core;

[AttributeUsage(AttributeTargets.Class)]
internal class Module : Attribute
{
    public bool Delayed { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
internal class ModuleOnStart : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
internal class ModuleOnAttach : Attribute
{ }

[AttributeUsage(AttributeTargets.Method)]
internal class ModuleOnDetach : Attribute
{ }