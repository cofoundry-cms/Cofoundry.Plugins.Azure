using Cofoundry.Core.Configuration;
using Cofoundry.Core.DependencyInjection;
using Cofoundry.Domain.Data;
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

            if (AutoRegisterServices())
            {
                var delayedRegistrationOptions = new RegistrationOptions()
                {
                    ReplaceExisting = true,
                    InstanceScope = InstanceScope.PerWebRequest
                };

                container
                    .RegisterType<IFileStoreService, AzureBlobFileService>(delayedRegistrationOptions)
                    ;
            }
        }

        /// <summary>
        /// Indicates whether we want to auto-bootstrap azure services and run against 
        /// the azure infrastructure. Disable this in dev when you want to test locally.
        /// </summary>
        /// <remarks>
        /// Publicly exposed setting so it can be used in other Azure dependency registrations.
        /// </remarks>
        public static bool AutoRegisterServices()
        {
            return ConfigurationHelper.GetSettingAsBool("Cofoundry:Plugins:Azure:AutoRegisterServices", true);
        }
    }
}
