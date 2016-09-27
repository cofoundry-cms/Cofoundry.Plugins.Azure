using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Core.Configuration;

namespace Cofoundry.Plugins.Azure
{
    public class AzureBlobFileServiceSettings : PluginConfigurationSettingsBase
    {
        public string ConnectionString { get; set; }
    }
}
