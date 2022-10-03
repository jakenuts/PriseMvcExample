using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Prise.Plugin;

namespace DynamicRazorLib
{
    [Plugin(PluginType = typeof(IMvcPlugin))]
    [MvcPluginDescription(Description = "This feature will add the '/dynamicrazorlib' widget to the current MVC Host.")]
    public class DynamicRazorLibPlugin : IMvcPlugin
    {
        // Nothing to do here, just some feature discovery happening...
    }
}
