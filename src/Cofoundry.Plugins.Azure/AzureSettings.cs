using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Core.Configuration;

namespace Cofoundry.Plugins.Azure
{
    public class AzureSettings : PluginConfigurationSettingsBase
    {
        /// <summary>
        /// Indicates whether the plugin should be disabled, which means services
        /// will not be boostrapped. Disable this in dev when you want to run using
        /// the standard non-cloud services.
        /// </summary>
        public bool Disabled { get; set; }
    }
}
