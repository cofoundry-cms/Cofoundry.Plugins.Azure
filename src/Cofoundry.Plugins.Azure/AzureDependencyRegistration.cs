using Cofoundry.Core.Configuration;
using Cofoundry.Core.DependencyInjection;
using Cofoundry.Domain.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Plugins.Azure
{
    public class AzureDependencyRegistration : IDependencyRegistration
    {
        public void Register(IContainerRegister container)
        {
            container
                .RegisterFactory<AzureBlobFileServiceSettings, ConfigurationSettingsFactory<AzureBlobFileServiceSettings>>()
                .RegisterFactory<AzureSettings, ConfigurationSettingsFactory<AzureSettings>>()
                ;

            if (container.Configuration.IsAzurePluginEnabled())
            {
                var delayedRegistrationOptions = RegistrationOptions.Override();

                container
                    .RegisterType<IFileStoreService, AzureBlobFileService>(delayedRegistrationOptions)
                    ;
            }
        }
    }
}
