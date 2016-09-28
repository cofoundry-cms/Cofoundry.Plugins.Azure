using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cofoundry.Core.Configuration;
using System.ComponentModel.DataAnnotations;

namespace Cofoundry.Plugins.Azure
{
    public class AzureBlobFileServiceSettings : PluginConfigurationSettingsBase
    {
        /// <summary>
        /// The connection string to use when accessing files in blob storage
        /// </summary>
        [Required]
        public string ConnectionString { get; set; }
    }
}
