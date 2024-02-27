using System;
using System.Collections.Generic;
using System.Text;

namespace QualityCompany.Service;

internal class ServiceRegistry
{
    public static RandomizerService Randomizer = new ();
}
