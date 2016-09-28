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
        public AzureSettings()
        {
            AutoRegisterDependencies = true;
        }

        /// <summary>
        /// Defaults to true. Indicates whether we want to auto-bootstrap azure services and run against the 
        /// azure infrastructure. Disable this in dev when you want to test locally.
        /// </summary>
        public bool AutoRegisterDependencies { get; set; }
    }
}
